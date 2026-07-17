using IManager.Web.Domain.Entities.Payrolls;
using IManager.Web.Domain.Entities.Users;
using IManager.Web.Domain.Enums;
using IManager.Web.Shared.Helpers;
using System.ComponentModel.DataAnnotations.Schema;

namespace IManager.Web.Domain.Entities.TimeTrackings;

public class TimeEntry : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public UserProfile Employee { get; set; } = null!;

    public Guid? PayslipId { get; set; }
    public Payslip? Payslip { get; set; }

    public ICollection<TimeCheck> Checks { get; set; } = new List<TimeCheck>();

    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public TimeEntryStatus Status { get; set; } = TimeEntryStatus.Accepted;
    public bool IsCurrent { get; set; } = true;
    public Guid? ParentId { get; set; }
    public TimeEntry? Parent { get; set; }
    public string? AdjustmentReason { get; set; }
    public string? RejectionReason { get; set; }

    [NotMapped]
    public bool IsConsistent => Checks.IsConsistent();

    [NotMapped]
    public TimeSpan HoursWorked => Checks.GetHoursWorked();
}