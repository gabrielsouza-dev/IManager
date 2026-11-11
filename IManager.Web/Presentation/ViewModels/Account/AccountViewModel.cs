using IManager.Web.Presentation.ViewModels.Companies;
using IManager.Web.Presentation.ViewModels.JobTitles;

namespace IManager.Web.Presentation.ViewModels.Account;

public class AccountViewModel
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
    public string? PhoneNumber { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public JobTitleHierarchyViewModel JobTitle { get; set; } = null!;
    public decimal BaseSalary { get; set; } = 0m;
    public bool IsActive { get; set; }
}