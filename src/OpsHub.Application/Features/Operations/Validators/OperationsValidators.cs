using FluentValidation;
using OpsHub.Application.Features.Operations.DTOs;

namespace OpsHub.Application.Features.Operations.Validators;

public class CreateEvaluacionRequestValidator : AbstractValidator<CreateEvaluacionRequest>
{
    public CreateEvaluacionRequestValidator()
    {
        RuleFor(x => x.TicketId)
            .GreaterThan(0)
            .WithMessage("El ticket es requerido.");

        RuleFor(x => x.PuntajeEstrellas)
            .InclusiveBetween(1, 5)
            .WithMessage("La calificación debe estar entre 1 y 5 estrellas.");

        RuleFor(x => x.Comentarios)
            .MaximumLength(500)
            .WithMessage("Los comentarios no pueden exceder 500 caracteres.");
    }
}

public class CreateTurnoRequestValidator : AbstractValidator<CreateTurnoRequest>
{
    public CreateTurnoRequestValidator()
    {
        RuleFor(x => x.UsuarioId)
            .GreaterThan(0)
            .WithMessage("El usuario asignado al turno es requerido.");

        RuleFor(x => x.FechaFin)
            .GreaterThan(x => x.FechaInicio)
            .WithMessage("La fecha de fin del turno debe ser posterior a la fecha de inicio.");
    }
}

public class CreateNotificacionRequestValidator : AbstractValidator<CreateNotificacionRequest>
{
    public CreateNotificacionRequestValidator()
    {
        RuleFor(x => x.UsuarioId)
            .GreaterThan(0)
            .WithMessage("El destinatario de la notificación es requerido.");

        RuleFor(x => x.Titulo)
            .NotEmpty().WithMessage("El título de la notificación es requerido.")
            .MaximumLength(150).WithMessage("El título no puede exceder 150 caracteres.");

        RuleFor(x => x.Mensaje)
            .NotEmpty().WithMessage("El mensaje de la notificación es requerido.")
            .MaximumLength(500).WithMessage("El mensaje no puede exceder 500 caracteres.");

        RuleFor(x => x.TipoEvento)
            .NotEmpty().WithMessage("El tipo de evento es requerido.")
            .MaximumLength(50).WithMessage("El tipo de evento no puede exceder 50 caracteres.");
    }
}
