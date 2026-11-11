using AutoMapper;
using IManager.Web.Application.Interfaces;
using IManager.Web.Data.Repositories;
using IManager.Web.Domain.Entities.Companies;
using IManager.Web.Domain.Interfaces.Persistence;
using IManager.Web.Domain.Interfaces.Repositories;
using IManager.Web.Presentation.ViewModels.JobTitles;
using IManager.Web.Shared;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace IManager.Web.Application.Services;

public class JobTitleService : IJobTitleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IJobTitlesRepository _jobTitleRepository;
    private readonly ILogger<JobTitleService> _logger;

    public JobTitleService(IUnitOfWork unitOfWork, IMapper mapper, IJobTitlesRepository jobTitleRepository, ILogger<JobTitleService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _jobTitleRepository = jobTitleRepository;
        _logger = logger;
    }

    public async Task<Result> CreateAsync(CreateJobTitleViewModel model)
    {
        var entity = _mapper.Map<JobTitle>(model);

        try
        {
            await _jobTitleRepository.AddAsync(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao cadastrar Cargo.");

            return Result.Fail("Não foi possivel cadastrar Cargo. Por favor, tente novamente.");
        }

        return Result.Ok();
    }

    public async Task<Result> SoftDeleteAsync(Guid id)
    {
        if (id == Guid.Empty) return Result.Fail("Cargo não encontrado.");

        var exists = await _jobTitleRepository.ExistsAsync(c => c.Id == id);
        if (!exists) return Result.Fail("Cargo não encontrado.");

        try
        {
            await _jobTitleRepository.SoftDeleteAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir cargo {Id}", id);

            return Result.Fail("Falha ao atualizar Cargo. Por favor, tente novamente.");
        }

        return Result.Ok();
    }

    public async Task<DetailsJobTitleViewModel?> GetDetailsViewModelByIdAsync(Guid id)
    {
        var entity = await _jobTitleRepository.GetByIdAsync(id, q => q.Include(j => j.Department).ThenInclude(d=>d.Company));
        if (entity == null) return null;

        var model = _mapper.Map<DetailsJobTitleViewModel>(entity);
        var info = await _jobTitleRepository.GetInfoByIdAsync(id);
        if(info == null) return null;
        model.Info = info;

        return model;
    }

    public async Task<EditJobTitleViewModel?> GetEditViewModelByIdAsync(Guid id)
    {
        var entity = await _jobTitleRepository.GetByIdAsync(id);
        if (entity == null) return null;

        return _mapper.Map<EditJobTitleViewModel?>(entity);
    }

    public async Task<JobTitleViewModel?> GetViewModelByIdAsync(Guid id)
    {
        var entity = await _jobTitleRepository.GetByIdAsync(id);

        return _mapper.Map<JobTitleViewModel?>(entity);
    }

    public async Task<PagedResult<IndexJobTitleViewModel>> GetPagedAsync(string search, ActiveFilter active, int page, int pageSize, Guid? companyId = null)
    {
        Func<IQueryable<JobTitle>, IQueryable<JobTitle>> query = q =>
        {
            q = q.Include(j => j.Department)
                 .ThenInclude(d=> d.Company);

            if (companyId.HasValue)
            {
                q = q.Where(j => j.Department.Company.Id == companyId);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                q = q.Where(d =>
                    EF.Functions.Like(d.Name.ToLower(), $"%{search}%") ||
                    EF.Functions.Like(d.Department.Name.ToLower(), $"%{search}%") ||
                    EF.Functions.Like(d.Department.Company.DocumentNumber.ToLower(), $"%{search}%") ||
                    EF.Functions.Like(d.Department.Company.TradeName.ToLower(), $"%{search}%") ||
                    EF.Functions.Like(d.Department.Company.LegalName.ToLower(), $"%{search}%")
                );
            }

            switch (active)
            {
                case ActiveFilter.Active:
                    q = q.Where(j => j.IsActive);
                    break;
                case ActiveFilter.Inactive:
                    q = q.Where(j => !j.IsActive);
                    break;
                default:
                    break;
            }

            return q;
        };

        var totalCount = await _jobTitleRepository.CountAsync(query);

        IEnumerable<IndexJobTitleViewModel> indexViewModel = await _jobTitleRepository.GetPagedAsync(query, page, pageSize);

        var pagedViewModel = new PagedResult<IndexJobTitleViewModel>()
        {
            Items = indexViewModel,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Search = search,
            ActiveFilter = active
        };

        return pagedViewModel;
    }

    public async Task<Result> UpdateAsync(EditJobTitleViewModel model)
    {
        var entity = await _jobTitleRepository.GetByIdAsync(model.Id);
        if (entity == null) throw new ArgumentException("Erro ao atualizar dados de cargo.");

        _mapper.Map(model, entity);

        try
        {
            await _jobTitleRepository.UpdateAsync(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar cargo {Id}", entity.Id);

            return Result.Fail("Falha ao atualizar Cargo. Por favor, tente novamente.");
        }

        return Result.Ok();
    }
}