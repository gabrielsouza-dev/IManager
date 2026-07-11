using IManager.Web.Application.Interfaces;
using IManager.Web.Application.Services;
using IManager.Web.Domain.Entities.Users;
using Microsoft.AspNetCore.Identity;

namespace IManager.Web.Presentation.Extensions;

public static class ServiceExtensions
{
    public static void AddServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IAccountService, AccountService>();
        builder.Services.AddScoped<IUserService, UserService>();

        builder.Services.AddScoped<ICompanyService, CompanyService>();
        builder.Services.AddScoped<IDepartmentService, DepartmentService>();
        builder.Services.AddScoped<IJobTitleService, JobTitleService>();

        builder.Services.AddScoped<ITimeEntryService, TimeEntryService>();
        
        builder.Services.AddScoped<IEmailService, EmailService>();
        builder.Services.AddScoped<IEmailSender<User>, IdentityEmailSender>();
    }
}