using IManager.Web.Domain.Entities.Users;
using IManager.Web.Presentation.ViewModels.Users;

namespace IManager.Web.Domain.Interfaces.Repositories;

public interface IUserProfilesRepository : IRepository<UserProfile>
{
    Task<InfoUserProfileViewModel?> GetInfoByIdAsync(Guid id);
}
