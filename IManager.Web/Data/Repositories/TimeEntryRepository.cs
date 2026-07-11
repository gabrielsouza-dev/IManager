using IManager.Web.Data.Persistence;
using IManager.Web.Domain.Entities.TimeTrackings;
using IManager.Web.Domain.Enums;
using IManager.Web.Domain.Interfaces.Repositories;
using IManager.Web.Presentation.ViewModels.TimeEntries;
using IManager.Web.Presentation.ViewModels.Users;
using Microsoft.EntityFrameworkCore;
namespace IManager.Web.Data.Repositories;

public class TimeEntryRepository : Repository<TimeEntry>, ITimeEntryRepository
{
    public TimeEntryRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<TimeEntry>> GetAudictoryHistory(Guid companyId)
    {
        return await _dbSet
            .Where(t => 
                t.Employee.CompanyId == companyId && 
                t.ParentId != null)
            .Select(t => t)
            .OrderByDescending(t => t.LastModified)
            .ToListAsync();
    }

    public async Task<IEnumerable<DateOnly>> GetPendentDateTimeEntry(Guid companyId)
    {
        var result = await _dbSet
            .Where(t =>
                t.Employee.CompanyId == companyId &&
                t.Status == TimeEntryStatus.Pending)
            .Select(t => new
            {
                t.Date.Year,
                t.Date.Month
            })
            .Distinct()
            .ToListAsync();

        return result
            .Select(x => new DateOnly(x.Year, x.Month, 1))
            .OrderByDescending(d => d)
            .ToList();
    }

    public async Task<IEnumerable<TimeEntryPending>> GetAllPendingViewModel(Guid companyId, DateOnly date)
    {
        var result = await _dbSet
            .AsNoTracking()
            .Where(t => 
                (t.Employee.CompanyId == companyId) && 
                (t.Status == TimeEntryStatus.Pending) && 
                t.Date.Month == date.Month && 
                t.Date.Year == date.Year)
            .Select(t => new TimeEntryPending()
            {
                Id = t.Id,
                AdjustmentReason = t.AdjustmentReason,
                OriginalChecks = t.Parent!.Checks,
                NewChecks = t.Checks,
                CreatedAt = t.CreatedAt,
                Date = t.Date,
                EmployeeId = t.EmployeeId,
                EmployeeName = t.Employee.FullName,
                IsCurrent = t.IsCurrent,
                ParentId = t.ParentId,
                Status = t.Status
            }).ToListAsync();

        return result;
    }
}