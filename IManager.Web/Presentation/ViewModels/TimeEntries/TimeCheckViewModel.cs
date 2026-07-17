namespace IManager.Web.Presentation.ViewModels.TimeEntries;

public class TimeCheckViewModel
{
    public Guid Id { get; set; }
    public string TimeZoneId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public DateTime TimestampLocal { get; set; }
}