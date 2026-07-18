using IManager.Web.Presentation.Requests;
using IManager.Web.Shared;

namespace IManager.Web.Application.Interfaces;

public interface IPayrollGenerationService
{
    Task<Result> ProcessAsync(Guid companyId, ProcessPayrollRequest request);
}