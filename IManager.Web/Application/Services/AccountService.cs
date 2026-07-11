using AutoMapper;
using IManager.Web.Application.Interfaces;
using IManager.Web.Domain.Entities.Users;
using IManager.Web.Domain.Interfaces.Persistence;
using IManager.Web.Domain.Interfaces.Repositories;
using IManager.Web.Presentation.ViewModels.Account;
using IManager.Web.Presentation.ViewModels.Users;
using IManager.Web.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UserProfile = IManager.Web.Domain.Entities.Users.UserProfile;


namespace IManager.Web.Application.Services;

public class AccountService : IAccountService
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailSender<User> _emailSender;
    private readonly IMapper _mapper;
    private readonly IUserProfilesRepository _userProfileRepository;
    private readonly IJobTitlesRepository _jobTitleRepository;

    public AccountService(SignInManager<User> signInManager, UserManager<User> userManager, 
        IUnitOfWork unitOfWork, IEmailSender<User> emailSender, IMapper mapper, 
        IUserProfilesRepository userProfileRepository, IJobTitlesRepository jobTitleRepository)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _emailSender = emailSender;
        _mapper = mapper;
        _userProfileRepository = userProfileRepository;
        _jobTitleRepository = jobTitleRepository;
    }

    #region Registro e Confirmação de E-mail

    public async Task<Result> RegisterAsync(RegisterViewModel model)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var user = new User { UserName = model.Email, Email = model.Email, PhoneNumber = model.PhoneNumber };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                await _unitOfWork.RollbackAsync();
                return Result.Fail(result.Errors.Select(r => r.Description));
            }
            var userProfile = _mapper.Map<UserProfile>(model);
            userProfile.Id = user.Id;

            await _userProfileRepository.AddAsync(userProfile);

            var jobTitle = await _jobTitleRepository.GetByIdAsync(userProfile.JobTitleId, q => 
                                q.Include(c => c.Department).ThenInclude(d => d.Company))
                                ?? throw new InvalidOperationException("JobTitle não localizado.");

            await _userManager.AddClaimsAsync(user, new List<Claim>
            {
                new("FullName", userProfile?.FullName ?? "Desconhecido"),
                new("CompanyId", jobTitle.Department.Company.Id.ToString() ?? "Null"),
                new("CompanyTradeName", jobTitle.Department.Company.TradeName.ToString() ?? "Null"),
                new("DepartmentId", jobTitle.Department.Id.ToString() ?? "Null"),
                new("Department", jobTitle.Department.Name.ToString() ?? "Null"),
                new("JobTitleId", jobTitle.Id.ToString() ?? "Null"),
                new("JobTitle", jobTitle.Name.ToString() ?? "Null")
            });

            await _userManager.AddToRoleAsync(user, model.Role);

            await _unitOfWork.CommitAsync();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            await _unitOfWork.RollbackAsync();
            return Result.Fail("Erro ao criar conta. Tente novamente.");
        }
    }

    public async Task<string> GenerateConfirmationTokenAsync(User user)
        => await _userManager.GenerateEmailConfirmationTokenAsync(user);

    public async Task SendConfirmationEmailAsync(User user, string link)
        => await _emailSender.SendConfirmationLinkAsync(user, user.Email!, link);

    public async Task<Result> ConfirmEmailAsync(Guid userId, string token)
    {
        var user = await GetByIdAsync(userId.ToString());
        if (user == null)
            return Result.Fail("Usuário não encontrado.");

        var result = await _userManager.ConfirmEmailAsync(user, token);

        if (!result.Succeeded)
            return Result.Fail(result.Errors.Select(r => r.Description));

        return Result.Ok();
    }

    #endregion

    #region Login e Logout

    public async Task<Result> LoginAsync(string email, string password)
    {
        var result = await _signInManager.PasswordSignInAsync(email, password, true, true);

        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
                return Result.Fail("Conta bloqueada. Tente novamente mais tarde.");
            if (result.IsNotAllowed)
                return Result.Fail("Confirme seu e-mail antes de fazer login.");
            return Result.Fail("E-mail ou senha inválidos.");
        }

        return Result.Ok();
    }

    public async Task LogoutAsync()
        => await _signInManager.SignOutAsync();

    #endregion

    #region Recuperação de Senha

    public async Task<string> GenerateResetPasswordTokenAsync(User user)
        => await _userManager.GeneratePasswordResetTokenAsync(user);

    public async Task SendResetPasswordEmailAsync(User user, string link)
        => await _emailSender.SendPasswordResetLinkAsync(user, user.Email!, link);

    public async Task<Result> ConfirmResetPasswordTokenAsync(ResetPasswordViewModel model)
    {
        var user = await GetByEmailAsync(model.Email);
        if (user == null)
            return Result.Fail("Usuário não encontrado.");

        var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);

        if (!result.Succeeded)
            return Result.Fail(result.Errors.Select(r => r.Description));

        return Result.Ok();
    }

    #endregion

    #region Perfil
    public async Task<AccountDetailsViewModel?> GetAccountDetailsViewModelByEmailAsync(string email)
    {
        var user = await GetByEmailAsync(email) ?? throw new ArgumentException("Usuário não encontrado!");

        var userProfile = await _userProfileRepository.GetByIdAsync(user.Id)
            ?? throw new ArgumentException("Usuário não encontrado!");

        var viewModel = _mapper.Map<AccountDetailsViewModel>(userProfile);
        _mapper.Map(user, viewModel);
        return viewModel;
    }

    public async Task<Result> EditDetailsAsync(string email, AccountDetailsViewModel model)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var user = await GetByEmailAsync(email);
            if (user == null)
                return Result.Fail("Usuário não encontrado.");

            user.PhoneNumber = model.PhoneNumber;
            await _userManager.UpdateAsync(user);

            var userProfile = await _userProfileRepository.GetByIdAsync(user.Id);
            userProfile!.FullName = model.FullName;
            userProfile.BirthDate = model.BirthDate;
            await _userProfileRepository.UpdateAsync(userProfile);

            var oldClaim = (await _userManager.GetClaimsAsync(user))
                .FirstOrDefault(c => c.Type == "FullName");

            if (oldClaim != null)
                await _userManager.ReplaceClaimAsync(user, oldClaim, new Claim("FullName", model.FullName));
            else
                await _userManager.AddClaimAsync(user, new Claim("FullName", model.FullName));

            await _unitOfWork.CommitAsync();
            return Result.Ok();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            return Result.Fail("Erro ao atualizar perfil. Tente novamente.");
        }
    }
    #endregion

    #region Editar User
    public async Task<EditAccountViewModel?> GetEditAccountViewModelByIdAsync(Guid id)
    {
        var userProfile = await _userProfileRepository.GetByIdAsync(id, q => q
                                          .Include(u => u.JobTitle)
                                          .ThenInclude(j => j.Department)
                                          .ThenInclude(d => d.Company));
        if (userProfile == null) return null;

        var editAccountCiewModel = _mapper.Map<EditAccountViewModel>(userProfile);

        var user = await GetByIdAsync(id.ToString());
        if (user == null) return null;

        editAccountCiewModel.Email = user!.Email!;
        editAccountCiewModel.PhoneNumber = user!.PhoneNumber!;

        return editAccountCiewModel;
    }

    public async Task<Result> EditAccountAsync(Guid id, EditAccountViewModel model)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var user = await GetByIdAsync(id.ToString());
            if(user == null) throw new ArgumentException("Usuário não encontrado.");

            var userProfile = await _userProfileRepository.GetByIdAsync(user.Id);
            if (user is null || userProfile is null) throw new ArgumentException("Usuário não encontrado.");

            if (user.PhoneNumber != model.PhoneNumber)
            {
                var result = await UpdatePhoneNumberAsync(user, model);

                if (!result.Succeeded)
                    throw new Exception("Falha ao atualizar telefone de usuário.");
            }

            await UpdateUserProfileInfos(userProfile, model);

            if (model.Email != user.Email)
            {
                var result = await UpdateEmailAsync(user, model);

                if (!result.Succeeded)
                    throw new Exception("Falha ao atualizar e-mail.");
            }

            var updateClaimsResult = await UpdateUserClaims(user, userProfile, model);
            if (!updateClaimsResult.Succeeded)
                return updateClaimsResult;

            await _unitOfWork.CommitAsync();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return Result.Fail(ex.Message);
        }
    }
    #endregion

    #region Deletar User
    public async Task<Result> SoftDeleteAsync(Guid id)
    {
        var user = await GetByIdAsync(id.ToString());
        var userProfile = await _userProfileRepository.GetByIdAsync(id);

        if (user is null || userProfile is null)
            return Result.Fail("Usuario não encontrado.");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var isLocked = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow;

            if (!isLocked)
            {
                await _userProfileRepository.SoftDeleteAsync(userProfile.Id);

                await _userManager.SetLockoutEnabledAsync(user, true);
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            }
            else
            {
                await _userProfileRepository.SoftDeleteAsync(userProfile.Id);

                await _userManager.SetLockoutEndDateAsync(user, null);
            }

            await _unitOfWork.CommitAsync();
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync();
            return Result.Fail("Falha ao atualizar Usuario. Por favor tente novamente.");
        }
        return Result.Ok();
    }
    #endregion

    #region Troca de E-mail

    public Task<string> GenerateChangeEmailTokenAsync(User user, string newEmail)
        => _userManager.GenerateChangeEmailTokenAsync(user, newEmail);

    public async Task SendChangeEmailLinkAsync(User user, string newEmail, string link)
        => await _emailSender.SendConfirmationLinkAsync(user, newEmail, link);

    public async Task<Result> ConfirmChangeEmailAsync(string currentEmail, string newEmail, string token)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.NormalizedEmail == currentEmail.ToUpperInvariant());
        if (user == null)
            return Result.Fail("Usuário não encontrado.");

        var result = await _userManager.ChangeEmailAsync(user, newEmail, token);
        if (!result.Succeeded)
            return Result.Fail(result.Errors.Select(e => e.Description));

        await _userManager.SetUserNameAsync(user, newEmail);
        return Result.Ok();
    }
    #endregion

    #region Troca de Senha

    public async Task<Result> ChangePasswordAsync(string email, ChangePasswordViewModel model)
    {
        var user = await GetByEmailAsync(email);
        if (user == null)
            return Result.Fail("Usuário não encontrado.");

        var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

        if (!result.Succeeded)
            return Result.Fail(result.Errors.Select(r => r.Description));

        return Result.Ok();
    }
    #endregion

    #region Utils
    public async Task<User?> GetByEmailAsync(string email)
    => await _userManager.Users
    .FirstOrDefaultAsync(u => u.NormalizedEmail == email.ToUpperInvariant());

    public async Task<User?> GetByIdAsync(string userId)
    => await _userManager.FindByIdAsync(userId);

    public async Task<AccountDetailsViewModel?> GetDetailsViewModelByIdAsync(Guid id)
    {
        var user = await GetByIdAsync(id.ToString());
        if (user == null) return null;

        var userProfile = await _userProfileRepository.GetByIdAsync(user.Id, q => q.Include(u => u.JobTitle));
        if (userProfile == null) return null;

        return _mapper.Map<AccountDetailsViewModel>(userProfile);
    }
    #endregion

    #region Helpers
    private async Task UpdateClaimAsync(User user, string type, string value)
    {
        var claims = await _userManager.GetClaimsAsync(user);
        var existing = claims.FirstOrDefault(c => c.Type == type);

        if (existing != null)
        {
            if (existing.Value != value)
                await _userManager.ReplaceClaimAsync(user, existing, new Claim(type, value));
        }
        else
        {
            await _userManager.AddClaimAsync(user, new Claim(type, value));
        }
    }

    private async Task<IdentityResult> UpdatePhoneNumberAsync(User user, EditAccountViewModel model)
    {
        user.PhoneNumber = model.PhoneNumber;
        return await _userManager.UpdateAsync(user);
    }

    private async Task<IdentityResult> UpdateEmailAsync(User user, EditAccountViewModel model)
    {
        user.Email = model.Email;
        user.NormalizedEmail = model.Email.ToUpperInvariant();
        user.UserName = model.Email;
        user.NormalizedUserName = model.Email.ToUpperInvariant();
        user.EmailConfirmed = true;

        return await _userManager.UpdateAsync(user);
    }

    private async Task UpdateUserProfileInfos(UserProfile userProfile, EditAccountViewModel model)
    {
        userProfile.FullName = model.FullName;
        userProfile.DocumentNumber = model.DocumentNumber;
        userProfile.BirthDate = model.BirthDate;
        userProfile.JobTitleId = model.JobTitleId;
        userProfile.BaseSalary = model.BaseSalary;

        await _userProfileRepository.UpdateAsync(userProfile);
    }

    private async Task<Result> UpdateUserClaims(User user, UserProfile userProfile, EditAccountViewModel model)
    {
        var jobTitle = await _jobTitleRepository.GetByIdAsync(userProfile.JobTitleId, q => q
                                          .Include(j => j.Department)
                                          .ThenInclude(d => d.Company));

        if (jobTitle is null)
            return Result.Fail("Cargo não localizado.");

        await UpdateClaimAsync(user, "FullName", userProfile.FullName);
        await UpdateClaimAsync(user, "CompanyId", jobTitle.Department.Company.Id.ToString());
        await UpdateClaimAsync(user, "CompanyTradeName", jobTitle.Department.Company.TradeName);
        await UpdateClaimAsync(user, "DepartmentId", jobTitle.Department.Id.ToString());
        await UpdateClaimAsync(user, "Department", jobTitle.Department.Name);
        await UpdateClaimAsync(user, "JobTitleId", jobTitle.Id.ToString());
        await UpdateClaimAsync(user, "JobTitle", jobTitle.Name);

        var currentRoles = await _userManager.GetRolesAsync(user);
        var currentRole = currentRoles.FirstOrDefault();

        var newRole = model.Role.ToString();

        if (!currentRoles.Contains(newRole))
        {
            if (!string.IsNullOrEmpty(currentRole) && currentRole != newRole)
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                    return Result.Fail("Erro ao remover perfil anterior.");
            }

            var addResult = await _userManager.AddToRoleAsync(user, newRole);
            if (!addResult.Succeeded)
                return Result.Fail("Erro ao atribuir novo perfil ao usuário.");

            userProfile.Role = newRole;
            await _userProfileRepository.UpdateAsync(userProfile);
        }
        return Result.Ok();
    }
    #endregion
}