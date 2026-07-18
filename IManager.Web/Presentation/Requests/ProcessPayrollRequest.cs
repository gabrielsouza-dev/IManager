namespace IManager.Web.Presentation.Requests;

public record ProcessPayrollRequest(Guid[] EmployeeIds, DateOnly CompetenceDate, bool IsForced);