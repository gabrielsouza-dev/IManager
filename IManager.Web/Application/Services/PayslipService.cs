using AutoMapper;
using IManager.Web.Application.Interfaces;
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

    public async Task<IEnumerable<IndexPayslipViewModel>> GetPayslipByUserAsync(Guid userId)
    {
        var model = await _payslipsRepository.GetPayslipsByUserIdAsync(userId);
        if(!model.Any())
            return Enumerable.Empty<IndexPayslipViewModel>();

        return model;
    }
}