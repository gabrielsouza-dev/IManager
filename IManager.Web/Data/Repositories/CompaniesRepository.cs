using IManager.Web.Data.Persistence;
using IManager.Web.Domain.Entities.Companies;
using IManager.Web.Domain.Interfaces.Repositories;
using IManager.Web.Presentation.ViewModels.Companies;
using Microsoft.EntityFrameworkCore;

namespace IManager.Web.Data.Repositories;

public class CompaniesRepository : Repository<Company>, ICompaniesRepository
{
    public CompaniesRepository(ApplicationDbContext context) : base(context) { }

    public async Task<InfoCompanyViewModel?> GetInfoByIdAsync(Guid id)
    {
        var start = new DateTime(DateTime.UtcNow.Year, 1, 1, 0, 0, 0, kind: DateTimeKind.Utc);
        var end = start.AddYears(1);

        var result = await _dbSet
            .Where(c => c.Id == id)
            .Select(c => new InfoCompanyViewModel
            {
                EmployeeCount = c.Employees.Count(),

                DepartmentCount = c.Departments.Count(),

                LastCreatedUser = c.Employees
                    .OrderByDescending(e => e.CreatedAt)
                    .Select(e => e.FullName)
                    .FirstOrDefault() ?? string.Empty,

                LastCreatedDate = c.Employees
                    .OrderByDescending(e => e.CreatedAt)
                    .Select(e => (DateTime?)e.CreatedAt)
                    .FirstOrDefault() ?? DateTime.MinValue,

                AnnualPayrollCost = c.Employees
                    .SelectMany(e => e.Payslips)
                    .Where(p => p.CreatedAt >= start && p.CreatedAt < end)
                    .Sum(p => (decimal?)p.NetSalary) ?? 0,

                AverageSalary = c.Employees
                    .Where(e => e.Payslips.Any(p => p.CreatedAt >= start && p.CreatedAt < end))
                    .Select(e => (decimal?)e.Payslips
                        .Where(p => p.CreatedAt >= start && p.CreatedAt < end)
                        .Average(p => (decimal?)p.NetSalary))
                    .Average() ?? 0
            })
            .FirstOrDefaultAsync();

        return result;
    }

    public async Task<List<IndexCompanyViewModel>> GetPagedAsync(Func<IQueryable<Company>, IQueryable<Company>>? query, int page, int pageSize)
    {
        IQueryable<Company> dbset = _dbSet;

        if (query != null)
            dbset = query(dbset);

        dbset = dbset
            .OrderBy(e => EF.Property<object>(e, "Id"))
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        var result = await dbset.Select(c => new IndexCompanyViewModel() 
        { 
            Id = c.Id,
            FoundedAt = c.FoundedAt,
            DocumentNumber = c.DocumentNumber,
            IsActive = c.IsActive,
            LegalName = c.LegalName,
            TradeName = c.TradeName,
            EmployeeCount = c.Employees.Count(),
            DepartmentCount = c.Departments.Count()
        }).ToListAsync();

        return result;
    }
}