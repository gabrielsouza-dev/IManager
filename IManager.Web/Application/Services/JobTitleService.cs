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

    public JobTitleService(IUnitOfWork unitOfWork, IMapper mapper, IJobTitlesRepository jobTitleRepository)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _jobTitleRepository = jobTitleRepository;
    }

    public async Task<Result> CreateAsync(CreateJobTitleModelView model)
    {
        var entity = _mapper.Map<JobTitle>(model);

        try
        {
            await _jobTitleRepository.AddAsync(entity);
        }
        catch (Exception ex)
        {
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
        catch (Exception)
        {
            return Result.Fail("Falha ao atualizar Cargo. Por favor, tente novamente.");
        }

        return Result.Ok();
    }

    public async Task<DetailsJobTitleModelView?> GetDetailsModelViewById(Guid id)
    {
        var entity = await _jobTitleRepository.GetByIdAsync(id, q => q.Include(j => j.Department).ThenInclude(d=>d.Company));
        if (entity == null) return null;

        var model = _mapper.Map<DetailsJobTitleModelView>(entity);
        var info = await _jobTitleRepository.GetInfoByIdAsync(id);
        model.Info = info;

        return model;
    }

    public async Task<EditJobTitleModelView?> GetEditModelViewByIdAsync(Guid id)
    {
        var entity = await _jobTitleRepository.GetByIdAsync(id);
        if (entity == null) return null;

        return _mapper.Map<EditJobTitleModelView?>(entity);
    }

    public async Task<JobTitleModelView?> GetModelViewByIdAsync(Guid id)
    {
        var entity = await _jobTitleRepository.GetByIdAsync(id);

        return _mapper.Map<JobTitleModelView?>(entity);
    }

    public async Task<PagedResult<IndexJobTitleModelView>> GetPagedAsync(string search, ActiveFilter active, int page, int pageSize, Guid? companyId = null)
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

        IEnumerable<IndexJobTitleModelView> indexModelView = await _jobTitleRepository.GetPagedAsync(query, page, pageSize);

        var pagedViewModel = new PagedResult<IndexJobTitleModelView>()
        {
            Items = indexModelView,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Search = search,
            ActiveFilter = active
        };

        return pagedViewModel;
    }

    public async Task<Result> UpdateAsync(EditJobTitleModelView model)
    {
        var entity = await _jobTitleRepository.GetByIdAsync(model.Id);
        if (entity == null) throw new ArgumentException("Erro ao atualizar dados de cargo.");

        _mapper.Map(model, entity);

        try
        {
            await _jobTitleRepository.UpdateAsync(entity);
        }
        catch (Exception)
        {
            return Result.Fail("Falha ao atualizar Cargo. Por favor, tente novamente.");
        }

        return Result.Ok();
    }
}