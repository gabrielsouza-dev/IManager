namespace IManager.Web.Presentation.ViewModels.JobTitles;

public class DetailsJobTitleModelView
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public InfoJobTitleViewModel Info { get; set; } = new();
    public string DepartmentName { get; set; } = string.Empty;
    public string CompanyTradeName { get; set; } = string.Empty;
    public string CompanyDocumentNumber { get; set; } = string.Empty;
    public bool IsHazard { get; set; }
    public bool IsUnhealthy { get; set; }
    public bool IsCommissioned { get; set; }
    public bool IsTimeBank { get; set; }
    public TimeSpan DailyHours { get; set; }
    public bool IsActive { get; set; }
}