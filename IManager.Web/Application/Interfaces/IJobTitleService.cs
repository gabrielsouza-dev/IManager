using IManager.Web.Presentation.ViewModels.Departments;
using IManager.Web.Presentation.ViewModels.JobTitles;
using IManager.Web.Shared;

namespace IManager.Web.Application.Interfaces;

public interface IJobTitleService
{
    Task<Result> CreateAsync(CreateJobTitleViewModel model);
    Task<Result> SoftDeleteAsync(Guid id);
    Task<DetailsJobTitleViewModel?> GetDetailsModelViewById(Guid id);
    Task<EditJobTitleViewModel?> GetEditModelViewByIdAsync(Guid id);
    Task<JobTitleViewModel?> GetModelViewByIdAsync(Guid id);
    Task<PagedResult<IndexJobTitleViewModel>> GetPagedAsync(string search, ActiveFilter active, int page, int pageSize, Guid? companyId = null);
    Task<Result> UpdateAsync(EditJobTitleViewModel model);
}