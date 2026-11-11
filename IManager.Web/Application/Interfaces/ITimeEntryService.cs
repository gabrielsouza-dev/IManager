using IManager.Web.Presentation.ViewModels.Payrolls;
using IManager.Web.Presentation.ViewModels.TimeEntries;
using IManager.Web.Shared;

namespace IManager.Web.Application.Interfaces;

public interface ITimeEntryService
{
    Task<IEnumerable<TimeEntryAudit>> GetAllAudictViewModel(Guid companyId, DateOnly date);
    Task<IEnumerable<TimeEntryPending>> GetAllPendingViewModel(Guid companyId, DateOnly date);
    Task<IEnumerable<DateOnly>> GetPendentDates(Guid companyId);
    Task<Result> ManageApprove(Guid id, Guid audictorId);
    Task<Result> ManageReject(Guid id, string? comment, Guid audictorId);
}