using IManager.Web.Application.Interfaces;
using IManager.Web.Domain.Consts;
using IManager.Web.Presentation.ViewModels.Departments;
using IManager.Web.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IManager.Web.Presentation.Controllers;

[Authorize(Policy = Poly.StaffOrAdmin)]
public class DepartmentsController : Controller
{
    public readonly ICompanyService _companyService;
    public readonly IDepartmentService _departmentService;

    public DepartmentsController(ICompanyService companyService, IDepartmentService departmentService)
    {
        _companyService = companyService;
        _departmentService = departmentService;
    }

    // GET: Departments
    public async Task<IActionResult> Index([FromQuery] string search, [FromQuery] ActiveFilter active = ActiveFilter.Active, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        PagedResult<IndexDepartmentViewModel> model;
        if(User.IsInRole(Role.Staff))
        {
            model = await _departmentService.GetPagedAsync(search, active, page, pageSize);
        }
        else
        {
            var companyId = Guid.Parse(User.FindFirst("CompanyId")!.Value);
            model = await _departmentService.GetPagedAsync(search, active, page, pageSize, companyId);
        }

        return View(model);
    }

    // GET: Departments/Details/5
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var model = await _departmentService.GetDetailsViewModelByIdAsync(id.Value);
        if (model == null) return NotFound();

        return View(model);
    }

    // GET: Departments/Create
    public async Task<IActionResult> Create()
    {
        var companyId = Guid.Parse(User.FindFirst("CompanyId")!.Value);

        await IfStaffAddViewBagViewModel();

        var model = new CreateDepartmentViewModel
        {
            CompanyId = companyId
        };
        return View(model);
    }

    // POST: Departments/Create
    [HttpPost]
    public async Task<IActionResult> Create(CreateDepartmentViewModel department)
    {
        if (!ModelState.IsValid)
        {
            await IfStaffAddViewBagViewModel();
            return View(department);
        }

        Result result = await _departmentService.CreateAsync(department);
        
        if (!result.Succeeded)
        {
            await IfStaffAddViewBagViewModel();
            TempData[ToastMessages.Error] = $"Erro - {string.Join(", ", result.Errors)}.";

            return View(department);
        }

        TempData[ToastMessages.Success] = "Setor cadastrado com sucesso!";
        return RedirectToAction(nameof(Index));
    }

    // GET: Departments/Edit/5
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();

        var department = await _departmentService.GetEditViewModelByIdAsync(id.Value);
        if (department == null) return NotFound();

        return View(department);
    }

    // POST: Departments/Edit/5
    [HttpPost]
    public async Task<IActionResult> Edit(Guid id, EditDepartmentViewModel department)
    {
        if (id != department.Id) return NotFound();

        if (!ModelState.IsValid) return View(department);

        var result = await _departmentService.UpdateAsync(department);
        if(!result.Succeeded)
        {
            TempData[ToastMessages.Error] = $"Erro - {string.Join(", ", result.Errors)}";
            return View(department);
        }

        TempData[ToastMessages.Success] = "Setor atualizado com sucesso!";
        return RedirectToAction(nameof(Index));
    }

    // GET: Departments/Delete/5
    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null) return NotFound();

        var model = await _departmentService.GetViewModelByIdAsync(id.Value);
        if (model == null) return NotFound();

        return View(model);
    }

    // POST: Departments/Delete/5
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        if (id == Guid.Empty) return NotFound();

        Result result = await _departmentService.SoftDeleteAsync(id);
        if(!result.Succeeded)
        {
            TempData[ToastMessages.Error] = $"Erro - {string.Join(", ", result.Errors)}.";
            return RedirectToAction(nameof(Index));
        }

        TempData[ToastMessages.Success] = "Setor atualizado com sucesso!";
        return RedirectToAction(nameof(Index));
    }

    private async Task IfStaffAddViewBagViewModel()
    {
        if (User.IsInRole(Role.Staff))
        {
            var companies = await _companyService.GetCompaniesViewModelsAsync();
            ViewBag.Companies = companies;
        }
    }
}
