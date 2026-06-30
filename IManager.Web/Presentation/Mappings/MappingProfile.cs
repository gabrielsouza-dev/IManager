using AutoMapper;
using IManager.Web.Data.Seeder.SeedDatas;
using IManager.Web.Domain.Entities.Companies;
using IManager.Web.Domain.Entities.TimeTrackings;
using IManager.Web.Domain.Entities.Users;
using IManager.Web.Presentation.ViewModels.Account;
using IManager.Web.Presentation.ViewModels.Companies;
using IManager.Web.Presentation.ViewModels.Departments;
using IManager.Web.Presentation.ViewModels.JobTitles;
using IManager.Web.Presentation.ViewModels.Users;
using IManager.Web.Shared.DTO.TimeTrackings;

namespace IManager.Web.Presentation.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        MapSeeders();
        MapUser();
        MapCompany();
        MapDepartment();
        MapJobtitle();
        MapTimeTracking();
    }

    private void MapSeeders()
    {
        CreateMap<UserSeedData, UserProfile>().ReverseMap();
        CreateMap<CompanySeedData, Company>().ReverseMap();
        CreateMap<DepartmentSeedData, Department>().ReverseMap();
        CreateMap<JobTitleSeedData, JobTitle>().ReverseMap();
    }

    private void MapUser()
    {
        CreateMap<User, AccountDetailsViewModel>().ReverseMap();
        CreateMap<UserProfile, AccountViewModel>().ReverseMap();
        CreateMap<UserProfile, IndexUserViewModel>().ReverseMap();
        CreateMap<User, DetailsUserViewModel>().ReverseMap();
        CreateMap<UserProfile, DetailsUserViewModel>().ReverseMap();
        CreateMap<UserProfile, EditAccountViewModel>()
            .ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src => src.JobTitle.Department.Company.Id))
            .ForMember(dest => dest.DepartmentId, opt => opt.MapFrom(src => src.JobTitle.Department.Id))
            .ForMember(dest => dest.JobTitleId, opt => opt.MapFrom(src => src.JobTitle.Id));
        CreateMap<UserProfile, AccountDetailsViewModel>()
            .ForMember(dest => dest.JobTitle, opt => opt.MapFrom(src => src.JobTitle.Name))
            .ReverseMap();
        CreateMap<UserProfile, RegisterViewModel>().ReverseMap();
    }

    private void MapCompany()
    {
        CreateMap<Company, CompanyViewModel>().ReverseMap();
        CreateMap<Company, IndexCompanyViewModel>().ReverseMap();
        CreateMap<Company, DetailsCompanyViewModel>().ReverseMap();
        CreateMap<Company, CreateCompanyViewModel>().ReverseMap();
        CreateMap<Company, EditCompanyViewModel>().ReverseMap();
        CreateMap<Company, CompanyHierarchyViewModel>().ReverseMap();
    }

    private void MapDepartment()
    {
        CreateMap<Department, DepartmentViewModel>().ReverseMap();
        CreateMap<Department, DetailsDepartmentViewModel>().ReverseMap();
        CreateMap<Department, CreateDepartmentViewModel>().ReverseMap();
        CreateMap<Department, EditDepartmentViewModel>().ReverseMap();
        CreateMap<Department, DepartmentHierarchyViewModel>().ReverseMap();
    }

    private void MapJobtitle()
    {
        CreateMap<JobTitle, JobTitleModelView>().ReverseMap();
        CreateMap<JobTitle, EditJobTitleModelView>().ReverseMap();
        CreateMap<JobTitle, IndexJobTitleModelView>()
            .ForMember(dest => dest.CompanyTradeName, opt => opt.MapFrom(src => src.Department.Company.TradeName))
            .ForMember(dest => dest.CompanyDocumentNumber, opt => opt.MapFrom(src => src.Department.Company.DocumentNumber))
            .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department.Name))
            .ReverseMap();
        CreateMap<JobTitle, CreateJobTitleModelView>()
            .ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src => src.Department.Company.Id))
            .ForMember(dest => dest.DepartmentId, opt => opt.MapFrom(src => src.Department.Id));
        CreateMap<CreateJobTitleModelView, JobTitle>()
            .ForMember(d => d.Department, opt => opt.Ignore());
        CreateMap<JobTitle, DetailsJobTitleModelView>()
            .ForMember(dest => dest.CompanyTradeName, opt => opt.MapFrom(src => src.Department.Company.TradeName))
            .ForMember(dest => dest.CompanyDocumentNumber, opt => opt.MapFrom(src => src.Department.Company.DocumentNumber))
            .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department.Name))
            .ReverseMap();
        CreateMap<JobTitle, JobTitleHierarchyModelView>().ReverseMap();
    }

    private void MapTimeTracking()
    {
        CreateMap<TimeEntry, TimeEntryDTO>().ReverseMap();
        CreateMap<TimeCheck, TimeCheckDTO>()
            .ReverseMap();
    }
}