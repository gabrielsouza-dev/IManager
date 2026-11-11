using IManager.Web.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IManager.Web.Presentation.Controllers;

[AllowAnonymous]
public class DemoLoginsController : Controller
{
    private readonly IDemoLoginService _demoLoginService;

    public DemoLoginsController(IDemoLoginService demoLoginService)
    {
        _demoLoginService = demoLoginService;
    }

    // GET: /demo-logins
    [HttpGet("demo-logins")]
    public async Task<IActionResult> Index()
    {
        // a lista de logins só fica disponível antes de logar
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        var model = await _demoLoginService.GetDemoLoginsAsync();
        return View(model);
    }
}
