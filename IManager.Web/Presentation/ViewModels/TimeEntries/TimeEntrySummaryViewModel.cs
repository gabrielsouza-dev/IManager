using IManager.Web.Domain.Enums;

namespace IManager.Web.Presentation.ViewModels.TimeEntries;

public class TimeEntrySummaryViewModel
{
    public Guid Id { get; set; }
    public bool IsConsistent { get; set; }
    public TimeSpan HoursWorked {  get; set; }
    public int DaysWorked { get; set; }
}