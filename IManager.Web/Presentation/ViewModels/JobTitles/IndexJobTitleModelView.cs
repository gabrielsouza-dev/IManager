namespace IManager.Web.Presentation.ViewModels.JobTitles;

public class IndexJobTitleModelView
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string CompanyTradeName { get; set; } = string.Empty;
    public string CompanyDocumentNumber { get; set; } = string.Empty;
    public bool IsHazard { get; set; }
    public bool IsUnhealthy { get; set; }
    public bool IsCommissioned { get; set; }
    public TimeSpan DailyHours { get; set; }
    public bool IsActive { get; set; }
    public int EmployeeCount { get; set; }
    public DateTime CreatedAt { get; set; }
}