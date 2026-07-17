using IManager.Web.Data.Persistence;
using IManager.Web.Domain.Consts;
using IManager.Web.Domain.Entities.Payrolls;
using IManager.Web.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IManager.Web.Presentation.Controllers;

//TODO: Implementar a visualização de holerite
[Authorize(Roles = Role.User)]
public class PayslipsController : Controller
{
    private readonly IRepository<Payslip> _payslipRepository;

    public PayslipsController(IRepository<Payslip> payslipRepository)
    {
        this._payslipRepository = payslipRepository;
    }

    public async Task<IActionResult> Index()
    {
        var model = await _payslipRepository.GetAllAsync(q => q
                                                .Include(p => p.Employee)
                                                .Include(p => p.Payroll));
        return View(model);
    }

    // GET: Payslips/Details/5
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var payslip = await _payslipRepository.GetByIdAsync(id.Value, q => q
                                                .Include(p => p.Employee)
                                                .Include(p => p.Payroll));
        if (payslip == null) return NotFound();

        return View(payslip);
    }

    // GET: Payslips/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Payslips/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("PayrollId,EmployeeId,GrossSalary,OvertimeAdditionals,HazardPay,UnhealthyPay,Commission,INSSDeduction,IRRFDeduction,OtherDeductions,Id,CreatedAt,LastModified")] Payslip payslip)
    {
        if (ModelState.IsValid)
        {
            await _payslipRepository.AddAsync(payslip);
            return RedirectToAction(nameof(Index));
        }

        return View(payslip);
    }

    // GET: Payslips/Edit/5
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();

        var payslip = await _payslipRepository.GetByIdAsync(id.Value);
        if (payslip == null) return NotFound();

        return View(payslip);
    }

    // POST: Payslips/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, [Bind("PayrollId,EmployeeId,GrossSalary,OvertimeAdditionals,HazardPay,UnhealthyPay,Commission,INSSDeduction,IRRFDeduction,OtherDeductions,Id,CreatedAt,LastModified")] Payslip payslip)
    {
        if (id != payslip.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                await _payslipRepository.UpdateAsync(payslip);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await PayslipExistsAsync(payslip.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }

        return View(payslip);
    }

    // GET: Payslips/Delete/5
    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null) return NotFound();

        var payslip = await _payslipRepository.GetByIdAsync(id.Value, q => q
                                                .Include(p => p.Employee)
                                                .Include(p => p.Payroll));
        if (payslip == null) return NotFound();

        return View(payslip);
    }

    // POST: Payslips/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var payslip = await _payslipRepository.GetByIdAsync(id);
        if (payslip != null) 
            await _payslipRepository.SoftDeleteAsync(payslip);

        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> PayslipExistsAsync(Guid id)
    {
        return await _payslipRepository.ExistsAsync(p => p.Id == id);
    }
}
