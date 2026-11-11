using IManager.Web.Application.Interfaces;
using IManager.Web.Domain.Consts;
using IManager.Web.Domain.Interfaces.Repositories;
using IManager.Web.Presentation.Configurations;
using IManager.Web.Presentation.ViewModels.DemoLogins;
using Microsoft.Extensions.Options;

namespace IManager.Web.Application.Services;

/// <summary>
/// Lista os logins criados pelo seed de demonstração (marcados com a claim DemoSeed).
/// As senhas não vêm do banco: são as senhas fixas usadas pelo seed.
/// </summary>
public class DemoLoginService : IDemoLoginService
{
    private readonly IUserProfilesRepository _userProfilesRepository;
    private readonly DemoProfilesOptions _demoOptions;

    public DemoLoginService(IUserProfilesRepository userProfilesRepository, IOptions<DemoProfilesOptions> demoOptions)
    {
        _userProfilesRepository = userProfilesRepository;
        _demoOptions = demoOptions.Value;
    }

    public async Task<DemoLoginsViewModel> GetDemoLoginsAsync()
    {
        var logins = (await _userProfilesRepository.GetDemoLoginsAsync()).ToList();

        foreach (var login in logins)
        {
            login.Password = login.Role switch
            {
                Role.Staff => _demoOptions.Users.Staff.Password,
                Role.Admin => DemoSeed.AdminPassword,
                _ => DemoSeed.UserPassword
            };
        }

        var companies = logins
            .Where(l => l.Role != Role.Staff)
            .GroupBy(l => new { l.CompanyId, l.CompanyTradeName })
            .OrderBy(g => g.Key.CompanyTradeName)
            .Select(g => new DemoCompanyLoginsViewModel
            {
                CompanyTradeName = g.Key.CompanyTradeName,
                Admin = g.FirstOrDefault(l => l.Role == Role.Admin),
                Users = g.Where(l => l.Role != Role.Admin)
                         .OrderBy(l => l.FullName)
                         .ToList()
            })
            .ToList();

        return new DemoLoginsViewModel
        {
            Staff = logins.Where(l => l.Role == Role.Staff).OrderBy(l => l.FullName).ToList(),
            Companies = companies
        };
    }
}
