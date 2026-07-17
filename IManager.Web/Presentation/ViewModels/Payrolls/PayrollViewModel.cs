using IManager.Web.Presentation.ViewModels.TimeEntries;

namespace IManager.Web.Presentation.ViewModels.Payrolls;

public class PayrollViewModel
{
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateOnly Competence { get; set; }
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
    public TimeEntrySummaryViewModel TimeEntry { get; set; } = new TimeEntrySummaryViewModel();
}
