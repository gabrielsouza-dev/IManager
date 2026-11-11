using IManager.Web.Application.Interfaces;
using IManager.Web.Domain.Consts;
using IManager.Web.Presentation.ViewModels.Account;
using IManager.Web.Presentation.ViewModels.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace IManager.Web.Presentation.Controllers;

public class AccountController : Controller
{
    private readonly IAccountService _accountService;
    private readonly ICompanyService _companyService;
    private readonly IDepartmentService _departmentService;

    public AccountController(IAccountService accountService, ICompanyService companyService, IDepartmentService departmentService)
    {
        _accountService = accountService;
        _companyService = companyService;
        _departmentService = departmentService;
    }

    #region Registro e Confirmação de E-mail

    [Authorize(Policy = Policies.StaffOrAdmin)]
    [HttpGet]
    public async Task<IActionResult> Register()
    {
        var model = new RegisterViewModel();
        await AddViewBagHierarchyViewModel(model);

        return View(model);
    }

    [Authorize(Policy = Policies.StaffOrAdmin)]
    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await AddViewBagHierarchyViewModel(model);

            return View(model);
        }

        var result = await _accountService.RegisterAsync(model);
        if (result.Succeeded)
        {
            var user = await _accountService.GetByEmailAsync(model.Email);
            var token = await _accountService.GenerateConfirmationTokenAsync(user!);
            var link = Url.Action("ConfirmEmail", "Account", new { userId = user!.Id, token }, Request.Scheme);
            try
            {
                await _accountService.SendConfirmationEmailAsync(user!, link!);
                return RedirectToAction("EmailConfirmationPending");
            }
            catch
            {
                TempData[ToastMessages.Error] = "Erro ao enviar o e-mail de confirmação. Tente novamente mais tarde.";

                await AddViewBagHierarchyViewModel(model);

                return View(model);
            }
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError("", error);

        return View(model);
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(Guid userId, string token)
    {
        await _accountService.LogoutAsync();

        var result = await _accountService.ConfirmEmailAsync(userId, token);
        if (!result.Succeeded)
        {
            TempData[ToastMessages.Error] = "Link inválido ou expirado.";
            return RedirectToAction("EmailConfirmationError");
        }
        else
        {
            TempData[ToastMessages.Success] = "E-mail confirmado com sucesso! Já pode fazer seu login.";
            return RedirectToAction("Login");
        }
    }

    [Authorize(Policy = Policies.StaffOrAdmin)]
    [HttpGet]
    public IActionResult EmailConfirmationPending() => View();

    #endregion

    #region Login e Logout

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login() => View();

    [AllowAnonymous]
    [HttpPost]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login(string email, string password)
    {
        var result = await _accountService.LoginAsync(email, password);
        if (result.Succeeded)
        {
            TempData[ToastMessages.Success] = "Login realizado com sucesso!";
            return RedirectToAction("Index", "Home");
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError("", error);

        return View();
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _accountService.LogoutAsync();
        TempData[ToastMessages.Success] = "Até logo!";
        return RedirectToAction("Index", "Home");
    }

    #endregion

    #region Recuperação de Senha

    [AllowAnonymous]
    [HttpGet]
    public IActionResult ForgotPassword() => View();

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _accountService.GetByEmailAsync(model.Email);
        if (user == null)
            return RedirectToAction("ForgotPasswordPending");

        var token = await _accountService.GenerateResetPasswordTokenAsync(user);
        var link = Url.Action("ConfirmResetPasswordToken", "Account", new { email = user.Email, token }, Request.Scheme);
        try
        {
            await _accountService.SendResetPasswordEmailAsync(user, link!);
            return RedirectToAction("ForgotPasswordPending");
        }
        catch
        {
            TempData[ToastMessages.Error] = "Erro ao enviar o e-mail. Tente novamente mais tarde.";
            return View(model);
        }
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult ForgotPasswordPending() => View();

    [AllowAnonymous]
    [HttpGet]
    public IActionResult ConfirmResetPasswordToken(string email, string token)
        => View(new ResetPasswordViewModel { Email = email, Token = token });

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> ConfirmResetPasswordToken(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _accountService.ConfirmResetPasswordTokenAsync(model);
        if (result.Succeeded)
        {
            TempData[ToastMessages.Success] = "Senha redefinida com sucesso!";
            return RedirectToAction("Login");
        }

        TempData[ToastMessages.Error] = "Token inválido ou expirado.";
        return View(model);
    }

    #endregion

    #region Perfil

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Details()
        => View(await _accountService.GetAccountDetailsViewModelByEmailAsync(User.Identity!.Name!));

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> EditDetails()
        => View(await _accountService.GetAccountDetailsViewModelByEmailAsync(User.Identity!.Name!));

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> EditDetails(AccountDetailsViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _accountService.EditDetailsAsync(User.Identity!.Name!, model);
        if (result.Succeeded)
        {
            TempData[ToastMessages.Success] = "Dados atualizados com sucesso!";
            return RedirectToAction("Details");
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError("", error);

        return View(model);
    }

    #endregion

    #region Troca de E-mail

    [Authorize]
    [HttpGet]
    public IActionResult ChangeEmail() => View();

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> ChangeEmail(ChangeEmailViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _accountService.GetByIdAsync(userId!);
        var token = await _accountService.GenerateChangeEmailTokenAsync(user!, model.NewEmail);
        var link = Url.Action("ConfirmChangeEmail", "Account",
            new { currentEmail = user!.Email, newEmail = model.NewEmail, token }, Request.Scheme);
        try
        {
            await _accountService.SendChangeEmailLinkAsync(user!, model.NewEmail, link!);
            return RedirectToAction("ChangeEmailPending");
        }
        catch
        {
            TempData[ToastMessages.Error] = "Erro ao enviar o e-mail. Tente novamente mais tarde.";
            return View(model);
        }
    }

    [Authorize]
    [HttpGet]
    public IActionResult ChangeEmailPending() => View();

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> ConfirmChangeEmail(string currentEmail, string newEmail, string token)
    {
        var result = await _accountService.ConfirmChangeEmailAsync(currentEmail, newEmail, token);
        if (result.Succeeded)
        {
            await _accountService.LogoutAsync();
            TempData[ToastMessages.Success] = "E-mail atualizado com sucesso!";
            return RedirectToAction("Login");
        }

        TempData[ToastMessages.Error] = "Link inválido ou expirado.";
        return RedirectToAction("EmailConfirmationError");
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult EmailConfirmationError() => View();

    #endregion

    #region Troca de Senha

    [Authorize]
    [HttpGet]
    public IActionResult ChangePassword() => View();

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _accountService.ChangePasswordAsync(User.Identity!.Name!, model);
        if (result.Succeeded)
        {
            TempData[ToastMessages.Success] = "Senha alterada com sucesso!";
            return RedirectToAction("Details");
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError("", error);

        return View(model);
    }

    #endregion

    #region Helpers
    private async Task AddViewBagHierarchyViewModel(RegisterViewModel model)
    {
        if (User.IsInRole(Role.Staff))
            ViewBag.Companies = await _companyService.GetCompaniesHierarchyViewModelAsync();
        else
        {
            var companyId = Guid.Parse(User.FindFirst("CompanyId")!.Value);
            ViewBag.Departments = await _departmentService.GetDepartmentsHierarchyViewModelAsync(companyId);

            model.CompanyId = companyId;
        }
    }
    #endregion
}