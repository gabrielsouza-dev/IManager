using IManager.Web.Application.Services;
using IManager.Web.Data.Persistence;
using IManager.Web.Domain.Consts;
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
using Org.BouncyCastle.Asn1.Ocsp;
namespace IManager.Web.Data.Repositories;

public class TimeEntriesRepository : Repository<TimeEntry>, ITimeEntriesRepository
{
    public TimeEntriesRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<TimeEntryAudit>> GetAuditHistoryPagedAsync(Func<IQueryable<TimeEntry>, IQueryable<TimeEntry>> query, int page, int pageSize)
    {
        IQueryable<TimeEntry> dbset = _dbSet.AsNoTracking();

        if (query != null)
            dbset = query(dbset);

        return await dbset
            .OrderByDescending(t => t.LastModified)
            .ThenBy(t => t.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TimeEntryAudit
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
            .ToListAsync();
    }

    public async Task<IEnumerable<DateOnly>> GetPendingDateTimeEntry(Guid companyId)
    {
        var result = await _dbSet
            .Where(t =>
                t.Employee.CompanyId == companyId &&
                t.Employee.Role == Role.User &&
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

    public async Task<IEnumerable<TimeEntryPending>> GetPendingPagedAsync(Func<IQueryable<TimeEntry>, IQueryable<TimeEntry>> query, int page, int pageSize)
    {
        IQueryable<TimeEntry> dbset = _dbSet.AsNoTracking();

        if (query != null)
            dbset = query(dbset);

        var result = await dbset
            .OrderBy(t => t.Date)
            .ThenBy(t => t.Employee.FullName)
            .ThenBy(t => t.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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

    public async Task<int> CountPendingPayrollsAsync(Guid companyId, DateOnly competenceDate)
    {
        return await PendingPayrollsQuery(companyId, competenceDate)
            .Select(t => t.EmployeeId)
            .Distinct()
            .CountAsync();
    }

    public async Task<IEnumerable<PayrollViewModel>> GetPendingPayrollsPagedAsync(Guid companyId, DateOnly competenceDate, int page, int pageSize)
    {
        // Pagina por funcionário (cada linha da tabela é um funcionário), não por TimeEntry
        var employeeIds = await PendingPayrollsQuery(companyId, competenceDate)
            .Select(t => new { t.EmployeeId, t.Employee.FullName })
            .Distinct()
            .OrderBy(e => e.FullName)
            .ThenBy(e => e.EmployeeId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => e.EmployeeId)
            .ToListAsync();

        if (employeeIds.Count == 0)
            return Enumerable.Empty<PayrollViewModel>();

        var result = await PendingPayrollsQuery(companyId, competenceDate)
            .Where(t => employeeIds.Contains(t.EmployeeId))
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
            .OrderBy(p => p.EmployeeName)
            .ThenBy(p => p.EmployeeId)
            .ToList();

        return summary;
    }

    private IQueryable<TimeEntry> PendingPayrollsQuery(Guid companyId, DateOnly competenceDate)
    {
        return _dbSet
            .AsNoTracking()
            .Where(t =>
                t.Employee.CompanyId == companyId &&
                t.Employee.Role == Role.User &&
                t.IsCurrent == true &&
                t.PayslipId == null &&
                t.Status == TimeEntryStatus.Accepted &&
                t.Date.Month == competenceDate.Month &&
                t.Date.Year == competenceDate.Year);
    }

    public async Task<IEnumerable<DateOnly>> GetPayrollCompetencesAsync(Guid companyId)
    {
        var result = await _dbSet
            .AsNoTracking()
            .Where(t =>
                t.Employee.CompanyId == companyId &&
                t.IsCurrent &&
                t.Status == TimeEntryStatus.Accepted)
            .Select(t => new
            {
                t.Date.Year,
                t.Date.Month
            })
            .Distinct()
            .OrderByDescending(x => x.Year)
            .ThenByDescending(x => x.Month)
            .ToListAsync();

        return result.Select(x =>
            new DateOnly(x.Year, x.Month, 1));
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
                t.Employee.Role == Role.User &&
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

    public async Task<List<TimeEntry>> GetTimeEntriesByCompetence(Guid companyId, Guid employeeId, DateOnly competenceDate)
    {
        var start = new DateOnly(
           competenceDate.Year,
           competenceDate.Month,
            1);
        var end = start.AddMonths(1);

        var result = await _dbSet
            .Include(t => t.Checks)
            .Where(t => t.Employee.CompanyId == companyId &&
                t.Employee.Role == Role.User &&
                t.EmployeeId == employeeId &&
                t.Date >= start &&
                t.Date < end &&
                t.IsCurrent &&
                t.Status == TimeEntryStatus.Accepted)
            .Select(t => t
            ).ToListAsync();

        return result;
    }
}