using FluentValidation;

namespace Services.Application.Services.Commands;

public class DeleteServiceCommandValidator : AbstractValidator<DeleteServiceCommand>
{
    public DeleteServiceCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEqual(Guid.Empty).WithMessage("El identificador del servicio es obligatorio para la eliminación.");
    }
}