using IManager.Web.Application.Services;
using IManager.Web.Domain.Entities.Companies;
using IManager.Web.Domain.Entities.TimeTrackings;
using IManager.Web.Domain.Entities.Users;

namespace IManager.Web.Domain.Entities.Payrolls;

public class Payslip : BaseEntity
{
    public Guid PayrollId { get; set; } = Guid.NewGuid();
    public Payroll Payroll { get; set; } = null!;

    public Guid EmployeeId { get; set; }
    public UserProfile Employee { get; set; } = null!;
    public ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();

    public decimal GrossSalary { get; set; }
    public decimal TotalExtraEarnings { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetSalary { get; set; }

    public decimal RegularSalary { get; set; }
    public decimal OvertimeAdditionals { get; set; } = 0m;
    public decimal HazardAdditionals { get; set; } = 0m;
    public decimal UnhealthyAdditionals { get; set; } = 0m;
    public decimal NightShiftAdditionals { get; set; }
    public decimal Commission { get; set; } = 0m;
    public decimal INSSDeduction { get; set; } = 0m;
    public decimal IRRFDeduction { get; set; } = 0m;
    public decimal OtherDeductions { get; set; } = 0m;
    public TimeSpan RegularHours { get; set; }
    public TimeSpan OvertimeHours { get; set; }
    public TimeSpan NightShiftHours { get; set; }
}
