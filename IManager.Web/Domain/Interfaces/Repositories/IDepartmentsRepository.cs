using IManager.Web.Domain.Entities.Companies;
using IManager.Web.Presentation.ViewModels.Departments;

namespace IManager.Web.Domain.Interfaces.Repositories
{
    public interface IDepartmentsRepository : IRepository<Department>
    {
        Task<InfoDepartmentViewModel?> GetInfoByIdAsync(Guid id);
        Task<List<IndexDepartmentViewModel>> GetPagedAsync(Func<IQueryable<Department>, IQueryable<Department>>? query, int page, int pageSize);
    }
}