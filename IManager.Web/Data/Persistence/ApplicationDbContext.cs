using AutoMapper;
using IManager.Web.Data.Seeder;
using IManager.Web.Domain.Consts;
using IManager.Web.Domain.Entities;
using IManager.Web.Domain.Entities.Companies;
using IManager.Web.Domain.Entities.Payrolls;
using IManager.Web.Domain.Entities.TimeTrackings;
using IManager.Web.Domain.Entities.Users;
using IManager.Web.Presentation.Configurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IManager.Web.Data.Persistence;

public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    protected ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<JobTitle> JobTitles { get; set; }
    public DbSet<TimeEntry> TimeEntries { get; set; }
    public DbSet<TimeCheck> TimeChecks { get; set; }
    public DbSet<Payroll> Payrolls { get; set; }
    public DbSet<Payslip> Payslips { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var postgresSettings = configuration.GetSection("Postgres").Get<PostgresSettings>()
                ?? throw new Exception("Postgres config not found");

            optionsBuilder.UseNpgsql(postgresSettings.ConnectionString);
        }
    }

    #region OnModelCreating
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
            .Where(e => typeof(BaseEntity).IsAssignableFrom(e.ClrType)))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .HasOne(typeof(User), nameof(BaseEntity.LastModifier))
                        .WithMany()
                        .HasForeignKey(nameof(BaseEntity.LastModifierId))
                        .IsRequired(false)
                        .OnDelete(DeleteBehavior.Restrict);
                }

        // Chave primária explícita para UserProfile
        modelBuilder.Entity<UserProfile>()
            .HasKey(u => u.Id);

        // 1:1 User -> UserProfile
        modelBuilder.Entity<User>()
            .HasOne(u => u.UserProfile)
            .WithOne()
            .HasForeignKey<UserProfile>(p => p.Id)
            .IsRequired();


        modelBuilder.Entity<User>()
            .HasIndex(u => u.NormalizedEmail)
            .IsUnique();

        // Company
        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DocumentNumber).IsRequired().HasMaxLength(18);
            entity.Property(e => e.LegalName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.TradeName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.FoundedAt).IsRequired();

            entity.HasMany(e => e.Employees)
                  .WithOne(e => e.Company)
                  .HasForeignKey(e => e.CompanyId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Departments)
                  .WithOne(e => e.Company)
                  .HasForeignKey(e => e.CompanyId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Payrolls)
                  .WithOne(e => e.Company)
                  .HasForeignKey(e => e.CompanyId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.DocumentNumber)
                  .IsUnique();
        });

        // Department
        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);

            entity.HasMany(e => e.JobTitles)
                  .WithOne(e => e.Department)
                  .HasForeignKey(e => e.DepartmentId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // JobTitle
        modelBuilder.Entity<JobTitle>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.DailyHours).IsRequired();

            entity.HasMany(e => e.Employees)
                  .WithOne(e => e.JobTitle)
                  .HasForeignKey(e => e.JobTitleId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // UserProfile
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.DocumentNumber).IsRequired().HasMaxLength(14);
            entity.Property(e => e.BirthDate).IsRequired();
            entity.Property(e => e.BaseSalary).HasColumnType("decimal(18,2)");

            entity.HasIndex(e => e.DocumentNumber)
                  .IsUnique();

            entity.HasMany(e => e.TimeEntries)
                  .WithOne(e => e.Employee)
                  .HasForeignKey(e => e.EmployeeId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Payslips)
                  .WithOne(e => e.Employee)
                  .HasForeignKey(e => e.EmployeeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // TimeEntry
        modelBuilder.Entity<TimeEntry>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasMany(e => e.Checks)
                  .WithOne(e => e.TimeEntry)
                  .HasForeignKey(e => e.TimeEntryId)
                  .OnDelete(DeleteBehavior.Cascade); // checks dependem totalmente do TimeEntry

            entity.HasIndex(e => new { e.EmployeeId, e.Date })
                  .HasFilter("\"IsCurrent\" = true")
                  .IsUnique();

            entity.HasOne(e => e.Parent)
                  .WithMany()
                  .HasForeignKey(e => e.ParentId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.Ignore(te => te.WorkedHours);

            entity.Ignore(te => te.IsConsistent);
        });

        // TimeCheck
        modelBuilder.Entity<TimeCheck>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Timestamp).IsRequired();
        });

        // Payroll
        modelBuilder.Entity<Payroll>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Company)
                  .WithMany()
                  .HasForeignKey(e => e.CompanyId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.PeriodStart).IsRequired();
            entity.Property(e => e.PeriodEnd).IsRequired();
            entity.Property(e => e.Competence).IsRequired();

            entity.HasMany(e => e.Payslips)
                  .WithOne(e => e.Payroll)
                  .HasForeignKey(e => e.PayrollId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.CompanyId, e.Competence })
                  .IsUnique();
        });

        // Payslip
        modelBuilder.Entity<Payslip>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.GrossSalary).HasColumnType("decimal(18,2)");
            entity.Property(e => e.OvertimeAdditionals).HasColumnType("decimal(18,2)");
            entity.Property(e => e.HazardAdditionals).HasColumnType("decimal(18,2)");
            entity.Property(e => e.UnhealthyAdditionals).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Commission).HasColumnType("decimal(18,2)");
            entity.Property(e => e.INSSDeduction).HasColumnType("decimal(18,2)");
            entity.Property(e => e.IRRFDeduction).HasColumnType("decimal(18,2)");
            entity.Property(e => e.OtherDeductions).HasColumnType("decimal(18,2)");

            entity.HasMany(e => e.TimeEntries)
                  .WithOne(t => t.Payslip)
                  .HasForeignKey(t => t.PayslipId)
                  .OnDelete(DeleteBehavior.SetNull);

            // propriedades calculadas são ignoradas pelo EF
            //entity.Ignore(e => e.TotalEarnings);
            //entity.Ignore(e => e.TotalDeductions);
            //entity.Ignore(e => e.NetSalary);
        });
    }
    #endregion
}