using IManager.Web.Data.Repositories;
using IManager.Web.Domain.Entities.Companies;
using IManager.Web.Domain.Entities.Users;
using IManager.Web.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IManager.Web.Application.Services;

public class JwtTokenService
{
    private readonly IConfiguration _config;
    private readonly UserManager<User> _userManager;
    private readonly IRepository<UserProfile> _userProfileRepository;
    private readonly IRepository<JobTitle> _jobTitleRepository;

    public JwtTokenService(IConfiguration config, UserManager<User> userManager, 
        IRepository<UserProfile> userProfileRepository, IRepository<JobTitle> jobTitleRepository)
    {
        _config = config;
        _userManager = userManager;
        _userProfileRepository = userProfileRepository;
        _jobTitleRepository = jobTitleRepository;
    }

    public async Task<string> GenerateTokenAsync(User user)
    {
        var userProfile = await _userProfileRepository.GetByIdAsync(user.Id) ?? throw new InvalidOperationException("UserProfile não localizado.");

        var jobTitle = await _jobTitleRepository.GetByIdAsync(userProfile.JobTitleId, q =>
                    q.Include(c => c.Department).ThenInclude(d => d.Company))
                    ?? throw new InvalidOperationException("JobTitle não localizado.");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, user.UserName!),
            new("FullName", userProfile.FullName ?? "Desconhecido"),
            new("CompanyId", jobTitle.Department.Company.Id.ToString()),
            new("CompanyTradeName", jobTitle.Department.Company.TradeName),
            new("DepartmentId", jobTitle.Department.Id.ToString()),
            new("Department", jobTitle.Department.Name),
            new("JobTitleId", jobTitle.Id.ToString()),
            new("JobTitle", jobTitle.Name)
        };

        var roles = await _userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpiresInMinutes"]!)),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}