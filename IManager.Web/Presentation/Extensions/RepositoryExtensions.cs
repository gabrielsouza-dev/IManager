using IManager.Web.Data.Repositories;
using IManager.Web.Domain.Interfaces.Repositories;

namespace IManager.Web.Presentation.Extensions;

public static class RepositoryExtensions
{
    public static void AddRepositories(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        builder.Services.AddScoped<IUserProfilesRepository, UserProfilesRepository>();
        builder.Services.AddScoped<IJobTitlesRepository, JobTitlesRepository>();
        builder.Services.AddScoped<ICompaniesRepository, CompaniesRepository>();
        builder.Services.AddScoped<IDepartmentsRepository, DepartmentsRepository>();
        builder.Services.AddScoped<ITimeEntryRepository, TimeEntryRepository>();
        builder.Services.AddScoped<IPayrollsRepository, PayrollsRepository>();

    }
}