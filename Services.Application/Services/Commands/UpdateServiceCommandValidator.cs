using FluentValidation;

namespace Services.Application.Services.Commands;

public class UpdateServiceCommandValidator : AbstractValidator<UpdateServiceCommand>
{
    public UpdateServiceCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEqual(Guid.Empty).WithMessage("El identificador del servicio es obligatorio.");

        RuleFor(v => v.Title)
            .NotEmpty().WithMessage("El título del servicio es obligatorio.")
            .MaximumLength(150).WithMessage("El título no puede superar los 150 caracteres.");

        RuleFor(v => v.Description)
            .NotEmpty().WithMessage("La descripción es obligatoria.");

        RuleFor(v => v.Price)
            .GreaterThan(0).WithMessage("El precio debe ser mayor a cero.");
    }
}