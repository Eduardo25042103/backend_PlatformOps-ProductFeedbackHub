namespace OpsHub.Application.Features.ProductFeedback.DTOs;

public class SugerenciaDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string ModuloAfectado { get; set; } = string.Empty;
    public int EstadoId { get; set; }
    public string EstadoCodigo { get; set; } = string.Empty;
    public string EstadoNombre { get; set; } = string.Empty;
    public int CreadoPorId { get; set; }
    public string CreadoPorNombre { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public int TotalVotos { get; set; }
    public int TotalClientesSolicitantes { get; set; }
    public decimal TotalArrRepresentado { get; set; }
    public PriorizacionDto? Priorizacion { get; set; }

    public List<VotoDto> Votos { get; set; } = new();
    public List<ClienteSolicitanteDto> ClientesSolicitantes { get; set; } = new();
}

public class PriorizacionDto
{
    public int Id { get; set; }
    public int EsfuerzoTiDias { get; set; }
    public int ImpactoComercialScore { get; set; }
    public decimal PuntuacionFinal { get; set; }
    public string EvaluadoPorNombre { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
}

public class VotoDto
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string UsuarioNombre { get; set; } = string.Empty;
    public string? MotivoVoto { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ClienteSolicitanteDto
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string ClienteRazonSocial { get; set; } = string.Empty;
    public string ClienteRuc { get; set; } = string.Empty;
    public decimal ValorAnualRepresentado { get; set; }
    public string OrigenWebhook { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class SugerenciaSummaryDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string ModuloAfectado { get; set; } = string.Empty;
    public string EstadoCodigo { get; set; } = string.Empty;
    public string EstadoNombre { get; set; } = string.Empty;
    public int TotalVotos { get; set; }
    public int TotalClientes { get; set; }
    public decimal TotalArr { get; set; }
    public decimal PuntuacionPriorizacion { get; set; }
    public DateTime CreatedAt { get; set; }
}
