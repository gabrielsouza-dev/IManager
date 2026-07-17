using IManager.Web.Presentation.Controllers;
using IManager.Web.Presentation.ViewModels.Payrolls;
using IManager.Web.Shared;

namespace IManager.Web.Application.Interfaces;

public interface IPayrollGenerationService
{
    Task<IEnumerable<DateOnly>> GetPendingCompetencesAsync(Guid companyId);
    Task<IEnumerable<PayrollViewModel>> GetPendingPayrollAsync(Guid companyId, DateOnly competenceDate);
    Task<IEnumerable<PayrollViewModel>> GetProcessedPayrollAsync(Guid companyId, DateOnly competenceDate);
    Task<Result> ProcessAsync(Guid companyId, ProcessPayrollRequest request);
}