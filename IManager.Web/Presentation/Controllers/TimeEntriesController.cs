using IManager.Web.Application.Interfaces;
using IManager.Web.Domain.Consts;
using IManager.Web.Presentation.Requests;
using IManager.Web.Presentation.ViewModels.TimeEntries;
using IManager.Web.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IManager.Web.Presentation.Controllers;

[Authorize(Roles = Role.Admin)]
public class TimeEntriesController : Controller
{
    private readonly ITimeEntryService _timeEntryService;

    public TimeEntriesController(ITimeEntryService timeEntryService)
    {
        _timeEntryService = timeEntryService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var companyId = Guid.Parse(User.FindFirst("CompanyId")!.Value);

        IEnumerable<DateOnly> model = await _timeEntryService.GetPendingDates(companyId);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> _TimeEntryPendingTable([FromQuery] DateOnly date, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var companyId = Guid.Parse(User.FindFirst("CompanyId")!.Value);

        PagedResult<TimeEntryPending> model = await _timeEntryService.GetPendingPagedAsync(companyId, date, page, pageSize);
        return PartialView(model);
    }

    [HttpGet]
    public async Task<IActionResult> _TimeEntryAuditTable([FromQuery] DateOnly date, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var companyId = Guid.Parse(User.FindFirst("CompanyId")!.Value);

        PagedResult<TimeEntryAudit> model = await _timeEntryService.GetAuditPagedAsync(companyId, date, page, pageSize);
        return PartialView(model);
    }

    [HttpPost]
    public async Task<IActionResult> ManageTimeEntryAction([FromBody] TimeEntryActionRequest request)
    {
        if (request.Id == Guid.Empty)
            return BadRequest("ID inválido.");
        
        var auditorId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

        Result result;
        if (request.IsApprove)
            result = await _timeEntryService.ManageApprove(request.Id, auditorId);
        else
            result = await _timeEntryService.ManageReject(request.Id, request.Comment, auditorId);

        if (result.Succeeded)
            TempData[ToastMessages.Success] = "Processamento realizado com sucesso!";
        else
            TempData[ToastMessages.Error] = "Erro ao realizar o processamento!";

        return RedirectToAction("Index");
    }
}
