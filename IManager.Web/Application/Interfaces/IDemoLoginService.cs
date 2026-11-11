using IManager.Web.Presentation.ViewModels.DemoLogins;

namespace IManager.Web.Application.Interfaces;

public interface IDemoLoginService
{
    Task<DemoLoginsViewModel> GetDemoLoginsAsync();
}
