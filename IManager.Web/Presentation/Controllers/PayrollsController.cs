using IManager.Web.Application.Interfaces;
using IManager.Web.Domain.Consts;
using IManager.Web.Presentation.Requests;
using IManager.Web.Presentation.ViewModels.Payrolls;
using IManager.Web.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Text.Json;

namespace IManager.Web.Presentation.Controllers;

[Authorize(Roles = Role.Admin)]
public class PayrollsController : Controller
{

    private readonly IPayrollGenerationService _payrollGenerationService;
    private readonly IPayrollQueryService _payrollQueryService;

    public PayrollsController(IPayrollGenerationService payrollGenerationService, IPayrollQueryService payrollQueryService)
    {
        _payrollGenerationService = payrollGenerationService;
        _payrollQueryService = payrollQueryService;
    }

    // GET: Payrolls
    public async Task<IActionResult> Index()
    {
        var companyId = GetCompanyId();
        IEnumerable<DateOnly> model = await _payrollQueryService.GetCompetencesAsync(companyId);

        return View(model);
    }

    public async Task<IActionResult> Process([FromBody] ProcessPayrollRequest request)
    {
        var companyId = GetCompanyId();

        Result result = await _payrollGenerationService.ProcessAsync(companyId, request);

        if(!result.Succeeded)
        {
            if(request.IsForced)
                TempData[ToastMessages.Error] = $"Falha ao processar a folha: {string.Join(", ", result.Errors)}";
    
            return BadRequest(result);
        }

        TempData[ToastMessages.Success] = $"Folha processada com sucesso.";
        return Ok();
    }

    public async Task<IActionResult> _payrollPendingTable(DateOnly competenceDate)
    {
        var companyId = GetCompanyId();

        var model = await _payrollQueryService.GetPendingPayrollAsync(companyId, competenceDate);

        return PartialView("_payrollPendingTable", model);
    }

    public async Task<IActionResult> _payrollProcessedTable(DateOnly competenceDate)
    {
        var companyId = GetCompanyId();

        var model = await _payrollQueryService.GetProcessedPayrollAsync(companyId, competenceDate);

        return PartialView("_payrollProcessedTable", model);
    }

    private Guid GetCompanyId()
    {
        return Guid.Parse(User.FindFirst("CompanyId")!.Value);
    }
}
