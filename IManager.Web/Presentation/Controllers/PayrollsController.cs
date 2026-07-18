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
        IEnumerable<DateOnly> model = await _payrollQueryService.GetPendingCompetencesAsync(companyId);

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
    // GET: Payrolls/Details/5
    // public async Task<IActionResult> Details(Guid? id)
    // {
    //     if (id == null) return NotFound();

    //     var payroll = await _payrollRepository.GetByIdAsync(id.Value, q => q.Include(p => p.Company));

    //     if (payroll == null) return NotFound();

    //     return View(payroll);
    // }

    // GET: Payrolls/Create
    // public IActionResult Create()
    // {
    //     return View();
    // }

    // POST: Payrolls/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.

    //[HttpPost]
    // [ValidateAntiForgeryToken]
    // public async Task<IActionResult> Create([Bind("CompanyId,PeriodStart,PeriodEnd,Id,CreatedAt,LastModified")] Payroll payroll)
    // {
    //     if (ModelState.IsValid)
    //     {
    //         await _payrollRepository.AddAsync(payroll);
    //         return RedirectToAction(nameof(Index));
    //     }

    //     return View(payroll);
    // }

    // GET: Payrolls/Edit/5
    // public async Task<IActionResult> Edit(Guid? id)
    // {
    //     if (id == null) return NotFound();

    //     var payroll = await _payrollRepository.GetByIdAsync(id.Value);
    //     if (payroll == null) return NotFound();

    //     return View(payroll);
    // }

    // POST: Payrolls/Edit/5
    //  To protect from overposting attacks, enable the specific properties you want to bind to.
    //  For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    // [HttpPost]
    // [ValidateAntiForgeryToken]
    // public async Task<IActionResult> Edit(Guid id, [Bind("CompanyId,PeriodStart,PeriodEnd,Id,CreatedAt,LastModified")] Payroll payroll)
    // {
    //     if (id != payroll.Id)
    //     {
    //         return NotFound();
    //     }

    //     if (ModelState.IsValid)
    //     {
    //         try
    //         {
    //             await _payrollRepository.UpdateAsync(payroll);

    //         }
    //         catch (DbUpdateConcurrencyException)
    //         {
    //             if (!await PayrollExistsAsync(payroll.Id))
    //             {
    //                 return NotFound();
    //             }
    //             else
    //             {
    //                 throw;
    //             }
    //         }
    //         return RedirectToAction(nameof(Index));
    //     }
    //     return View(payroll);
    // }

    // GET: Payrolls/Delete/5
    // public async Task<IActionResult> Delete(Guid? id)
    // {
    //     if (id == null) return NotFound();

    //     var payroll = await _payrollRepository.GetByIdAsync(id.Value, q => q.Include(p => p.Company));

    //     if (payroll == null) return NotFound();

    //     return View(payroll);
    // }

    // POST: Payrolls/Delete/5
    // [HttpPost, ActionName("Delete")]
    // [ValidateAntiForgeryToken]
    // public async Task<IActionResult> DeleteConfirmed(Guid id)
    // {
    //     var payroll = await _payrollRepository.GetByIdAsync(id);
    //     if (payroll != null)
    //     {
    //         await _payrollRepository.SoftDeleteAsync(payroll);
    //     }

    //     return RedirectToAction(nameof(Index));
    // }

    private Guid GetCompanyId()
    {
        return Guid.Parse(User.FindFirst("CompanyId")!.Value);
    }
}
