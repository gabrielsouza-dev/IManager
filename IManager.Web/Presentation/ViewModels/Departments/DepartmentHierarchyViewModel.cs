using IManager.Web.Presentation.ViewModels.JobTitles;

namespace IManager.Web.Presentation.ViewModels.Departments;

public class DepartmentHierarchyViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public IEnumerable<JobTitleViewModel> JobTitles { get; set; } = new List<JobTitleViewModel>();
}