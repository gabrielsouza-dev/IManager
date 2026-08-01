using IManager.Web.Domain.Entities.Payrolls;
using IManager.Web.Presentation.ViewModels.Payrolls;

namespace IManager.Web.Domain.Interfaces.Repositories;

public interface IPayslipsRepository : IRepository<Payslip>
{
    Task<IEnumerable<ProcessedPayrollViewModel>> GetProcessedPayrollsAsync(Guid companyId, DateOnly competenceDate);

}