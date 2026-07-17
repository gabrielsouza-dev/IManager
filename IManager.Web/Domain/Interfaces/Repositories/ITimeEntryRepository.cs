using IManager.Web.Application.Services;
using IManager.Web.Domain.Entities.TimeTrackings;
using IManager.Web.Presentation.ViewModels.Payrolls;
using IManager.Web.Presentation.ViewModels.TimeEntries;

namespace IManager.Web.Domain.Interfaces.Repositories;

public interface ITimeEntryRepository : IRepository<TimeEntry>
{
    Task<IEnumerable<TimeEntryPending>> GetAllPendingViewModel(Guid companyId, DateOnly date);
    Task<IEnumerable<TimeEntryAudict>> GetAudictoryHistory(Guid companyId, DateOnly date);
    Task<IEnumerable<DateOnly>> GetPendentDateTimeEntry(Guid companyId);
    Task<IEnumerable<DateOnly>> GetPendingPayrollCompetencesAsync(Guid companyId);
    Task<IEnumerable<PayrollViewModel>> GetPendingPayrollsAsync(Guid companyId, DateOnly competenceDate);
    Task<IEnumerable<PayrollViewModel>> GetProcessedPayrollsAsync(Guid companyId, DateOnly competenceDate);
}