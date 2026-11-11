using IManager.Web.Application.Services;
using IManager.Web.Domain.Entities.TimeTrackings;
using IManager.Web.Presentation.Requests;
using IManager.Web.Presentation.ViewModels.Payrolls;
using IManager.Web.Presentation.ViewModels.TimeEntries;

namespace IManager.Web.Domain.Interfaces.Repositories;

public interface ITimeEntriesRepository : IRepository<TimeEntry>
{
    Task<IEnumerable<TimeEntryPending>> GetPendingPagedAsync(Func<IQueryable<TimeEntry>, IQueryable<TimeEntry>> query, int page, int pageSize);
    Task<IEnumerable<TimeEntryAudit>> GetAuditHistoryPagedAsync(Func<IQueryable<TimeEntry>, IQueryable<TimeEntry>> query, int page, int pageSize);
    Task<IEnumerable<DateOnly>> GetPendingDateTimeEntry(Guid companyId);
    Task<IEnumerable<DateOnly>> GetPayrollCompetencesAsync(Guid companyId);
    Task<int> CountPendingPayrollsAsync(Guid companyId, DateOnly competenceDate);
    Task<IEnumerable<PayrollViewModel>> GetPendingPayrollsPagedAsync(Guid companyId, DateOnly competenceDate, int page, int pageSize);
    Task<IEnumerable<ProcessPayrollSummary>> GetProcessPayrollSummariesAsync(Guid companyId, ProcessPayrollRequest request);
    Task<List<TimeEntry>> GetTimeEntriesByCompetence(Guid companyId, Guid employeeId, DateOnly competenceDate);
}