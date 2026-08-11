using AutoMapper;
using IManager.Web.Data.Seeder.Interfaces;
using IManager.Web.Data.Seeder.SeedDatas;
using IManager.Web.Domain.Consts;
using IManager.Web.Domain.Entities.Companies;
using IManager.Web.Domain.Entities.Users;
using IManager.Web.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IManager.Web.Data.Seeder.Factory;

public sealed class UserSeeder : IEntitySeeder<UserSeedData>
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly IRepository<UserProfile> _profiles;
    private readonly IRepository<JobTitle> _jobTitles;
    private readonly IMapper _mapper;

    public UserSeeder(
        UserManager<User> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IRepository<UserProfile> profiles,
        IRepository<JobTitle> jobTitles,
        IMapper mapper)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _profiles = profiles;
        _jobTitles = jobTitles;
        _mapper = mapper;
    }

    public async Task SeedAsync(IEnumerable<UserSeedData> data)
    {
        foreach (var item in data)
        {
            if (!await _roleManager.RoleExistsAsync(item.Role))
                throw new ArgumentException(
                    $"Falha ao criar usuario {item.Email}, role {item.Role} não existe."
                );

            var userId = item.Id;

            var profile = _mapper.Map<UserProfile>(item);
            profile.Id = userId;
            profile.IsActive = true;

            var user = new User
            {
                Id = userId,
                UserName = item.Email,
                Email = item.Email,
                EmailConfirmed = true,
                UserProfile = profile
            };

            var result = await _userManager.CreateAsync(user, item.Password);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

            await _userManager.AddToRoleAsync(user, item.Role);

            var jobTitle = await _jobTitles.GetByIdAsync(
                item.JobTitleId,
                q => q
                    .Include(j => j.Department)
                    .ThenInclude(d => d.Company)
            );

            await _userManager.AddClaimsAsync(user, new List<Claim>
            {
                new("FullName", profile.FullName),
                new("CompanyId", profile.CompanyId.ToString()),
                new("CompanyTradeName", jobTitle!.Department.Company.TradeName),
                new("DepartmentId", jobTitle.Department.Id.ToString()),
                new("Department", jobTitle.Department.Name),
                new("JobTitleId", profile.JobTitleId.ToString()),
                new("JobTitle", jobTitle.Name)
            });
        }
    }
}
