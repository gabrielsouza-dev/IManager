namespace IManager.Web.Presentation.ViewModels.Payrolls;

public class ProcessedPayrollViewModel
{
    public Guid Id { get; set; }

    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeDepartment { get; set; } = string.Empty;
    public string EmployeeJobtitle{ get; set; } = string.Empty;

    public decimal GrossSalary { get; set; }
    public decimal NetSalary { get; set; }
    public decimal RegularSalary { get; set; }
    public decimal TotalExtraEarnings { get; set; }
    public decimal TotalDeductions { get; set; }
}