using IManager.Web.Application.Interfaces;
using IManager.Web.Domain.Consts;
using IManager.Web.Shared.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using System.Security.Claims;

namespace IManager.Web.Presentation.Controllers;

[Authorize(Roles = Role.User)]
public class PayslipsController : Controller
{
    private readonly IPayslipService _payslipService;

    public PayslipsController(IPayslipService payslipService)
    {
        _payslipService = payslipService;
    }

    public async Task<IActionResult> Index()
    {
        var userId = GetUserId();

        var model = await _payslipService.GetPayslipByUserAsync(userId);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> VisualizePdf(Guid id)
    {
        var userId = GetUserId();
        var payslip = await _payslipService.GetByIdAsync(userId, id);

        if (payslip is null)
            return NotFound();

        var document = new PayslipDocument(payslip);

        byte[] pdf = document.GeneratePdf();

        return File(pdf, "application/pdf");
    }

    [HttpGet]
    public async Task<IActionResult> DownloadPdf(Guid id)
    {
        var userId = GetUserId();
        var payslip = await _payslipService.GetByIdAsync(userId, id);

        if (payslip is null)
            return NotFound();

        var document = new PayslipDocument(payslip);

        byte[] pdf = document.GeneratePdf();

        return File(
            pdf,
            "application/pdf",
            $"holerite-{payslip.EmployeeName.Split(' ')[0].ToLower()}-{payslip.ReferenceMonth.ToString()}-{payslip.ReferenceYear}.pdf"
        );
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
}