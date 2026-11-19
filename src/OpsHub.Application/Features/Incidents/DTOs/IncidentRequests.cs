namespace OpsHub.Application.Features.Incidents.DTOs;

public class CreateTicketRequest
{
    public int ClienteId { get; set; }
    public int CategoriaId { get; set; }
    public int PrioridadId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string? TelefonoContacto { get; set; }
    public string CanalOrigen { get; set; } = "WEB";
}

public class UpdateTicketStatusRequest
{
    public int NuevoEstadoId { get; set; }
    public int CurrentVersion { get; set; } // Validación estricta de bloqueo optimista
    public string? Comentario { get; set; }
}

public class EscalateTicketRequest
{
    public int AsignadoAId { get; set; }
    public string MotivoEscalacion { get; set; } = string.Empty;
    public string? DiagnosticoInicial { get; set; }
    public int CurrentVersion { get; set; }
}

public class AddCommentRequest
{
    public string Comentario { get; set; } = string.Empty;
    public bool EsPrivadoTi { get; set; } = false;
}

public class PresignedUrlRequest
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/octet-stream";
}

public class PresignedUrlResponse
{
    public string UploadUrl { get; set; } = string.Empty;
    public string S3Key { get; set; } = string.Empty;
    public int ExpiresInMinutes { get; set; } = 5;
}

public class ConfirmAttachmentRequest
{
    public string NombreArchivo { get; set; } = string.Empty;
    public string S3Key { get; set; } = string.Empty;
    public string TipoMime { get; set; } = string.Empty;
    public long TamanioBytes { get; set; }
}

public class TicketFilterDto
{
    public int? EstadoId { get; set; }
    public int? PrioridadId { get; set; }
    public int? CategoriaId { get; set; }
    public int? AsignadoAId { get; set; }
    public string? SearchTerm { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class CatalogItemDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? ColorHex { get; set; }
    public string? Descripcion { get; set; }
}
