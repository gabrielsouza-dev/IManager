using IManager.Web.Presentation.ViewModels.Payrolls;
using IManager.Web.Presentation.ViewModels.TimeEntries;
using IManager.Web.Shared;

namespace IManager.Web.Application.Interfaces;

public interface ITimeEntryService
{
    Task<PagedResult<TimeEntryAudit>> GetAuditPagedAsync(Guid companyId, DateOnly date, int page, int pageSize);
    Task<PagedResult<TimeEntryPending>> GetPendingPagedAsync(Guid companyId, DateOnly date, int page, int pageSize);
    Task<IEnumerable<DateOnly>> GetPendingDates(Guid companyId);
    Task<Result> ManageApprove(Guid id, Guid auditorId);
    Task<Result> ManageReject(Guid id, string? comment, Guid auditorId);
}