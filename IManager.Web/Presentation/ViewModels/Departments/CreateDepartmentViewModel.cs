using IManager.Web.Presentation.ViewModels.Companies;
using IManager.Web.Presentation.ViewModels.JobTitles;

namespace IManager.Web.Presentation.ViewModels.Departments;

public class CreateDepartmentViewModel
{
    public string Name { get; set; } = string.Empty;
    public Guid CompanyId { get; set; }
}
