using IManager.Web.Application.Interfaces;
using IManager.Web.Domain.Entities.Companies;
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
    private readonly ITimeEntryRepository _timeEntryRepository;

    public PayrollQueryService(ITimeEntryRepository timeEntryRepository)
    {
        _timeEntryRepository = timeEntryRepository;
    }

    public async Task<IEnumerable<DateOnly>> GetPendingCompetencesAsync(Guid companyId)
    {
        if (companyId == Guid.Empty)
            return Enumerable.Empty<DateOnly>();

        var model = await _timeEntryRepository.GetPendingPayrollCompetencesAsync(companyId);
        return model ?? Enumerable.Empty<DateOnly>();
    }   

    public async Task<IEnumerable<PayrollViewModel>> GetPendingPayrollAsync(Guid companyId, DateOnly competenceDate)
    {
        if (companyId == Guid.Empty)
            return Enumerable.Empty<PayrollViewModel>();

        var model = await _timeEntryRepository.GetPendingPayrollsAsync(companyId, competenceDate);
        return model ?? Enumerable.Empty<PayrollViewModel>();
    }

    public async Task<IEnumerable<PayrollViewModel>> GetProcessedPayrollAsync(Guid companyId, DateOnly competenceDate)
    {
        if (companyId == Guid.Empty)
            return Enumerable.Empty<PayrollViewModel>();

        var model = await _timeEntryRepository.GetProcessedPayrollsAsync(companyId, competenceDate);
        return model ?? Enumerable.Empty<PayrollViewModel>();
    }
}
