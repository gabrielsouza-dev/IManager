using IManager.Web.Presentation.ViewModels.Payrolls;

namespace IManager.Web.Application.Interfaces;

public interface IPayrollQueryService
{
    Task<IEnumerable<DateOnly>> GetCompetencesAsync(Guid companyId);
    Task<IEnumerable<PayrollViewModel>> GetPendingPayrollAsync(Guid companyId, DateOnly competenceDate);
    Task<IEnumerable<ProcessedPayrollViewModel>> GetProcessedPayrollAsync(Guid companyId, DateOnly competenceDate);
}