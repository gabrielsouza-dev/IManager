using IManager.Web.Application.Interfaces;
using IManager.Web.Domain.Entities.Companies;
using IManager.Web.Domain.Entities.Payrolls;
using IManager.Web.Domain.Entities.TimeTrackings;
using IManager.Web.Domain.Interfaces.Persistence;
using IManager.Web.Domain.Interfaces.Repositories;
using IManager.Web.Presentation.Controllers;
using IManager.Web.Presentation.ViewModels.Payrolls;
using IManager.Web.Presentation.ViewModels.TimeEntries;
using IManager.Web.Shared;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace IManager.Web.Application.Services;

public class PayrollQueryService : IPayrollQueryService
{
    private readonly ITimeEntriesRepository _timeEntryRepository;
    private readonly IPayslipsRepository _payslipRepository;

    public PayrollQueryService(ITimeEntriesRepository timeEntryRepository, IPayslipsRepository payslipRepository)
    {
        _timeEntryRepository = timeEntryRepository;
        _payslipRepository = payslipRepository;
    }

    public async Task<IEnumerable<DateOnly>> GetCompetencesAsync(Guid companyId)
    {
        if (companyId == Guid.Empty)
            return Enumerable.Empty<DateOnly>();

        var model = await _timeEntryRepository.GetPayrollCompetencesAsync(companyId);
        return model ?? Enumerable.Empty<DateOnly>();
    }   

    public async Task<PagedResult<PayrollViewModel>> GetPendingPayrollPagedAsync(Guid companyId, DateOnly competenceDate, int page, int pageSize)
    {
        if (companyId == Guid.Empty)
            return new PagedResult<PayrollViewModel>() { Page = page, PageSize = pageSize };

        var totalCount = await _timeEntryRepository.CountPendingPayrollsAsync(companyId, competenceDate);

        var items = await _timeEntryRepository.GetPendingPayrollsPagedAsync(companyId, competenceDate, page, pageSize);

        return new PagedResult<PayrollViewModel>()
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PagedResult<ProcessedPayrollViewModel>> GetProcessedPayrollPagedAsync(Guid companyId, DateOnly competenceDate, int page, int pageSize)
    {
        if (companyId == Guid.Empty)
            return new PagedResult<ProcessedPayrollViewModel>() { Page = page, PageSize = pageSize };

        Func<IQueryable<Payslip>, IQueryable<Payslip>> query = q => q.Where(p =>
            p.Payroll.CompanyId == companyId &&
            p.Payroll.Competence.Month == competenceDate.Month &&
            p.Payroll.Competence.Year == competenceDate.Year);

        var totalCount = await _payslipRepository.CountAsync(query);

        var items = await _payslipRepository.GetProcessedPayrollsPagedAsync(query, page, pageSize);

        return new PagedResult<ProcessedPayrollViewModel>()
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

}