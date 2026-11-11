using IManager.Web.Presentation.ViewModels.Payslips;

namespace IManager.Web.Application.Interfaces;

public interface IPayslipService
{
    Task<PayslipViewModel?> GetByIdAsync(Guid userId, Guid payslipId);
    Task<PagedResult<IndexPayslipViewModel>> GetPagedByUserAsync(Guid userId, int page, int pageSize);
}
