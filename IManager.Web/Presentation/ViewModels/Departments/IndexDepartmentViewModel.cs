using IManager.Web.Presentation.ViewModels.Companies;

namespace IManager.Web.Presentation.ViewModels.Departments;

public class IndexDepartmentViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CompanyTradeName { get; set; } = string.Empty;
    public string CompanyDocumentNumber { get; set; } = string.Empty;
    public int JobTitleCount { get; set; }
    public int EmployeeCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}
