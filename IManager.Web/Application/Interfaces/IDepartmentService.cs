using IManager.Web.Presentation.ViewModels.Departments;
using IManager.Web.Shared;

namespace IManager.Web.Application.Interfaces;

public interface IDepartmentService
{
    Task<DepartmentViewModel?> GetViewModelByIdAsync(Guid id);
    Task<DetailsDepartmentViewModel?> GetDetailsViewModelByIdAsync(Guid id);
    Task<IEnumerable<DepartmentHierarchyViewModel>> GetDepartmentsHierarchyViewModelAsync(Guid? companyId = null);
    Task<PagedResult<IndexDepartmentViewModel>> GetPagedAsync(string search, ActiveFilter active, int page, int pageSize, Guid? companyId = null);
    Task<Result> CreateAsync(CreateDepartmentViewModel department);
    Task<EditDepartmentViewModel?> GetEditViewModelByIdAsync(Guid id);
    Task<Result> UpdateAsync(EditDepartmentViewModel department);
    Task<Result> SoftDeleteAsync(Guid id);
}