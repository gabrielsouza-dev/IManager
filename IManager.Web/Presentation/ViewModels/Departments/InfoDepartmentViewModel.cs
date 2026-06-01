namespace IManager.Web.Presentation.ViewModels.Departments;

public class InfoDepartmentViewModel
{
    public int JobTitleCount { get; set; }
    public int EmployeeCount { get; set; }
    public decimal AverageSalary { get; set; }
    public string HighestCostJobTitleName { get; set; } = string.Empty;
    public decimal HighestCostJobTitleValue { get; set; }
    public string MostCommonJobTitle { get; set; } = string.Empty;
}