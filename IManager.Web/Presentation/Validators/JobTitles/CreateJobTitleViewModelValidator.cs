using FluentValidation;
using IManager.Web.Presentation.ViewModels.Departments;
using IManager.Web.Presentation.ViewModels.JobTitles;

namespace IManager.Web.Presentation.Validators.JobTitles;

public class CreateJobTitleViewModelValidator : AbstractValidator<CreateJobTitleViewModel>
{
    public CreateJobTitleViewModelValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O Nome do cargo é obrigatorio.");

        RuleFor(x => x.CompanyId)
            .NotEmpty().WithMessage("A empresa é obrigatória.");

        RuleFor(x => x.DepartmentId)
            .NotEmpty().WithMessage("O departamento é obrigatório.");
    }
}