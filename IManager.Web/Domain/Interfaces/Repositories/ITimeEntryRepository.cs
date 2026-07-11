using IManager.Web.Domain.Entities.TimeTrackings;
using IManager.Web.Presentation.ViewModels.TimeEntries;

namespace IManager.Web.Domain.Interfaces.Repositories;

public interface ITimeEntryRepository : IRepository<TimeEntry>
{
    Task<IEnumerable<TimeEntryPending>> GetAllPendingViewModel(Guid companyId, DateOnly date);
    Task<IEnumerable<TimeEntry>> GetAudictoryHistory(Guid companyId);
    Task<IEnumerable<DateOnly>> GetPendentDateTimeEntry(Guid companyId);
}