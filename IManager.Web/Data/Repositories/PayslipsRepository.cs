using IManager.Web.Data.Persistence;
using IManager.Web.Domain.Entities.Companies;
using IManager.Web.Domain.Entities.Payrolls;
using IManager.Web.Domain.Entities.Users;
using IManager.Web.Domain.Interfaces.Repositories;
using IManager.Web.Presentation.ViewModels.Companies;
using IManager.Web.Presentation.ViewModels.Payrolls;
using IManager.Web.Presentation.ViewModels.Payslips;
using Microsoft.EntityFrameworkCore;

namespace IManager.Web.Data.Repositories;

public class PayslipsRepository : Repository<Payslip>, IPayslipsRepository
{
    public PayslipsRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<IndexPayslipViewModel>> GetPayslipsByUserIdAsync(Guid userId)
    {
        var result = await _dbSet
            .AsNoTracking()
            .Where(p => p.EmployeeId == userId)
            .Select(p => new IndexPayslipViewModel
            {
                Id = p.Id,
                CreatedAt = p.CreatedAt,
                CompetenceDate = p.Payroll.Competence
            })
            .OrderByDescending(x => x.CompetenceDate)
            .ToListAsync();

        return result;
    }

    public async Task<PayslipViewModel?> GetPayslipViewModelAsync(Guid payslipId)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(p => p.Id == payslipId)
            .Select(p => new PayslipViewModel
            {
                Id = p.Id,
                CreatedAt = p.CreatedAt,

                Commission = p.Commission,

                CompanyDocument = p.Payroll.Company.DocumentNumber,
                CompanyName = p.Payroll.Company.LegalName,

                EmployeeId = p.EmployeeId,
                EmployeeDocument = p.Employee.DocumentNumber,
                EmployeeName = p.Employee.FullName,
                JobTitle = p.Employee.JobTitle.Name,

                GrossSalary = p.GrossSalary,
                HazardAdditionals = p.HazardAdditionals,
                INSSDeduction = p.INSSDeduction,
                IRRFDeduction = p.IRRFDeduction,

                NetSalary = p.NetSalary,

                NightShiftAdditionals = p.NightShiftAdditionals,
                NightShiftHours = p.NightShiftHours,

                OtherDeductions = p.OtherDeductions,

                OvertimeAdditionals = p.OvertimeAdditionals,
                OvertimeHours = p.OvertimeHours,

                RegularHours = p.RegularHours,
                RegularSalary = p.RegularSalary,

                ReferenceMonth = p.Payroll.Competence.Month,
                ReferenceYear = p.Payroll.Competence.Year,

                TotalDeductions = p.TotalDeductions,
                TotalExtraEarnings = p.TotalExtraEarnings,

                UnhealthyAdditionals = p.UnhealthyAdditionals
            })
            .FirstOrDefaultAsync();
    }

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