using IManager.Web.Application.Interfaces;
using IManager.Web.Domain.Consts;
using IManager.Web.Domain.Entities.TimeTrackings;
using IManager.Web.Domain.Enums;
using IManager.Web.Domain.Interfaces.Persistence;
using IManager.Web.Domain.Interfaces.Repositories;
using IManager.Web.Presentation.ViewModels.Payrolls;
using IManager.Web.Presentation.ViewModels.TimeEntries;
using IManager.Web.Shared;
using Microsoft.EntityFrameworkCore;

namespace IManager.Web.Application.Services;
public class TimeEntryService : ITimeEntryService
{
    private readonly ITimeEntriesRepository _timeEntryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TimeEntryService> _logger;

    public TimeEntryService(ITimeEntriesRepository timeEntryRepository, IUnitOfWork unitOfWork, ILogger<TimeEntryService> logger)
    {
        _timeEntryRepository = timeEntryRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PagedResult<TimeEntryAudit>> GetAuditPagedAsync(Guid companyId, DateOnly date, int page, int pageSize)
    {
        if (companyId == Guid.Empty)
            return new PagedResult<TimeEntryAudit>() { Page = page, PageSize = pageSize };

        Func<IQueryable<TimeEntry>, IQueryable<TimeEntry>> query = q => q.Where(t =>
            t.Employee.CompanyId == companyId &&
            t.Employee.Role == Role.User &&
            t.ParentId != null &&
            t.Status != TimeEntryStatus.Pending &&
            t.Date.Month == date.Month &&
            t.Date.Year == date.Year);

        var totalCount = await _timeEntryRepository.CountAsync(query);

        var items = await _timeEntryRepository.GetAuditHistoryPagedAsync(query, page, pageSize);

        return new PagedResult<TimeEntryAudit>()
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PagedResult<TimeEntryPending>> GetPendingPagedAsync(Guid companyId, DateOnly date, int page, int pageSize)
    {
        if (companyId == Guid.Empty)
            return new PagedResult<TimeEntryPending>() { Page = page, PageSize = pageSize };

        Func<IQueryable<TimeEntry>, IQueryable<TimeEntry>> query = q => q.Where(t =>
            t.Employee.CompanyId == companyId &&
            t.Status == TimeEntryStatus.Pending &&
            t.Employee.Role == Role.User &&
            t.Date.Month == date.Month &&
            t.Date.Year == date.Year);

        var totalCount = await _timeEntryRepository.CountAsync(query);

        var items = await _timeEntryRepository.GetPendingPagedAsync(query, page, pageSize);

        return new PagedResult<TimeEntryPending>()
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<IEnumerable<DateOnly>> GetPendingDates(Guid companyId)
    {
        if(companyId == Guid.Empty)
            return Enumerable.Empty<DateOnly>();

        var model = await _timeEntryRepository.GetPendingDateTimeEntry(companyId);
        return model;
    }

    public async Task<Result> ManageApprove(Guid id, Guid auditorId)
    {
        if(id == Guid.Empty)
            return Result.Fail("Id invalido.");

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var timeEntry = await GetTimeEntryOrThrow(id);

            timeEntry.Parent!.IsCurrent = false;
            await _timeEntryRepository.UpdateAsync(timeEntry.Parent);

            timeEntry.IsCurrent = true;
            timeEntry.Status = TimeEntryStatus.Accepted;
            await _timeEntryRepository.UpdateAsync(timeEntry, auditorId);

            await _unitOfWork.CommitAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Erro ao aprovar o lançamento de ponto.");

            return Result.Fail("Erro ao aprovar o lançamento de ponto.");
        }
    }

    public async Task<Result> ManageReject(Guid id, string? comment, Guid auditorId)
    {
        if (id == Guid.Empty)
            return Result.Fail("Id invalido.");

        if (string.IsNullOrWhiteSpace(comment))
            return Result.Fail("Comentário inválido.");

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var timeEntry = await GetTimeEntryOrThrow(id);

            timeEntry.IsCurrent = false;
            timeEntry.Status = TimeEntryStatus.Rejected;
            timeEntry.RejectionReason = comment;
            await _timeEntryRepository.UpdateAsync(timeEntry, auditorId);

            timeEntry.Parent!.IsCurrent = true;
            await _timeEntryRepository.UpdateAsync(timeEntry.Parent);

            await _unitOfWork.CommitAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Erro ao rejeitar o lançamento de ponto.");

            return Result.Fail("Erro ao rejeitar o lançamento de ponto.");
        }
    }

    private async Task<TimeEntry> GetTimeEntryOrThrow(Guid id)
    {
        var timeEntry = await _timeEntryRepository.GetByIdAsync(id, q => q.Include(t => t.Parent));
        if (timeEntry == null)
            throw new InvalidOperationException("Lançamento de ponto não encontrado.");

        if (timeEntry.Parent == null)
            throw new InvalidOperationException("Lançamento de ponto pai não encontrado.");

        return timeEntry;
    }
}
