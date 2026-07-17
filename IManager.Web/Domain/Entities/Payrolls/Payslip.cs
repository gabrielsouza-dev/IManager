using IManager.Web.Domain.Entities.TimeTrackings;
using IManager.Web.Domain.Entities.Users;

namespace IManager.Web.Domain.Entities.Payrolls;

public class Payslip : BaseEntity
{
    public Guid PayrollId { get; set; }
    public Payroll Payroll { get; set; } = null!;

    public Guid EmployeeId { get; set; }
    public UserProfile Employee { get; set; } = null!;
    public ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();


    public decimal GrossSalary { get; set; } = 0m;
    public decimal TotalEarnings { get; set; }//=> GrossSalary + OvertimeAdditionals + HazardPay + UnhealthyPay + Commission;
    public decimal TotalDeductions { get; set; }//=> INSSDeduction + IRRFDeduction + OtherDeductions;
    public decimal NetSalary { get; set; }//=> TotalEarnings - TotalDeductions;

    public decimal OvertimeAdditionals { get; set; } = 0m;
    public decimal HazardPay { get; set; } = 0m;
    public decimal UnhealthyPay { get; set; } = 0m;
    public decimal Commission { get; set; } = 0m;
    public decimal INSSDeduction { get; set; } = 0m;
    public decimal IRRFDeduction { get; set; } = 0m;
    public decimal OtherDeductions { get; set; } = 0m;
}