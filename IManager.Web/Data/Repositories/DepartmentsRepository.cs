using IManager.Web.Data.Persistence;
using IManager.Web.Domain.Entities.Companies;
using IManager.Web.Domain.Interfaces.Repositories;
using IManager.Web.Presentation.ViewModels.Companies;
using IManager.Web.Presentation.ViewModels.Departments;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;

namespace IManager.Web.Data.Repositories;

public class DepartmentsRepository : Repository<Department>, IDepartmentsRepository
{
    public DepartmentsRepository(ApplicationDbContext context) : base(context) { }

    public async Task<InfoDepartmentViewModel?> GetInfoByIdAsync(Guid id)
    {
        var result = await _dbSet.Where(d => d.Id == id).Select(d => new InfoDepartmentViewModel
        {
            AverageSalary = d.JobTitles
                .SelectMany(j => j.Employees)
                .SelectMany(e => e.Payslips)
                .Average(p => (decimal?)p.NetSalary) ?? 0,

            EmployeeCount = d.JobTitles
                .SelectMany(j => j.Employees)
                .Count(),

            HighestCostJobTitleName = d.JobTitles
                .Select(j => new
                {
                    j.Name,
                    TotalCost = j.Employees
                        .SelectMany(e => e.Payslips)
                        .Sum(p => (decimal?)p.GrossSalary) ?? 0
                })
                .OrderByDescending(x => x.TotalCost)
                .Select(x => x.Name)
                .FirstOrDefault() ?? string.Empty,

            HighestCostJobTitleValue = d.JobTitles
                .Select(j => new
                {
                    j.Name,
                    TotalCost = j.Employees
                        .SelectMany(e => e.Payslips)
                        .Sum(p => (decimal?)p.GrossSalary) ?? 0
                })
                .OrderByDescending(x => x.TotalCost)
                .Select(x => x.TotalCost)
                .FirstOrDefault(),

            MostCommonJobTitle = d.JobTitles.GroupBy(j => j.Name)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault() ?? string.Empty,

            JobTitleCount = d.JobTitles.Count(),
        }).FirstOrDefaultAsync();

        return result;
    }

    public async Task<List<IndexDepartmentViewModel>> GetPagedAsync(Func<IQueryable<Department>, IQueryable<Department>>? query, int page, int pageSize)
    {
        IQueryable<Department> dbset = _dbSet;

        if (query != null)
            dbset = query(dbset);

        dbset = dbset
            .OrderBy(e => EF.Property<object>(e, "Id"))
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        var result = await dbset.Select(c => new IndexDepartmentViewModel()
        {
            Id = c.Id,
            Name = c.Name,
            CompanyTradeName = c.Company.TradeName,
            CompanyDocumentNumber= c.Company.DocumentNumber,
            CreatedAt = c.CreatedAt,
            EmployeeCount = c.JobTitles.Sum(j => j.Employees.Count),
            JobTitleCount = c.JobTitles.Count(),
            IsActive = c.IsActive,
        }).ToListAsync();

        return result;
    }
}
