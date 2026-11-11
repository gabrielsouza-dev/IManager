namespace IManager.Web.Presentation.ViewModels.DemoLogins;

public class DemoCompanyLoginsViewModel
{
    public string CompanyTradeName { get; set; } = string.Empty;
    public DemoLoginViewModel? Admin { get; set; }
    public IEnumerable<DemoLoginViewModel> Users { get; set; } = Enumerable.Empty<DemoLoginViewModel>();
}
