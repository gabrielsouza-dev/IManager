using IManager.Web.Application.Services;
using IManager.Web.Domain.Consts;
using IManager.Web.Domain.Entities.Companies;
using IManager.Web.Domain.Entities.TimeTrackings;
using IManager.Web.Domain.Entities.Users;
using IManager.Web.Domain.Interfaces.Persistence;
using IManager.Web.Domain.Interfaces.Repositories;
using IManager.Web.Presentation.Requests;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;

namespace Imanager.Test;

public class PayrollGenerationTests
{
    private readonly ITestOutputHelper _output;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserProfilesRepository> _userProfileRepositoryMock;
    private readonly Mock<ITimeEntryRepository> _timeEntryRepositoryMock;
    private readonly Mock<IPayrollsRepository> _payrollRepositoryMock;
    private readonly Mock<IPayslipsRepository> _PayslipsRepositoryMock;
    private readonly Mock<ILogger<PayrollGenerationService>> _loggerMock;

    public PayrollGenerationTests(ITestOutputHelper output)
    {
        _output = output;
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userProfileRepositoryMock = new Mock<IUserProfilesRepository>();
        _timeEntryRepositoryMock = new Mock<ITimeEntryRepository>();
        _payrollRepositoryMock = new Mock<IPayrollsRepository>();
        _PayslipsRepositoryMock = new Mock<IPayslipsRepository>();
        _loggerMock = new Mock<ILogger<PayrollGenerationService>>();
        _loggerMock
            .Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Callback(new InvocationAction(invocation =>
            {
                var logLevel = (LogLevel)invocation.Arguments[0];
                var exception = invocation.Arguments[3] as Exception;
                var formatter = invocation.Arguments[4];

                // Usa reflection pra invocar o formatter, já que o tipo genérico é It.IsAnyType
                var invokeMethod = formatter.GetType().GetMethod("Invoke");
                var message = invokeMethod?.Invoke(formatter, new[] { invocation.Arguments[2], exception })?.ToString();

                _output.WriteLine($"[{logLevel}] {message}");
                if (exception != null)
                    _output.WriteLine(exception.ToString());
            }));
    }
    [Fact]
    public async Task ProcessAsync_TimeEntryConsistenteEIsForcedFalse_DeveProcessar()
    {
        var companyId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var request = new ProcessPayrollRequest(
            EmployeeIds: [employeeId],
            CompetenceDate: new DateOnly(2026, 7, 1),
            IsForced: false);

        _timeEntryRepositoryMock
            .Setup(x => x.GetTimeEntriesByCompetence(companyId, employeeId, request.CompetenceDate))
            .ReturnsAsync(new List<TimeEntry>
            {
                new TimeEntry { EmployeeId = employeeId}
            });

        _timeEntryRepositoryMock
            .Setup(x => x.GetProcessPayrollSummariesAsync(companyId, request))
            .ReturnsAsync(
            [
                    new ProcessPayrollSummary(
                        EmployeId: employeeId,
                        EmployeName: "Test",
                        Date: new DateOnly(2026, 7, 15),
                        IsConcistent: true,
                        CheckCount: 3)
            ]
        );

        var userProfile = new UserProfile
        {
            Id = employeeId,
            Role = Role.User,
            BaseSalary = 3000m,
            JobTitle = new JobTitle
            {
                DailyHours = TimeSpan.FromHours(8),
                IsHazard = false,
                IsUnhealthy = false
            }
        };

        _userProfileRepositoryMock
            .Setup(x => x.GetByIdAsync(employeeId, It.IsAny<Func<IQueryable<UserProfile>, IQueryable<UserProfile>>>()))
            .ReturnsAsync(userProfile);

        var service = new PayrollGenerationService(_payrollRepositoryMock.Object, 
            _PayslipsRepositoryMock.Object,
            _timeEntryRepositoryMock.Object, 
            _userProfileRepositoryMock.Object, 
            _unitOfWorkMock.Object, _loggerMock.Object);

        var result = await service.ProcessAsync(companyId, request);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task ProcessAsync_TimeEntryInconsistenteEIsForcedFalse_DeveRetornarErro()
    {
        var companyId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var request = new ProcessPayrollRequest(
            EmployeeIds: [employeeId],
            CompetenceDate: new DateOnly(2026, 7, 1),
            IsForced: false);

        _timeEntryRepositoryMock
            .Setup(x => x.GetTimeEntriesByCompetence(companyId, employeeId, request.CompetenceDate))
            .ReturnsAsync(new List<TimeEntry>
            {
                        new TimeEntry { EmployeeId = employeeId}
            });

        _timeEntryRepositoryMock
            .Setup(x => x.GetProcessPayrollSummariesAsync(companyId, request))
            .ReturnsAsync(
            [
                    new ProcessPayrollSummary(
                        EmployeId: employeeId,
                        EmployeName: "Test",
                        Date: new DateOnly(2026, 7, 15),
                        IsConcistent: false,
                        CheckCount: 4)
            ]
        );

        var userProfile = new UserProfile
        {
            Id = employeeId,
            Role = Role.User,
            BaseSalary = 3000m,
            JobTitle = new JobTitle
            {
                DailyHours = TimeSpan.FromHours(8),
                IsHazard = false,
                IsUnhealthy = false
            }
        };

        _userProfileRepositoryMock
            .Setup(x => x.GetByIdAsync(employeeId, It.IsAny<Func<IQueryable<UserProfile>, IQueryable<UserProfile>>>()))
            .ReturnsAsync(userProfile);

        var service = new PayrollGenerationService(_payrollRepositoryMock.Object,
            _PayslipsRepositoryMock.Object,
            _timeEntryRepositoryMock.Object,
            _userProfileRepositoryMock.Object,
            _unitOfWorkMock.Object, _loggerMock.Object);

        var result = await service.ProcessAsync(companyId, request);

        Assert.False(result.Succeeded);
        Assert.Contains(
            result.Errors,
            e => e.Contains("inconsistente"));
    }

    [Fact]
    public async Task ProcessAsync_TimeEntryInconsistenteEIsForcedTrue_DeveProcessar()
    {
        var companyId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var request = new ProcessPayrollRequest(
            EmployeeIds: [employeeId],
            CompetenceDate: new DateOnly(2026, 7, 1),
            IsForced: true);

        _timeEntryRepositoryMock
            .Setup(x => x.GetTimeEntriesByCompetence(companyId, employeeId, request.CompetenceDate))
            .ReturnsAsync(new List<TimeEntry>
            {
                        new TimeEntry { EmployeeId = employeeId}
            });

        _timeEntryRepositoryMock
            .Setup(x => x.GetProcessPayrollSummariesAsync(companyId, request))
            .ReturnsAsync(
            [
                    new ProcessPayrollSummary(                        
                        EmployeId: employeeId,
                        EmployeName: "Test",
                        Date: new DateOnly(2026, 7, 15),
                        IsConcistent: false,
                        CheckCount: 3)
            ]
        );

        var userProfile = new UserProfile
        {
            Id = employeeId,
            Role = Role.User,
            BaseSalary = 3000m,
            JobTitle = new JobTitle
            {
                DailyHours = TimeSpan.FromHours(8),
                IsHazard = false,
                IsUnhealthy = false
            }
        };

        _userProfileRepositoryMock
            .Setup(x => x.GetByIdAsync(employeeId, It.IsAny<Func<IQueryable<UserProfile>, IQueryable<UserProfile>>>()))
            .ReturnsAsync(userProfile);

        var service = new PayrollGenerationService(_payrollRepositoryMock.Object,
            _PayslipsRepositoryMock.Object,
            _timeEntryRepositoryMock.Object,
            _userProfileRepositoryMock.Object,
            _unitOfWorkMock.Object, _loggerMock.Object);


        var result = await service.ProcessAsync(companyId, request);

        Assert.True(result.Succeeded);
        Assert.DoesNotContain(
            result.Errors,
            e => e.Contains("inconsistente"));
    }
}

