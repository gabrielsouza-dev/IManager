using IManager.Web.Data.Persistence;
using IManager.Web.Domain.Entities.Payrolls;
using IManager.Web.Domain.Enums;
using IManager.Web.Domain.Interfaces.Repositories;
using IManager.Web.Presentation.ViewModels.Payrolls;
using IManager.Web.Presentation.ViewModels.TimeEntries;
using Microsoft.EntityFrameworkCore;

namespace IManager.Web.Data.Repositories;

public class PayrollsRepository : Repository<Payroll>, IPayrollsRepository
{
    public PayrollsRepository(ApplicationDbContext context) : base(context) { }
}