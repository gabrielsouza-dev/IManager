using IManager.Web.Data.Persistence;
using IManager.Web.Domain.Entities.Users;
using IManager.Web.Domain.Interfaces.Repositories;
using IManager.Web.Presentation.ViewModels.Users;
using Microsoft.EntityFrameworkCore;
namespace IManager.Web.Data.Repositories;

public class UserProfilesRepository : Repository<UserProfile>, IUserProfilesRepository
{
    public UserProfilesRepository(ApplicationDbContext context) : base(context) { }

    public async Task<InfoUserProfileViewModel?> GetInfoByIdAsync(Guid id)
    {
        var start = new DateTime(DateTime.UtcNow.Year, 1, 1, 0, 0, 0, kind: DateTimeKind.Utc);
        var end = start.AddYears(1);

        var result = await _dbSet
            .Where(u => u.Id == id)
            .Select(u => new InfoUserProfileViewModel()
            {
                JobTitle = u.JobTitle.Name,
                Department = u.JobTitle.Department.Name,
                CompanyTradeName = u.Company.TradeName,
                CompanyDocumentNumber = u.Company.DocumentNumber,
                LastAnnualNetSalary = u.Payslips
                    .Where(p => p.CreatedAt >= start && p.CreatedAt < end)
                    .Sum(p => (decimal?)p.NetSalary) ?? 0,

                LastAnnualGrossSalary = u.Payslips
                    .Where(p => p.CreatedAt >= start && p.CreatedAt < end)
                    .Sum(p => (decimal?)p.GrossSalary) ?? 0,

                AverageNetSalary = u.Payslips
                    .Where(p => p.CreatedAt >= start && p.CreatedAt < end)
                    .Average(p => (decimal?)p.NetSalary) ?? 0,

                AverageGrossSalary = u.Payslips
                    .Where(p => p.CreatedAt >= start && p.CreatedAt < end)
                    .Average(p => (decimal?)p.GrossSalary) ?? 0
            }).FirstOrDefaultAsync();

        return result;
    }
}