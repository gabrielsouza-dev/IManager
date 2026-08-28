namespace IManager.Web.Presentation.ViewModels.Payslips;

public class PayslipViewModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Funcionário
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeDocument { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;

    // Empresa
    public string CompanyName { get; set; } = string.Empty;
    public string CompanyDocument { get; set; } = string.Empty;

    // Competência
    public int ReferenceMonth { get; set; }
    public int ReferenceYear { get; set; }

    // Totais
    public decimal GrossSalary { get; set; }
    public decimal TotalExtraEarnings { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetSalary { get; set; }

    // Proventos
    public decimal RegularSalary { get; set; }
    public decimal OvertimeAdditionals { get; set; }
    public decimal HazardAdditionals { get; set; }
    public decimal UnhealthyAdditionals { get; set; }
    public decimal NightShiftAdditionals { get; set; }
    public decimal Commission { get; set; }

    // Descontos
    public decimal INSSDeduction { get; set; }
    public decimal IRRFDeduction { get; set; }
    public decimal OtherDeductions { get; set; }

    // Jornada
    public TimeSpan RegularHours { get; set; }
    public TimeSpan OvertimeHours { get; set; }
    public TimeSpan NightShiftHours { get; set; }
}