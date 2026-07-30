using IManager.Web.Data.Persistence;
using IManager.Web.Domain.Entities.Companies;
using IManager.Web.Domain.Entities.Payrolls;
using IManager.Web.Domain.Interfaces.Repositories;
using IManager.Web.Presentation.ViewModels.Companies;
using Microsoft.EntityFrameworkCore;

namespace IManager.Web.Data.Repositories;

public class PayslipsRepository : Repository<Payslip>, IPayslipsRepository
{
    public PayslipsRepository(ApplicationDbContext context) : base(context) { }
}