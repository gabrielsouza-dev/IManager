using IManager.Web.Application.Interfaces;
using IManager.Web.Data.Persistence;
using IManager.Web.Data.Seeder.Builders;
using IManager.Web.Data.Seeder.Interfaces;
using IManager.Web.Domain.Consts;
using IManager.Web.Domain.Entities.Users;
using IManager.Web.Presentation.Configurations;
using IManager.Web.Presentation.Requests;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;

namespace IManager.Web.Presentation.Extensions;

public static class InitializeExtensions
{
    private const int DemoCount = 3;
    private const int BatchSize = 10;
    private const int MaxDatabaseRetries = 10;

    public static async Task Initialize(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var serviceProvider = scope.ServiceProvider;

        var dbContext =
            serviceProvider.GetRequiredService<ApplicationDbContext>();

        var userManager =
            serviceProvider.GetRequiredService<UserManager<User>>();

        var roleManager =
            serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        var demoSeeder =
            serviceProvider.GetRequiredService<IDemoSeeder>();

        var demoOptions =
            serviceProvider
                .GetRequiredService<IOptions<DemoProfilesOptions>>()
                .Value;

        var payrollGenerationService =
            serviceProvider.GetRequiredService<IPayrollGenerationService>();

        await MigrateDatabaseAsync(dbContext);

        await SeedRolesAsync(roleManager);

        if (await userManager.Users.AnyAsync())
        {
            Log.Information("Seed ignorado. Já existem usuários cadastrados.");
            return;
        }

        Log.Information("Iniciando seed DEMO...");

        await demoSeeder.SeedAsync(
            DemoFixedSeedBuilder.Build(demoOptions)
        );

        dbContext.ChangeTracker.Clear();

        var requests = new Dictionary<Guid, List<ProcessPayrollRequest>>();

        for (var i = 0; i < DemoCount; i++)
        {
            Log.Information(
                "Iniciando DEMO {Current}/{Total}...",
                i + 1,
                DemoCount
            );

            var demoSeed = DemoSeedBuilder.Build();

            await demoSeeder.SeedAsync(demoSeed);

            AddPayrollRequests(
                requests,
                demoSeed.Users,
                demoSeed.Entries
            );

            dbContext.ChangeTracker.Clear();

            Log.Information(
                "DEMO {Current}/{Total} concluído.",
                i + 1,
                DemoCount
            );
        }

        await ProcessPayrollsAsync(
            requests,
            payrollGenerationService,
            dbContext
        );

        Log.Information("Seed DEMO concluído.");
    }

    private static async Task MigrateDatabaseAsync(
        ApplicationDbContext dbContext)
    {
        var retries = 1;

        while (true)
        {
            try
            {
                await dbContext.Database.MigrateAsync();

                Log.Information("Banco de dados atualizado.");

                return;
            }
            catch (Npgsql.NpgsqlException ex)
            {
                Log.Warning(
                    ex,
                    "Tentativa {Retry}/{MaxRetries} de conexão com Postgres falhou.",
                    retries,
                    MaxDatabaseRetries
                );

                if (retries >= MaxDatabaseRetries)
                {
                    throw;
                }

                retries++;

                await Task.Delay(2000);
            }
        }
    }

    private static async Task SeedRolesAsync(
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        foreach (var role in Role.All)
        {
            if (await roleManager.RoleExistsAsync(role))
            {
                continue;
            }

            var result = await roleManager.CreateAsync(
                new IdentityRole<Guid>(role)
            );

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Falha ao criar role '{role}': " +
                    string.Join(
                        ", ",
                        result.Errors.Select(error => error.Description)
                    )
                );
            }
        }
    }

    private static void AddPayrollRequests(
        Dictionary<Guid, List<ProcessPayrollRequest>> requests,
        IEnumerable<dynamic> users,
        IEnumerable<dynamic> entries)
    {
        var usersByCompany = users
            .GroupBy(user => (Guid)user.CompanyId);

        foreach (var company in usersByCompany)
        {
            var companyId = company.Key;

            var employeeIds = company
                .Select(user => (Guid)user.Id)
                .ToHashSet();

            var companyRequests = entries
                .Where(entry =>
                    employeeIds.Contains((Guid)entry.EmployeeId))
                .GroupBy(entry => new
                {
                    Year = (int)entry.Date.Year,
                    Month = (int)entry.Date.Month
                })
                .OrderBy(group => group.Key.Year)
                .ThenBy(group => group.Key.Month)
                .Select(group =>
                    new ProcessPayrollRequest(
                        group
                            .Select(entry => (Guid)entry.EmployeeId)
                            .Distinct()
                            .ToArray(),
                        new DateOnly(
                            group.Key.Year,
                            group.Key.Month,
                            1
                        ),
                        true
                    )
                )
                .ToList();

            requests[companyId] = companyRequests;
        }
    }

    private static async Task ProcessPayrollsAsync(
        Dictionary<Guid, List<ProcessPayrollRequest>> requests,
        IPayrollGenerationService payrollGenerationService,
        ApplicationDbContext dbContext)
    {
        var totalRequests = requests.Sum(
            request => request.Value.Count
        );

        var processedRequests = 0;
        var succeededRequests = 0;
        var failedRequests = 0;

        Log.Information(
            "Iniciando processamento de {PayrollCount} competências para {CompanyCount} empresas.",
            totalRequests,
            requests.Count
        );

        foreach (var (companyId, companyRequests) in requests)
        {
            Log.Information(
                "Empresa {CompanyId}: {PayrollCount} competências.",
                companyId,
                companyRequests.Count
            );

            var batches = companyRequests
                .Chunk(BatchSize)
                .ToArray();

            for (var batchIndex = 0;
                 batchIndex < batches.Length;
                 batchIndex++)
            {
                var batch = batches[batchIndex];

                Log.Information(
                    "Empresa {CompanyId} - Batch {CurrentBatch}/{TotalBatches} - {Count} competências.",
                    companyId,
                    batchIndex + 1,
                    batches.Length,
                    batch.Length
                );

                foreach (var request in batch)
                {
                    var result =
                        await payrollGenerationService.ProcessAsync(
                            companyId,
                            request
                        );

                    processedRequests++;

                    if (result.Succeeded)
                    {
                        succeededRequests++;
                    }
                    else
                    {
                        failedRequests++;
                    }

                    Log.Information(
                        "Payroll {Current}/{Total} - Empresa {CompanyId} - Competência {Competence:MM/yyyy} - Funcionários {EmployeeCount} - Sucesso {Succeeded}",
                        processedRequests,
                        totalRequests,
                        companyId,
                        request.CompetenceDate,
                        request.EmployeeIds.Length,
                        result.Succeeded
                    );

                    if (!result.Succeeded)
                    {
                        Log.Warning(
                            "Falha no payroll da empresa {CompanyId}, competência {Competence:MM/yyyy}: {Errors}",
                            companyId,
                            request.CompetenceDate,
                            string.Join(", ", result.Errors)
                        );
                    }
                }

                dbContext.ChangeTracker.Clear();
            }
        }

        Log.Information(
            "Processamento concluído. Total: {Total} | Sucesso: {Succeeded} | Falhas: {Failed}",
            processedRequests,
            succeededRequests,
            failedRequests
        );
    }
}