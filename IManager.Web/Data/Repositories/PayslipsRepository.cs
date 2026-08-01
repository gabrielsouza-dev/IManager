using IManager.Web.Data.Persistence;
using IManager.Web.Domain.Entities.Companies;
using IManager.Web.Domain.Entities.Payrolls;
using IManager.Web.Domain.Interfaces.Repositories;
using IManager.Web.Presentation.ViewModels.Companies;
using IManager.Web.Presentation.ViewModels.Payrolls;
using Microsoft.EntityFrameworkCore;

namespace IManager.Web.Data.Repositories;

public class PayslipsRepository : Repository<Payslip>, IPayslipsRepository
{
    public PayslipsRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<ProcessedPayrollViewModel>> GetProcessedPayrollsAsync(Guid companyId, DateOnly competenceDate)
    {
        var result = await _dbSet
            .AsNoTracking()
            .Where(p => p.Payroll.CompanyId == companyId
                && p.Payroll.Competence.Month == competenceDate.Month
                && p.Payroll.Competence.Year == competenceDate.Year)
            .Select(p => new ProcessedPayrollViewModel
            {
                Id = p.Id,
                EmployeeName = p.Employee.FullName,
                EmployeeDepartment = p.Employee.JobTitle.Department.Name,
                EmployeeJobtitle = p.Employee.JobTitle.Name,
                GrossSalary = p.GrossSalary,
                NetSalary = p.NetSalary,
                RegularSalary = p.RegularSalary,
                TotalDeductions = p.TotalDeductions,
                TotalExtraEarnings = p.TotalExtraEarnings
            })
            .ToListAsync();

        return result;
    }
}