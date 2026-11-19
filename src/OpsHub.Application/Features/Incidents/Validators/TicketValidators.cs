using FluentValidation;
using OpsHub.Application.Features.Incidents.DTOs;

namespace OpsHub.Application.Features.Incidents.Validators;

public class CreateTicketRequestValidator : AbstractValidator<CreateTicketRequest>
{
    public CreateTicketRequestValidator()
    {
        RuleFor(x => x.ClienteId)
            .GreaterThan(0).WithMessage("Debe seleccionar un cliente SaaS válido.");

        RuleFor(x => x.CategoriaId)
            .GreaterThan(0).WithMessage("Debe seleccionar una categoría de incidencia válida.");

        RuleFor(x => x.PrioridadId)
            .GreaterThan(0).WithMessage("Debe seleccionar una prioridad válida.");

        RuleFor(x => x.Titulo)
            .NotEmpty().WithMessage("El título del ticket es obligatorio.")
            .MaximumLength(200).WithMessage("El título no puede exceder 200 caracteres.");

        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción detallada del problema es obligatoria.");

        RuleFor(x => x.CanalOrigen)
            .NotEmpty().WithMessage("El canal de origen es obligatorio.")
            .MaximumLength(20);
    }
}

public class UpdateTicketStatusRequestValidator : AbstractValidator<UpdateTicketStatusRequest>
{
    public UpdateTicketStatusRequestValidator()
    {
        RuleFor(x => x.NuevoEstadoId)
            .GreaterThan(0).WithMessage("Debe indicar el nuevo estado del ticket.");

        RuleFor(x => x.CurrentVersion)
            .GreaterThan(0).WithMessage("El número de versión actual es obligatorio para el control de concurrencia optimista.");
    }
}

public class EscalateTicketRequestValidator : AbstractValidator<EscalateTicketRequest>
{
    public EscalateTicketRequestValidator()
    {
        RuleFor(x => x.AsignadoAId)
            .GreaterThan(0).WithMessage("Debe seleccionar el colaborador técnico al cual escalar.");

        RuleFor(x => x.MotivoEscalacion)
            .NotEmpty().WithMessage("El motivo de escalación es obligatorio.")
            .MaximumLength(255);

        RuleFor(x => x.CurrentVersion)
            .GreaterThan(0).WithMessage("La versión del ticket es obligatoria.");
    }
}

public class AddCommentRequestValidator : AbstractValidator<AddCommentRequest>
{
    public AddCommentRequestValidator()
    {
        RuleFor(x => x.Comentario)
            .NotEmpty().WithMessage("El contenido del comentario es obligatorio.");
    }
}

public class ConfirmAttachmentRequestValidator : AbstractValidator<ConfirmAttachmentRequest>
{
    public ConfirmAttachmentRequestValidator()
    {
        RuleFor(x => x.NombreArchivo).NotEmpty().WithMessage("El nombre del archivo es obligatorio.");
        RuleFor(x => x.S3Key).NotEmpty().WithMessage("La clave S3 del archivo es obligatoria.");
        RuleFor(x => x.TipoMime).NotEmpty().WithMessage("El tipo MIME es obligatorio.");
    }
}
