namespace IManager.Web.Presentation.Requests;

public class TimeEntryActionRequest
{
    public Guid Id { get; set; }
    public bool IsApprove { get; set; }
    public string? Comment { get; set; } = string.Empty;
}