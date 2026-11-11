namespace IManager.Web.Presentation.ViewModels.DemoLogins;

public class DemoLoginViewModel
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public Guid CompanyId { get; set; }
    public string CompanyTradeName { get; set; } = string.Empty;
}
