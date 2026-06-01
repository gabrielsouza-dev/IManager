namespace IManager.Web.Presentation.ViewModels.Companies;

public class IndexCompanyViewModel
{
    public Guid Id { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
    public int DepartmentCount { get; set; }
    public string LegalName { get; set; } = string.Empty;
    public string TradeName { get; set; } = string.Empty;
    public DateOnly FoundedAt { get; set; }
    public bool IsActive { get; set; }
}
