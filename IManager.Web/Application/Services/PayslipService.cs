using AutoMapper;
using IManager.Web.Application.Interfaces;
using IManager.Web.Domain.Entities.Payrolls;
using IManager.Web.Domain.Interfaces.Repositories;
using IManager.Web.Presentation.ViewModels.Payslips;

namespace IManager.Web.Application.Services;

public class PayslipService : IPayslipService
{
    private readonly IPayslipsRepository _payslipsRepository;
    private readonly IMapper _mapper;

    public PayslipService(IPayslipsRepository payslipsRepository, IMapper mapper)
    {
        _payslipsRepository = payslipsRepository;
        _mapper = mapper;
    }

    public async Task<PayslipViewModel?> GetByIdAsync(Guid userId, Guid payslipId)
    {
        return await _payslipsRepository.GetPayslipViewModelAsync(payslipId);
    }

    public async Task<PagedResult<IndexPayslipViewModel>> GetPagedByUserAsync(Guid userId, int page, int pageSize)
    {
        Func<IQueryable<Payslip>, IQueryable<Payslip>> query = q => q.Where(p => p.EmployeeId == userId);

        var totalCount = await _payslipsRepository.CountAsync(query);

        IEnumerable<IndexPayslipViewModel> indexViewModel = await _payslipsRepository.GetIndexPagedAsync(query, page, pageSize);

        return new PagedResult<IndexPayslipViewModel>()
        {
            Items = indexViewModel,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
