using IManager.Web.Domain.Entities.Payrolls;
using IManager.Web.Presentation.ViewModels.Payrolls;
using IManager.Web.Presentation.ViewModels.Payslips;

namespace IManager.Web.Domain.Interfaces.Repositories;

public interface IPayslipsRepository : IRepository<Payslip>
{
    Task<IEnumerable<IndexPayslipViewModel>> GetPayslipsByUserIdAsync(Guid userId);
    Task<PayslipViewModel> GetPayslipViewModelAsync(Guid payslipId);
    Task<IEnumerable<ProcessedPayrollViewModel>> GetProcessedPayrollsAsync(Guid companyId, DateOnly competenceDate);

}