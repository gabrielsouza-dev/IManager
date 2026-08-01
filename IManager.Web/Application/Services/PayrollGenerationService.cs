using IManager.Web.Application.Interfaces;
using IManager.Web.Domain.Consts;
using IManager.Web.Domain.Entities.Payrolls;
using IManager.Web.Domain.Entities.TimeTrackings;
using IManager.Web.Domain.Entities.Users;
using IManager.Web.Domain.Interfaces.Persistence;
using IManager.Web.Domain.Interfaces.Repositories;
using IManager.Web.Presentation.Requests;
using IManager.Web.Shared;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IManager.Web.Application.Services;

public class PayrollGenerationService : IPayrollGenerationService
{
    private readonly IPayrollsRepository _payrollsRepository;
    private readonly IPayslipsRepository _payslipsRepository;
    private readonly ITimeEntryRepository _timeEntryRepository;
    private readonly IUserProfilesRepository _userProfilesRepository;

    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PayrollGenerationService> _logger;

    public PayrollGenerationService(IPayrollsRepository payrollsRepository, IPayslipsRepository payslipsRepository, 
        ITimeEntryRepository timeEntryRepository, IUserProfilesRepository userProfilesRepository, 
        IUnitOfWork unitOfWork, ILogger<PayrollGenerationService> logger)
    {
        _payrollsRepository = payrollsRepository;
        _payslipsRepository = payslipsRepository;
        _timeEntryRepository = timeEntryRepository;
        _userProfilesRepository = userProfilesRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> ProcessAsync(Guid companyId, ProcessPayrollRequest request)
    {
        var summaries = await _timeEntryRepository.GetProcessPayrollSummariesAsync(companyId, request);

        IEnumerable<string> errors = Validate(summaries);

        if (errors.Any() && !request.IsForced)
            return new Result(errors);

        //TODO: atualmente é cancelado todo processamento caso haja algum erro, mesmo que seja apenas um funcionário. Avaliar se é melhor processar os funcionários válidos e retornar os inválidos.
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var payroll = await GetOrCreatePayrollAsync(companyId, request);

            foreach (var employeeId in request.EmployeeIds)
                await ProcessEmployee(companyId, employeeId, payroll, request);

            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(
                ex,
                "Erro ao processar a folha de pagamento, competência {CompetenceDate}",
                request.CompetenceDate);
            return Result.Fail($"Ocorreu um erro ao processar a folha de pagamento.");
        }

        return Result.Ok();
    }

    private async Task ProcessEmployee(Guid companyId, Guid employeeId, Payroll payroll, ProcessPayrollRequest request)
    {
        var userProfile = await GetUserById(employeeId);
        if (userProfile == null || userProfile.Role != Role.User)
            throw new ArgumentException("O perfil do usuário é inválido.", nameof(userProfile));

        var timeentries = await _timeEntryRepository.GetTimeEntriesByCompetence(companyId, employeeId, request.CompetenceDate);
        if (timeentries.Count() == 0)
            throw new ArgumentException("Registros de ponto não localizados.", nameof(timeentries));

        var payslip = new Payslip()
        {
            EmployeeId = employeeId,
            PayrollId = payroll.Id,
            TimeEntries = timeentries
        };

        CalculatePayslip(payslip, userProfile);

        await _payslipsRepository.AddAsync(payslip);
    }

    private TimeSpan CalculateNightshiftHours(IEnumerable<TimeCheck> checks)
    {
        var sorted = checks.OrderBy(c => c.TimestampLocal).ToList();

        TimeSpan totalHours = TimeSpan.Zero;

        for (int i = 0; i + 1 < sorted.Count; i += 2)
        {
            var workStart = sorted[i].TimestampLocal;
            var workEnd = sorted[i + 1].TimestampLocal;

            totalHours += GetNightOverlap(workStart, workEnd);
        }

        return totalHours;
    }

    // Verifica as janelas noturnas (22h -> 05h do dia seguinte) que podem tocar esse intervalo.
    private TimeSpan GetNightOverlap(DateTime workStart, DateTime workEnd)
    {
        TimeSpan overlap = TimeSpan.Zero;

        // Identifica os dias que podem conter a janela noturna
        var candidateDates = new[] { workStart.Date.AddDays(-1), workStart.Date, workEnd.Date }.Distinct();

        foreach (var date in candidateDates)
        {
            var nightStart = date.AddHours(22);
            var nightEnd = date.AddDays(1).AddHours(5);

            var overlapStart = workStart > nightStart ? workStart : nightStart;
            var overlapEnd = workEnd < nightEnd ? workEnd : nightEnd;

            if (overlapEnd > overlapStart)
                overlap += overlapEnd - overlapStart;
        }

        return overlap;
    }

    private TimeEntrySummaryResult CalculateWithTimeBank(TimeEntry timeEntry, TimeSpan dailyHours)
    {
        var timeEntrySummary = new TimeEntrySummaryResult();
        var extra = timeEntry.WorkedHours - dailyHours;

        timeEntrySummary.RegularHours += timeEntry.WorkedHours < dailyHours ? timeEntry.WorkedHours : dailyHours;
        timeEntrySummary.OvertimeHours += extra > TimeSpan.Zero ? extra : TimeSpan.Zero;

        return timeEntrySummary;
    }

    private TimeEntrySummaryResult CalculateWithOutTimeBank(TimeEntry timeEntry, TimeSpan dailyHours)
    {
        var timeEntrySummary = new TimeEntrySummaryResult();

        if (timeEntry.WorkedHours > dailyHours)
        {
            timeEntrySummary.OvertimeHours = timeEntry.WorkedHours - dailyHours;
            timeEntrySummary.RegularHours = dailyHours;
        }
        else
        {
            timeEntrySummary.OvertimeHours = TimeSpan.Zero;
            timeEntrySummary.RegularHours = timeEntry.WorkedHours;
        }

        return timeEntrySummary;
    }

    private Payslip CalculatePayslip(Payslip payslip, UserProfile userProfile)
    {

        foreach (var timeEntry in payslip.TimeEntries)
        {
            var dailyHours = userProfile.JobTitle.DailyHours;

            var result = userProfile.JobTitle.IsTimeBank
                ? CalculateWithTimeBank(timeEntry, dailyHours)
                : CalculateWithOutTimeBank(timeEntry, dailyHours);

            payslip.RegularHours += result.RegularHours;
            payslip.OvertimeHours += result.OvertimeHours;
            payslip.NightShiftHours += CalculateNightshiftHours(timeEntry.Checks);
        }

        //TODO: Implementar quais dias o Jobtitle trabalha, para calcular o salário por hora corretamente.
        var salaryByDay = (userProfile.BaseSalary / 30 /*quantidade de dias a trabalhar*/ );
        var salaryByHour = (salaryByDay / (decimal)userProfile.JobTitle.DailyHours.TotalHours);

        payslip.RegularSalary = (decimal)payslip.RegularHours.TotalHours * salaryByHour;
        payslip.NightShiftAdditionals = payslip.NightShiftHours > TimeSpan.Zero
            ? (decimal)payslip.NightShiftHours.TotalHours * salaryByHour * 0.2m  // 20% é valor de hora noturna - HARDCODE
            : 0m;
        payslip.OvertimeAdditionals = (decimal)payslip.OvertimeHours.TotalHours * salaryByHour * 1.5m; // 1.5 é valor de extra - HARDCODE
        payslip.HazardAdditionals = userProfile.JobTitle.IsHazard ? (userProfile.BaseSalary * 0.3m) : 0m;  // 30% de adicional de periculosidade - HARDCODE
        payslip.UnhealthyAdditionals = userProfile.JobTitle.IsUnhealthy ? (userProfile.BaseSalary * 0.2m) : 0m; // 20% de adicional de insalubridade - HARDCODE

        payslip.TotalExtraEarnings = payslip.OvertimeAdditionals + payslip.HazardAdditionals + payslip.UnhealthyAdditionals + payslip.Commission + payslip.NightShiftAdditionals;

        //TODO: Implementar tabela de comissão e cálculo de comissão, caso o cargo seja comissionado.
        //payslip.Commission = userProfile.JobTitle.IsCommissioned ? valor da comissão : 0m; 
        payslip.GrossSalary = payslip.RegularSalary + payslip.TotalExtraEarnings;
        payslip.INSSDeduction = CalculateDeduction(DeductionType.INSS, payslip.GrossSalary);

        var taxBase = payslip.GrossSalary - payslip.INSSDeduction;
        payslip.IRRFDeduction = CalculateDeduction(DeductionType.IRRF, taxBase);

        payslip.TotalDeductions = payslip.INSSDeduction + payslip.IRRFDeduction + payslip.OtherDeductions;

        payslip.NetSalary = payslip.GrossSalary - payslip.TotalDeductions;
        return payslip;
    }

    private decimal CalculateDeduction(DeductionType type, decimal value)
    {
        var table = GetDeductionTable(type);
        var rate = table.Rates.FirstOrDefault(r => value >= r.MinValue && value <= r.MaxValue);

        if (rate == null)
            throw new InvalidOperationException($"Nenhuma faixa encontrada para {type} com valor {value}.");

        return (value * rate.Rate) - rate.Deduction;
    }

    //TODO: Implementar tabela de dedução no banco de dados.
    private DeductionTable GetDeductionTable(DeductionType type)
    {
        if(type == DeductionType.IRRF)
        {
            return new DeductionTable()
            {
                Type = DeductionType.IRRF,
                Rates = new List<DeductionRate>()
                {
                    new DeductionRate() { Rate = 0.000m, MinValue = 0m,       MaxValue = 2428.80m, Deduction = 0m },
                    new DeductionRate() { Rate = 0.075m, MinValue = 2428.81m, MaxValue = 2826.65m, Deduction = 182.16m },
                    new DeductionRate() { Rate = 0.150m, MinValue = 2826.66m, MaxValue = 3751.05m, Deduction = 394.16m },
                    new DeductionRate() { Rate = 0.225m, MinValue = 3751.06m, MaxValue = 4664.68m, Deduction = 675.49m },
                    new DeductionRate() { Rate = 0.275m, MinValue = 4664.69m, MaxValue = decimal.MaxValue, Deduction = 908.73m }
                }
            };
        } 
        else
        {
            return new DeductionTable()
            {
                Type = DeductionType.INSS,
                Rates = new List<DeductionRate>()
                {
                    new DeductionRate() { Rate = 0.075m, MinValue = 0m,       MaxValue = 1621.00m, Deduction = 0m },
                    new DeductionRate() { Rate = 0.090m, MinValue = 1621.01m, MaxValue = 2902.84m, Deduction = 24.32m },
                    new DeductionRate() { Rate = 0.120m, MinValue = 2902.85m, MaxValue = 4354.27m, Deduction = 111.40m },
                    new DeductionRate() { Rate = 0.140m, MinValue = 4354.28m, MaxValue = 8475.55m, Deduction = 198.49m }
                }
            };
        }
    }
    private async Task<Payroll> GetOrCreatePayrollAsync(Guid companyId, ProcessPayrollRequest request)
    {
        var payroll = await _payrollsRepository.FirstOrDefaultAsync(p => p.CompanyId == companyId && p.Competence == request.CompetenceDate);
        if (payroll == null)
        {
            payroll = new Payroll(companyId, request.CompetenceDate);
            await _payrollsRepository.AddAsync(payroll);
            await _unitOfWork.SaveChangesAsync();
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

    private async Task<UserProfile?> GetUserById(Guid id) => await _userProfilesRepository.GetByIdAsync(id, q => q.Include(u => u.JobTitle));
}

public record ProcessPayrollSummary(Guid EmployeId, string EmployeName, DateOnly Date, bool IsConcistent, int CheckCount);

public class TimeEntrySummaryResult
{
    public TimeSpan RegularHours { get; set; }
    public TimeSpan OvertimeHours { get; set; }
    public TimeSpan NightShiftHours { get; set; }
}

public class DeductionTable 
{ 
    public DeductionType Type { get; set; }
    public ICollection<DeductionRate> Rates { get; set; } = new List<DeductionRate>();
}

public class DeductionRate
{
    public decimal Rate { get; set; }
    public decimal MinValue { get; set; }
    public decimal MaxValue { get; set; }
    public decimal Deduction { get; set; }
}

public enum DeductionType
{
    INSS,
    IRRF
}