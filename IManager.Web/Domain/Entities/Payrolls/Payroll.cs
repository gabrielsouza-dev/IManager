using IManager.Web.Domain.Entities.Companies;

namespace IManager.Web.Domain.Entities.Payrolls;

public class Payroll : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public DateOnly Competence { get; set; }
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }

    public ICollection<Payslip> Payslips { get; set; } = new List<Payslip>();

    public Payroll(Guid companyId ,DateOnly competence)
    {
        CompanyId = companyId;
        Competence = new DateOnly(competence.Year, competence.Month, 1);
        PeriodStart = Competence;
        PeriodEnd = new DateOnly(competence.Year, competence.Month, DateTime.DaysInMonth(competence.Year, competence.Month));
    }
}
