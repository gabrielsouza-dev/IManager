namespace IManager.Web.Presentation.ViewModels.JobTitles;

public class CreateJobTitleModelView
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid DepartmentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsHazard { get; set; } = false;
    public bool IsUnhealthy { get; set; } = false;
    public bool IsCommissioned { get; set; } = false;
    public bool IsTimeBank { get; set; } = true;
    public TimeSpan DailyHours { get; set; } = TimeSpan.FromHours(8);
}
