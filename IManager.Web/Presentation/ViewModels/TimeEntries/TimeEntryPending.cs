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
    public ICollection<TimeCheckViewModel> OriginalChecks { get; set; } = new List<TimeCheckViewModel>();
    public ICollection<TimeCheckViewModel> NewChecks { get; set; } = new List<TimeCheckViewModel>();
    public TimeEntryStatus Status { get; set; }
    public Guid? ParentId { get; set; }
    public string? AdjustmentReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsConsistent { get; set; }
    public TimeSpan HoursWorked { get; set; }
    public bool IsCurrent { get; set; } = true;
}