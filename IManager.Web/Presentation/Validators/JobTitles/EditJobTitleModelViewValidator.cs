using FluentValidation;
using IManager.Web.Presentation.ViewModels.JobTitles;

namespace IManager.Web.Presentation.Validators.JobTitles;

public class EditJobTitleModelViewValidator : AbstractValidator<EditJobTitleModelView>
{
    public EditJobTitleModelViewValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("o Id é obrigatório.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O Nome do cargo é obrigatorio.");

        RuleFor(x => x.DailyHours)
            .Must(x => x > TimeSpan.Zero && x.TotalHours <= 24)
            .WithMessage("A carga horária diária deve estar entre 00:00 e 23:59.");
    }
}