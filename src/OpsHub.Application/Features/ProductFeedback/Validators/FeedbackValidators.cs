using FluentValidation;
using OpsHub.Application.Features.ProductFeedback.DTOs;

namespace OpsHub.Application.Features.ProductFeedback.Validators;

public class CreateSugerenciaRequestValidator : AbstractValidator<CreateSugerenciaRequest>
{
    public CreateSugerenciaRequestValidator()
    {
        RuleFor(x => x.Titulo)
            .NotEmpty().WithMessage("El título de la sugerencia es obligatorio.")
            .MaximumLength(200).WithMessage("El título no puede exceder 200 caracteres.");

        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción detallada del requerimiento es obligatoria.");

        RuleFor(x => x.ModuloAfectado)
            .NotEmpty().WithMessage("El módulo del sistema afectado es obligatorio.")
            .MaximumLength(100);
    }
}

public class PriorizarSugerenciaRequestValidator : AbstractValidator<PriorizarSugerenciaRequest>
{
    public PriorizarSugerenciaRequestValidator()
    {
        RuleFor(x => x.EsfuerzoTiDias)
            .InclusiveBetween(1, 180).WithMessage("El esfuerzo técnico estimado debe estar entre 1 y 180 días.");

        RuleFor(x => x.ImpactoComercialScore)
            .InclusiveBetween(1, 10).WithMessage("El score de impacto comercial debe estar entre 1 y 10.");
    }
}

public class AddClienteSolicitanteRequestValidator : AbstractValidator<AddClienteSolicitanteRequest>
{
    public AddClienteSolicitanteRequestValidator()
    {
        RuleFor(x => x.ClienteId)
            .GreaterThan(0).WithMessage("Debe seleccionar un cliente SaaS válido.");

        RuleFor(x => x.ValorAnualRepresentado)
            .GreaterThanOrEqualTo(0).WithMessage("El valor anual representado (ARR) no puede ser negativo.");
    }
}
