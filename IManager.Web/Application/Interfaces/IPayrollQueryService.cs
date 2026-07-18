using IManager.Web.Presentation.ViewModels.Payrolls;

namespace IManager.Web.Application.Interfaces;

public interface IPayrollQueryService
{
    Task<IEnumerable<DateOnly>> GetPendingCompetencesAsync(Guid companyId);
    Task<IEnumerable<PayrollViewModel>> GetPendingPayrollAsync(Guid companyId, DateOnly competenceDate);
    Task<IEnumerable<PayrollViewModel>> GetProcessedPayrollAsync(Guid companyId, DateOnly competenceDate);

}