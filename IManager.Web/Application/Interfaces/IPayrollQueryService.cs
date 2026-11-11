using IManager.Web.Presentation.ViewModels.Payrolls;

namespace IManager.Web.Application.Interfaces;

public interface IPayrollQueryService
{
    Task<IEnumerable<DateOnly>> GetCompetencesAsync(Guid companyId);
    Task<PagedResult<PayrollViewModel>> GetPendingPayrollPagedAsync(Guid companyId, DateOnly competenceDate, int page, int pageSize);
    Task<PagedResult<ProcessedPayrollViewModel>> GetProcessedPayrollPagedAsync(Guid companyId, DateOnly competenceDate, int page, int pageSize);
}
