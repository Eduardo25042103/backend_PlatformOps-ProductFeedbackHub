namespace OpsHub.Application.Features.Incidents.DTOs;

public class TicketDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string? TelefonoContacto { get; set; }
    public string CanalOrigen { get; set; } = string.Empty;
    
    // Bloqueo Optimista
    public int Version { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? FechaCierre { get; set; }

    // Cliente SaaS
    public int ClienteId { get; set; }
    public string ClienteRazonSocial { get; set; } = string.Empty;
    public string ClienteRuc { get; set; } = string.Empty;

    // Usuarios
    public int ReportadoPorId { get; set; }
    public string ReportadoPorNombre { get; set; } = string.Empty;
    public int? AsignadoAId { get; set; }
    public string? AsignadoANombre { get; set; }

    // Catálogos
    public int CategoriaId { get; set; }
    public string CategoriaCodigo { get; set; } = string.Empty;
    public string CategoriaNombre { get; set; } = string.Empty;

    public int PrioridadId { get; set; }
    public string PrioridadCodigo { get; set; } = string.Empty;
    public string PrioridadNombre { get; set; } = string.Empty;
    public string PrioridadColorHex { get; set; } = string.Empty;
    public int TiempoMaxResolucionHoras { get; set; }

    public int EstadoId { get; set; }
    public string EstadoCodigo { get; set; } = string.Empty;
    public string EstadoNombre { get; set; } = string.Empty;
    public string EstadoColorHex { get; set; } = string.Empty;
    public bool EsFinal { get; set; }

    public List<TicketComentarioDto> Comentarios { get; set; } = new();
    public List<TicketAdjuntoDto> Adjuntos { get; set; } = new();
    public List<EscalacionDto> Escalaciones { get; set; } = new();
}

public class TicketComentarioDto
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string UsuarioNombre { get; set; } = string.Empty;
    public string Comentario { get; set; } = string.Empty;
    public bool EsPrivadoTi { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TicketAdjuntoDto
{
    public int Id { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string S3Url { get; set; } = string.Empty;
    public string TipoMime { get; set; } = string.Empty;
    public long TamanioBytes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class EscalacionDto
{
    public int Id { get; set; }
    public string EscaladoPorNombre { get; set; } = string.Empty;
    public string AsignadoANombre { get; set; } = string.Empty;
    public string MotivoEscalacion { get; set; } = string.Empty;
    public string? DiagnosticoInicial { get; set; }
    public DateTime CreatedAt { get; set; }
}
