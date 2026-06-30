using IManager.Web.Domain.Enums;

namespace IManager.Web.Domain.Entities.TimeTrackings;

public class TimeCheck : BaseEntity
{
    public Guid TimeEntryId { get; set; }
    public TimeEntry TimeEntry { get; set; } = null!;
    public string TimeZoneId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
