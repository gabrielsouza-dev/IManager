using IManager.Web.Application.Services;
using IManager.Web.Data.Persistence;
using IManager.Web.Domain.Entities.TimeTrackings;
using IManager.Web.Domain.Enums;
using IManager.Web.Domain.Interfaces.Repositories;
using IManager.Web.Presentation.Requests;
using IManager.Web.Presentation.ViewModels.Payrolls;
using IManager.Web.Presentation.ViewModels.TimeEntries;
using IManager.Web.Presentation.ViewModels.Users;
using IManager.Web.Shared.Helpers;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace IManager.Web.Data.Repositories;

public class TimeEntryRepository : Repository<TimeEntry>, ITimeEntryRepository
{
    public TimeEntryRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<TimeEntryAudict>> GetAudictoryHistory(Guid companyId, DateOnly date)
    {
        return await _dbSet
            .Where(t => 
                t.Employee.CompanyId == companyId && 
                t.ParentId != null && 
                t.Status != TimeEntryStatus.Pending &&
                t.Date.Month == date.Month &&
                t.Date.Year == date.Year)
            .Select(t => new TimeEntryAudict
            {
                Id = t.Id,
                EmployeeName = t.Employee.FullName,
                Date = t.Date,
                OriginalChecks = t.Parent!.Checks,
                NewChecks = t.Checks,
                CreatedAt = t.CreatedAt,
                LastModifierName = t.LastModifier!.UserProfile!.FullName ?? "System",
                LastModified = t.LastModified ?? DateTime.MinValue,
                Status = t.Status
            })
            .OrderByDescending(t => t.LastModified)
            .ToListAsync();
    }

    public async Task<IEnumerable<DateOnly>> GetPendentDateTimeEntry(Guid companyId)
    {
        var result = await _dbSet
            .Where(t =>
                t.Employee.CompanyId == companyId &&
                t.ParentId != null)
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

    public async Task<IEnumerable<TimeEntryPending>> GetAllPendingViewModel(Guid companyId, DateOnly competenceDate)
    {
        var result = await _dbSet
            .AsNoTracking()
            .Where(t => 
                (t.Employee.CompanyId == companyId) && 
                (t.Status == TimeEntryStatus.Pending) && 
                t.Date.Month == competenceDate.Month && 
                t.Date.Year == competenceDate.Year)
            .Select(t => new TimeEntryPending()
            {
                Id = t.Id,
                AdjustmentReason = t.AdjustmentReason,
                OriginalChecks = t.Parent!.Checks.Select(c => new TimeCheckViewModel
                {
                    Id = c.Id,
                    Timestamp = c.Timestamp,
                    TimeZoneId = c.TimeZoneId,
                    TimestampLocal = c.TimestampLocal
                }).ToList(),
                NewChecks = t.Checks.Select(c => new TimeCheckViewModel
                {
                    Id = c.Id,
                    Timestamp = c.Timestamp,
                    TimeZoneId = c.TimeZoneId,
                    TimestampLocal = c.TimestampLocal
                }).ToList(),
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

    public async Task<IEnumerable<PayrollViewModel>> GetPendingPayrollsAsync(Guid companyId, DateOnly competenceDate)
    {
        var result = await _dbSet
            .AsNoTracking()
            .Where(t => 
                t.Employee.CompanyId == companyId && 
                t.IsCurrent == true && 
                t.PayslipId == null &&
                t.Status == TimeEntryStatus.Accepted &&
                t.Date.Month == competenceDate.Month && 
                t.Date.Year == competenceDate.Year)
            .Select(g => new
            {
                CompanyId = g.Employee.CompanyId,
                EmployeeId = g.EmployeeId,
                EmployeeName = g.Employee.FullName,
                CompanyName = g.Employee.Company.TradeName,
                Competence = new DateOnly(g.Date.Year, g.Date.Month, 1),
                PeriodStart = new DateOnly(g.Date.Year, g.Date.Month, 1),
                PeriodEnd = new DateOnly(g.Date.Year, g.Date.Month, DateTime.DaysInMonth(g.Date.Year, g.Date.Month)),
                TimeEntry = new TimeEntrySummaryViewModel()
                {
                    Id = g.Id,
                    IsConsistent = g.Checks.IsConsistent(),
                    HoursWorked = g.Checks.GetHoursWorked(),
                }
            })
            .ToListAsync();

        var summary = result
            .GroupBy(r => r.EmployeeId)
            .Select(g => 
            {
                var first = g.First();

                return new PayrollViewModel()
                {
                    CompanyId = first.CompanyId,
                    CompanyName = first.CompanyName,
                    EmployeeId = g.Key,
                    EmployeeName = first.EmployeeName,

                    Competence = first.Competence,
                    PeriodStart = first.PeriodStart,
                    PeriodEnd = first.PeriodEnd,

                    TimeEntry = new TimeEntrySummaryViewModel
                    {
                        Id = first.TimeEntry.Id,
                        DaysWorked = g.Count(),
                        HoursWorked = TimeSpan.FromTicks(
                            g.Sum(x => x.TimeEntry.HoursWorked.Ticks)),

                        IsConsistent = g.All(x => x.TimeEntry.IsConsistent)
                    }
                };
            })
            .ToList();

        return summary;
    }

    public async Task<IEnumerable<DateOnly>> GetPendingPayrollCompetencesAsync(Guid companyId)
    {
        var result = await _dbSet
            .AsNoTracking()
            .Where(t =>
                t.Employee.CompanyId == companyId &&
                t.IsCurrent == true &&
                t.PayslipId == null &&
                t.Status == TimeEntryStatus.Accepted)
            .GroupBy(t => new { t.Date.Year, t.Date.Month })
            .Select(g => new DateOnly(g.Key.Year, g.Key.Month, 1))
            .OrderByDescending(x => x.Year)
            .ThenByDescending(x => x.Month)
            .ToListAsync();

        return result;
    }

    public async Task<IEnumerable<PayrollViewModel>> GetProcessedPayrollsAsync(Guid companyId, DateOnly competenceDate)
    {
        var result = await _dbSet
            .AsNoTracking()
            .Where(t =>
                t.Employee.CompanyId == companyId &&
                t.IsCurrent == true &&
                t.PayslipId != null &&
                t.Status == TimeEntryStatus.Accepted &&
                t.Date.Month == competenceDate.Month &&
                t.Date.Year == competenceDate.Year)
            .Select(g => new
            {
                CompanyId = g.Employee.CompanyId,
                EmployeeId = g.EmployeeId,
                EmployeeName = g.Employee.FullName,
                CompanyName = g.Employee.Company.TradeName,
                Competence = new DateOnly(g.Date.Year, g.Date.Month, 1),
                PeriodStart = new DateOnly(g.Date.Year, g.Date.Month, 1),
                PeriodEnd = new DateOnly(g.Date.Year, g.Date.Month, DateTime.DaysInMonth(g.Date.Year, g.Date.Month)),
                TimeEntry = new TimeEntrySummaryViewModel()
                {
                    Id = g.Id,
                    IsConsistent = g.Checks.IsConsistent(),
                    HoursWorked = g.Checks.GetHoursWorked(),
                }
            })
            .ToListAsync();

        var summary = result
            .GroupBy(r => r.EmployeeId)
            .Select(g =>
            {
                var first = g.First();

                return new PayrollViewModel()
                {
                    CompanyId = first.CompanyId,
                    CompanyName = first.CompanyName,

                    EmployeeId = g.Key,
                    EmployeeName = first.EmployeeName,

                    Competence = first.Competence,
                    PeriodStart = first.PeriodStart,
                    PeriodEnd = first.PeriodEnd,

                    TimeEntry = new TimeEntrySummaryViewModel
                    {
                        Id = first.TimeEntry.Id,
                        DaysWorked = g.Count(),
                        HoursWorked = TimeSpan.FromTicks(
                            g.Sum(x => x.TimeEntry.HoursWorked.Ticks)),

                        IsConsistent = g.All(x => x.TimeEntry.IsConsistent)
                    }
                };
            })
            .ToList();

        return summary;
    }

    public async Task<IEnumerable<ProcessPayrollSummary>> GetProcessPayrollSummariesAsync(Guid companyId, ProcessPayrollRequest request)
    {
        var start = new DateOnly(
            request.CompetenceDate.Year,
            request.CompetenceDate.Month,
            1);
        var end = start.AddMonths(1);
          
        var result = await _dbSet
            .AsNoTracking()
            .Where(t => t.Employee.CompanyId == companyId &&
                request.EmployeeIds.Contains(t.EmployeeId) &&
                t.Date >= start &&
                t.Date < end &&
                t.IsCurrent &&
                t.Status == TimeEntryStatus.Accepted) 
            .Select(t =>
                    new ProcessPayrollSummary(
                        EmployeId: t.EmployeeId,
                        EmployeName: t.Employee.FullName,
                        Date: t.Date,
                        IsConcistent: t.Checks.IsConsistent(),
                        CheckCount: t.Checks.Count)
            ).ToListAsync();

        return result;
    }
}