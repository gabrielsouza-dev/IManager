using IManager.Web.Application.Interfaces;
using IManager.Web.Domain.Consts;
using IManager.Web.Presentation.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IManager.Web.Presentation.Controllers;

[Authorize(Policy = Policies.StaffOrAdmin)]
public class UsersController : Controller
{
    private readonly IAccountService _accountService;
    private readonly IUserService _UserService;
    private readonly IDepartmentService _departmentService;

    public UsersController(IAccountService accountService, IUserService userService, IDepartmentService departmentService)
    {
        _accountService = accountService;
        _UserService = userService;
        _departmentService = departmentService;
    }

    // GET: Users
    public async Task<IActionResult> Index([FromQuery] string search, [FromQuery] ActiveFilter active = ActiveFilter.Active, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        PagedResult<IndexUserViewModel> model;
        if (User.IsInRole(Role.Staff))
        {
            model = await _UserService.GetPagedAsync(page, pageSize, active, search: search);
        }
        else
        {
            var companyId = Guid.Parse(User.FindFirst("CompanyId")!.Value);
            model = await _UserService.GetPagedAsync(page, pageSize, active, companyId, search);
        }

        return View(model);
    }

    // GET: Users/Details/5
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var userProfile = await _UserService.GetDetailsViewModelByIdAsync(id.Value);
        if (userProfile == null) return NotFound();

        return View(userProfile);
    }

    // GET: Users/Edit/5
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();
        
        var model = await _accountService.GetEditAccountViewModelByIdAsync(id.Value);
        if (model == null) return NotFound();

        await AddDepartmentsHierarchyViewBagAsync(model.CompanyId);

        return View(model);
    }

    // POST: Users/Edit/5
    [HttpPost]
    public async Task<IActionResult> Edit(Guid id, EditAccountViewModel model)
    {
        if (id != model.Id) return NotFound();

        if (!ModelState.IsValid)
        {
            await AddDepartmentsHierarchyViewBagAsync(model.CompanyId);
            return View(model);
        }

        var result = await _accountService.EditAccountAsync(id, model);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error);

            TempData[ToastMessages.Error] = $"Erro - {string.Join(", ", result.Errors)}.";
            await AddDepartmentsHierarchyViewBagAsync(model.CompanyId);
            return View(model);
        }

        TempData[ToastMessages.Success] = "Dados atualizados com sucesso!";

        return RedirectToAction("Details", new { id });
    }

    // GET: Users/Delete/5
    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null) return NotFound();

        var userProfile = await _accountService.GetDetailsViewModelByIdAsync(id.Value);
        if (userProfile == null) return NotFound();

        return View(userProfile);
    }

    // POST: Users/Delete/5
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        if (id == Guid.Empty) return NotFound();

        var result = await _accountService.SoftDeleteAsync(id);
        if (!result.Succeeded)
        {
            TempData[ToastMessages.Error] = $"Erro - {string.Join(", ", result.Errors)}";
            return RedirectToAction(nameof(Index));
        }

        TempData[ToastMessages.Success] = "Usuário atualizado com sucesso!";
        return RedirectToAction(nameof(Index));
    }

    private async Task AddDepartmentsHierarchyViewBagAsync(Guid CompanyId)
    {
        ViewBag.Departments = await _departmentService.GetDepartmentsHierarchyViewModelAsync(CompanyId);
    }
}