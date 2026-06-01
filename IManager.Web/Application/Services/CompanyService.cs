using AutoMapper;
using IManager.Web.Application.Interfaces;
using IManager.Web.Data.Repositories;
using IManager.Web.Domain.Entities.Companies;
using IManager.Web.Domain.Entities.Users;
using IManager.Web.Domain.Interfaces.Persistence;
using IManager.Web.Domain.Interfaces.Repositories;
using IManager.Web.Presentation.ViewModels.Account;
using IManager.Web.Presentation.ViewModels.Companies;
using IManager.Web.Shared;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;


namespace IManager.Web.Application.Services;

public class CompanyService : ICompanyService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICompaniesRepository _companyRepository;

    public CompanyService(IUnitOfWork unitOfWork, IMapper mapper, ICompaniesRepository companyRepository)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _companyRepository = companyRepository;
    }

    public async Task<Result> CreateAsync(CreateCompanyViewModel company)
    {
        var exists = await _companyRepository.ExistsAsync(c=>c.DocumentNumber == company.DocumentNumber);

        if (exists)
            return Result.Fail("Empresa já esta cadastrada.");

        try
        {
            var entity = _mapper.Map<Company>(company);
            await _companyRepository.AddAsync(entity);
        }
        catch (Exception ex)
        {
            return Result.Fail("Não foi possivel cadastrar empresa. Por favor, tente novamente.");
        }

        return Result.Ok();
    }

    public async Task<Result> SoftDeleteAsync(Guid id)
    {
        if (id == Guid.Empty) return Result.Fail("Empresa não encontrado.");

        var exists = await _companyRepository.ExistsAsync(c => c.Id == id);
        if(!exists) return Result.Fail("Empresa não encontrado.");

        try
        {
            await _companyRepository.SoftDeleteAsync(id);
        }
        catch (Exception)
        {
            return Result.Fail("Falha ao atualizar Empresa. Por favor, tente novamente.");
        }

        return Result.Ok();
    }

    public async Task<IEnumerable<CompanyHierarchyViewModel>> GetCompaniesHierarchyViewModelAsync()
    {
        var companies = await _companyRepository.GetAllAsync(q => q.Include(c => c.Departments)
                                                                   .ThenInclude(d => d.JobTitles));
        if (!companies.Any()) return Enumerable.Empty<CompanyHierarchyViewModel>();

        var companiesHierarchy = _mapper.Map<IEnumerable<CompanyHierarchyViewModel>>(companies);
        return companiesHierarchy;
    }

    public async Task<EditCompanyViewModel?> GetEditViewModelByIdAsync(Guid id)
    {
        var entity = await _companyRepository.GetByIdAsync(id);
        if (entity == null) return null;

        var model = _mapper.Map<EditCompanyViewModel>(entity);
        return model;
    }

    public async Task<PagedResult<IndexCompanyViewModel>> GetPagedAsync(string search, ActiveFilter active, int page, int pageSize)
    {
        Func<IQueryable<Company>, IQueryable<Company>> query = q =>
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                q = q.Where(u =>
                    EF.Functions.Like(u.LegalName.ToLower(), $"%{search}%") ||
                    EF.Functions.Like(u.DocumentNumber.ToLower(), $"%{search}%") ||
                    EF.Functions.Like(u.TradeName.ToLower(), $"%{search}%")
                );
            }

            switch (active)
            {
                case ActiveFilter.Active:
                    q = q.Where(u => u.IsActive);
                    break;
                case ActiveFilter.Inactive:
                    q = q.Where(u => !u.IsActive);
                    break;
                default:
                    break;
            }

            return q;
        };

        IEnumerable<IndexCompanyViewModel> companies = await _companyRepository.GetPagedAsync(query, page, pageSize);

        var totalCount = await _companyRepository.CountAsync(query);

        var pagedViewModel = new PagedResult<IndexCompanyViewModel>()
        {
            Items = companies,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Search = search,
            ActiveFilter = active
        };

        return pagedViewModel;
    }

    public async Task<CompanyViewModel?> GetViewModelByIdAsync(Guid id)
    {
        var entity = await _companyRepository.GetByIdAsync(id);
        if (entity == null) return null;

        var model = _mapper.Map<CompanyViewModel>(entity);
        return model;
    }

    public async Task<DetailsCompanyViewModel?> GetDetailsViewModelByIdAsync(Guid id)
    {
        var entity = await _companyRepository.GetByIdAsync(id);
        if (entity == null) return null;

        var model = _mapper.Map<DetailsCompanyViewModel>(entity);

        var infoModel = await _companyRepository.GetInfoByIdAsync(id);
        model.Info = infoModel!;

        return model;
    }

    public async Task<Result> UpdateAsync(Guid id,EditCompanyViewModel company)
    {
        try
        {
            var entity = await _companyRepository.GetByIdAsync(id);
            if (entity == null && company.Id != id) Result.Fail("Empresa não encontrada.");

            _mapper.Map(company, entity);
            await _companyRepository.UpdateAsync(entity!);
        }
        catch (Exception)
        {
            return Result.Fail("Falha ao atualizar a Empresa. Por favor, tente novamente.");
        }
        return Result.Ok();
    }

    public async Task<IEnumerable<CompanyViewModel>> GetCompaniesViewModelsAsync()
    {
        var entities = await _companyRepository.GetAllAsync();
        if (!entities.Any()) return Enumerable.Empty<CompanyViewModel>();

        var viewModel = _mapper.Map<IEnumerable<CompanyViewModel>>(entities);
        return viewModel;
    }
}