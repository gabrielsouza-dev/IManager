using IManager.Web.Domain.Entities.Payrolls;
using IManager.Web.Presentation.ViewModels.Payrolls;
using IManager.Web.Presentation.ViewModels.Payslips;

namespace IManager.Web.Domain.Interfaces.Repositories;

public interface IPayslipsRepository : IRepository<Payslip>
{
    Task<IEnumerable<IndexPayslipViewModel>> GetIndexPagedAsync(Func<IQueryable<Payslip>, IQueryable<Payslip>> query, int page, int pageSize);
    Task<PayslipViewModel?> GetPayslipViewModelAsync(Guid payslipId);
    Task<IEnumerable<ProcessedPayrollViewModel>> GetProcessedPayrollsPagedAsync(Func<IQueryable<Payslip>, IQueryable<Payslip>> query, int page, int pageSize);

}