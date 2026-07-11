using IManager.Web.Domain.Entities.TimeTrackings;
using IManager.Web.Domain.Enums;
using IManager.Web.Shared.Helpers;

namespace IManager.Web.Presentation.ViewModels.TimeEntries;

public class TimeEntryPending
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public ICollection<TimeCheck> OriginalChecks { get; set; } = new List<TimeCheck>();
    public ICollection<TimeCheck> NewChecks { get; set; } = new List<TimeCheck>();
    public TimeEntryStatus Status { get; set; }
    public Guid? ParentId { get; set; }
    public string? AdjustmentReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsConsistent => NewChecks.IsConsistent();
    public TimeSpan HoursWorked => NewChecks.GetHoursWorked();
    public bool IsCurrent { get; set; } = true;
}