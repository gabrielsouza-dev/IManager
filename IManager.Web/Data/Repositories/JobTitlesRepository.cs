using IManager.Web.Data.Persistence;
using IManager.Web.Domain.Entities.Companies;
using IManager.Web.Domain.Interfaces.Repositories;
using IManager.Web.Presentation.ViewModels.Departments;
using IManager.Web.Presentation.ViewModels.JobTitles;
using Microsoft.EntityFrameworkCore;

namespace IManager.Web.Data.Repositories;

public class JobTitlesRepository : Repository<JobTitle>, IJobTitlesRepository
{
    public JobTitlesRepository(ApplicationDbContext context) : base(context) { }

    public async Task<InfoJobTitleViewModel?> GetInfoByIdAsync(Guid id)
    {
        var start = new DateTime(DateTime.UtcNow.Year, 1, 1, 0, 0, 0, kind: DateTimeKind.Utc);
        var end = start.AddYears(1);

        var result = await _dbSet
            .Where(j => j.Id == id)
            .Select(j => new InfoJobTitleViewModel
            {
                EmployeeCount = j.Employees.Count(),

                AverageSalary = j.Employees
                    .Where(e => e.Payslips
                        .Any(p => p.CreatedAt >= start && p.CreatedAt < end))
                        .Average(e => e.Payslips.Where(p => p.CreatedAt >= start && p.CreatedAt < end)
                        .Average(p => (decimal?)p.NetSalary)) ?? 0,

                TotalCost = j.Employees
                    .SelectMany(e => e.Payslips)
                    .Where(p => p.CreatedAt >= start && p.CreatedAt < end)
                    .Sum(p => (decimal?)p.GrossSalary) ?? 0
            })
            .FirstOrDefaultAsync();

        return result;
    }

    public async Task<IEnumerable<IndexJobTitleModelView>> GetPagedAsync(Func<IQueryable<JobTitle>, IQueryable<JobTitle>> query, int page, int pageSize)
    {
        IQueryable<JobTitle> dbset = _dbSet;

        if (query != null)
            dbset = query(dbset);

        dbset = dbset
            .OrderBy(e => EF.Property<object>(e, "Id"))
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        var result = await dbset.Select(c => new IndexJobTitleModelView()
        {
            Id = c.Id,
            Name = c.Name,
            CompanyTradeName = c.Department.Company.TradeName,
            CompanyDocumentNumber = c.Department.Company.DocumentNumber,
            CreatedAt = c.CreatedAt,
            EmployeeCount = c.Employees.Count,
            DailyHours = c.DailyHours,
            DepartmentName = c.Department.Name,
            IsCommissioned = c.IsCommissioned,
            IsHazard = c.IsHazard,
            IsUnhealthy = c.IsUnhealthy,
            IsActive = c.IsActive,
        }).ToListAsync();

        return result;
    }
}
