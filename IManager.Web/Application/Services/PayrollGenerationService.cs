using IManager.Web.Application.Interfaces;
using IManager.Web.Data.Repositories;
using IManager.Web.Domain.Entities.Payrolls;
using IManager.Web.Domain.Interfaces.Persistence;
using IManager.Web.Domain.Interfaces.Repositories;
using IManager.Web.Presentation.Requests;
using IManager.Web.Shared;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Org.BouncyCastle.Asn1.Ocsp;

namespace IManager.Web.Application.Services;

public class PayrollGenerationService : IPayrollGenerationService
{
    private readonly IPayrollsRepository _payrollsRepository;
    private readonly ITimeEntryRepository _timeEntryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PayrollGenerationService> _logger;

    public PayrollGenerationService(ITimeEntryRepository timeEntryRepository, IUnitOfWork unitOfWork, ILogger<PayrollGenerationService> logger)
    {
        _timeEntryRepository = timeEntryRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> ProcessAsync(Guid companyId, ProcessPayrollRequest request)
    {
        var summaries = await _timeEntryRepository.GetProcessPayrollSummariesAsync(companyId, request);

        IEnumerable<string> errors = Validate(summaries);

        if (errors.Count() > 0 && !request.IsForced)
            return new Result(errors);

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var payroll = await GetOrCreatePayrollAsync(companyId, request);

            foreach (var employeeId in request.EmployeeIds)
            {
                var payslip = new Payslip()
                {
                    EmployeeId = employeeId,
                    PayrollId = payroll.Id,
                };

                payroll.Payslips.Add(payslip);



            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(
                ex,
                "Erro ao processar a folha de pagamento. Empresa {CompanyId}, competência {CompetenceDate}, quantidade de funcionários {EmployeeCount}",
                companyId,
                request.CompetenceDate,
                request.EmployeeIds.Count());
            return Result.Fail($"Ocorreu um erro ao processar a folha de pagamento.");
        }

        return Result.Ok();
    }

    private async Task<Payroll> GetOrCreatePayrollAsync(Guid companyId, ProcessPayrollRequest request)
    {
        var payroll = await _payrollsRepository.FirstOrDefaultAsync(p => p.CompanyId == companyId && p.Competence == request.CompetenceDate);
        if (payroll == null)
        {
            payroll = new Payroll(companyId, request.CompetenceDate);
            await _payrollsRepository.AddAsync(payroll);
        }
        return payroll;
    }

    private IEnumerable<string> Validate(IEnumerable<ProcessPayrollSummary> summaries)
    {
        List<string> errors = new();

        foreach (var summary in summaries)
        {
            var isInconsistent = !summary.IsConcistent;
            var checkCount = summary.CheckCount;

            if (isInconsistent)
                errors.Add($"{summary.EmployeName} possui quantidade de registros inconsistentes dia {summary.Date} com {checkCount} registrados.");

            if (checkCount == 2)
                errors.Add($"{summary.EmployeName} possui apenas 2 registros dia {summary.Date}.");
        }

        return errors;
    }
}

public record ProcessPayrollSummary(Guid EmployeId, string EmployeName, DateOnly Date, bool IsConcistent, int CheckCount);