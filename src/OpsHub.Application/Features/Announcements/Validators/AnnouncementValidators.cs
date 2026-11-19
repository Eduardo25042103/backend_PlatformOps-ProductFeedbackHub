using FluentValidation;
using OpsHub.Application.Features.Announcements.DTOs;

namespace OpsHub.Application.Features.Announcements.Validators;

public class CreateComunicadoRequestValidator : AbstractValidator<CreateComunicadoRequest>
{
    public CreateComunicadoRequestValidator()
    {
        RuleFor(x => x.TipoId)
            .GreaterThan(0)
            .WithMessage("Debe seleccionar un tipo de comunicado válido.");

        RuleFor(x => x.Titulo)
            .NotEmpty().WithMessage("El título del comunicado es requerido.")
            .MaximumLength(200).WithMessage("El título no puede exceder 200 caracteres.");

        RuleFor(x => x.Mensaje)
            .NotEmpty().WithMessage("El mensaje del comunicado es requerido.");
    }
}

public class CreateChangelogRequestValidator : AbstractValidator<CreateChangelogRequest>
{
    public CreateChangelogRequestValidator()
    {
        RuleFor(x => x.VersionTag)
            .NotEmpty().WithMessage("La versión es requerida (ej. v1.2.0).")
            .MaximumLength(20).WithMessage("La versión no puede exceder 20 caracteres.");

        RuleFor(x => x.Titulo)
            .NotEmpty().WithMessage("El título del anuncio es requerido.")
            .MaximumLength(200).WithMessage("El título no puede exceder 200 caracteres.");

        RuleFor(x => x.Resumen)
            .NotEmpty().WithMessage("El resumen del changelog es requerido.");

        RuleFor(x => x.FechaLanzamiento)
            .NotEmpty().WithMessage("La fecha de lanzamiento es requerida.");
    }
}
