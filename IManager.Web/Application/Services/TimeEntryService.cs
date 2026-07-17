using IManager.Web.Application.Interfaces;
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
    private readonly ITimeEntryRepository _timeEntryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TimeEntryService> _logger;

    public TimeEntryService(ITimeEntryRepository timeEntryRepository, IUnitOfWork unitOfWork, ILogger<TimeEntryService> logger)
    {
        _timeEntryRepository = timeEntryRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IEnumerable<TimeEntryAudict>> GetAllAudictViewModel(Guid companyId, DateOnly date)
    {
        if (companyId == Guid.Empty)
            return Enumerable.Empty<TimeEntryAudict>();

        var model = await _timeEntryRepository.GetAudictoryHistory(companyId, date);
        return model;
    }

    public async Task<IEnumerable<TimeEntryPending>> GetAllPendingViewModel(Guid companyId, DateOnly date   )
    {
        if(companyId == Guid.Empty)
            return Enumerable.Empty<TimeEntryPending>();

        var model = await _timeEntryRepository.GetAllPendingViewModel(companyId, date);
        return model;
    }

    public async Task<IEnumerable<DateOnly>> GetPendentDates(Guid companyId)
    {
        if(companyId == Guid.Empty)
            return Enumerable.Empty<DateOnly>();

        var model = await _timeEntryRepository.GetPendentDateTimeEntry(companyId);
        return model;
    }

    public async Task<Result> ManageApprove(Guid id, Guid audictorId)
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
            await _timeEntryRepository.UpdateAsync(timeEntry, audictorId);

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

    public async Task<Result> ManageReject(Guid id, string? comment, Guid audictorId)
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
            await _timeEntryRepository.UpdateAsync(timeEntry, audictorId);

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
