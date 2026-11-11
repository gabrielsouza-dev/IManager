namespace IManager.Web.Presentation.ViewModels.DemoLogins;

public class DemoLoginsViewModel
{
    public IEnumerable<DemoLoginViewModel> Staff { get; set; } = Enumerable.Empty<DemoLoginViewModel>();
    public IEnumerable<DemoCompanyLoginsViewModel> Companies { get; set; } = Enumerable.Empty<DemoCompanyLoginsViewModel>();
}
