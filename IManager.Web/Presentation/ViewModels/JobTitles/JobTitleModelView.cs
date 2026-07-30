namespace IManager.Web.Presentation.ViewModels.JobTitles;

public class JobTitleModelView
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsHazard { get; set; } = false;
    public bool IsUnhealthy { get; set; } = false;
    public bool IsCommissioned { get; set; } = false;
    public bool IsTimeBank { get; set; } = false;
    public TimeSpan DailyHours { get; set; } = TimeSpan.FromHours(8);
    public bool IsActive { get; set; } = false;
}
