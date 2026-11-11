using IManager.Web.Domain.Entities.Companies;
using IManager.Web.Presentation.ViewModels.Departments;

namespace IManager.Web.Presentation.ViewModels.JobTitles;

public class JobTitleHierarchyViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DepartmentViewModel Department { get; set; } = null!;
    public bool IsHazard { get; set; } = false;
    public bool IsUnhealthy { get; set; } = false;
    public bool IsCommissioned { get; set; } = false;
    public TimeSpan DailyHours { get; set; } = TimeSpan.FromHours(8);

}
