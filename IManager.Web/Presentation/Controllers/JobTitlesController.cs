using IManager.Web.Application.Interfaces;
using IManager.Web.Domain.Consts;
using IManager.Web.Presentation.ViewModels.JobTitles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Result = IManager.Web.Shared.Result;

namespace IManager.Web.Presentation.Controllers;

[Authorize(Policy = Policies.StaffOrAdmin)]
public class JobTitlesController : Controller
{
    private readonly  IJobTitleService _jobTitleService;
    private readonly ICompanyService _companiesService;
    private readonly IDepartmentService _departmentsService;

    public JobTitlesController(IJobTitleService jobTitleService, ICompanyService companiesService, IDepartmentService departmentsService)
    {
        _jobTitleService = jobTitleService;
        _companiesService = companiesService;
        _departmentsService = departmentsService;
    }

    // GET: JobTitles
    public async Task<IActionResult> Index([FromQuery] string search, [FromQuery] ActiveFilter active = ActiveFilter.Active, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        PagedResult<IndexJobTitleViewModel> model;
        if (User.IsInRole(Role.Staff))
        {
            model = await _jobTitleService.GetPagedAsync(search, active, page, pageSize);
        }
        else
        {
            var companyId = Guid.Parse(User.FindFirst("CompanyId")!.Value);
            model = await _jobTitleService.GetPagedAsync(search, active, page, pageSize, companyId);
        }

        return View(model);
    }

    // GET: JobTitles/Details/5
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var model = await _jobTitleService.GetDetailsModelViewById(id.Value);
        if (model == null) return NotFound();

        return View(model);
    }

    // GET: JobTitles/Create
    public async Task<IActionResult> Create()
    {
        var model = new CreateJobTitleViewModel();
        await AddViewBagHierarchyViewModel(model);

        return View(model);
    }

    // POST: JobTitles/Create
    [HttpPost]
    public async Task<IActionResult> Create(CreateJobTitleViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await AddViewBagHierarchyViewModel(model);
            return View(model);
        }

        Result result = await _jobTitleService.CreateAsync(model);

        if(!result.Succeeded)
        {
            await AddViewBagHierarchyViewModel(model);
            TempData[ToastMessages.Error] = $"Erro - {string.Join(", ", result.Errors)}.";
            return View(model);
        }

        TempData[ToastMessages.Success] = "Cargo cadastrado com sucesso!";

        return RedirectToAction(nameof(Index));
    }

    // GET: JobTitles/Edit/5
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();

        var jobTitle = await _jobTitleService.GetEditModelViewByIdAsync(id.Value);
        if (jobTitle == null) return NotFound();

        return View(jobTitle);
    }

    // POST: JobTitles/Edit/5
    [HttpPost]
    public async Task<IActionResult> Edit(Guid id, EditJobTitleViewModel model)
    {
        if (id != model.Id) return NotFound();

        if (!ModelState.IsValid) return View(model);

        var result = await _jobTitleService.UpdateAsync(model);
        
        if(!result.Succeeded)
        {
            TempData[ToastMessages.Error] = $"Erro - {string.Join(", ", result.Errors)}.";
            return View(model);
        }

        TempData[ToastMessages.Success] = "Cargo atualizado com sucesso!";
        return RedirectToAction(nameof(Index));
    }

    // GET: JobTitles/Delete/5
    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null) return NotFound();

        var jobTitle = await _jobTitleService.GetModelViewByIdAsync(id.Value);
        if (jobTitle == null) return NotFound();

        return View(jobTitle);
    }

    // POST: JobTitles/Delete/5
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        if (id == Guid.Empty) return NotFound();

        var result = await _jobTitleService.SoftDeleteAsync(id);
        if (!result.Succeeded)
        {
            TempData[ToastMessages.Error] = $"Erro - {string.Join(", ", result.Errors)}.";
            return RedirectToAction(nameof(Index));
        }

        TempData[ToastMessages.Success] = "Cargo atualizado com sucesso!";
        return RedirectToAction(nameof(Index));
    }

    #region Helpers
    private async Task AddViewBagHierarchyViewModel(CreateJobTitleViewModel model)
    {
        if (User.IsInRole(Role.Staff))
            ViewBag.Companies = await _companiesService.GetCompaniesHierarchyViewModelAsync();
        else
        {
            var companyId = GetCompanyId();
            ViewBag.Departments = await _departmentsService.GetDepartmentsHierarchyViewModelAsync(companyId);
            model.CompanyId = companyId;
        }
    }

    private Guid GetCompanyId() => Guid.Parse(User.FindFirst("CompanyId")!.Value);
    #endregion
}
