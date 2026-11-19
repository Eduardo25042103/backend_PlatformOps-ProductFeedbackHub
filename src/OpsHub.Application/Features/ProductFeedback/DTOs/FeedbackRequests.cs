namespace OpsHub.Application.Features.ProductFeedback.DTOs;

public class CreateSugerenciaRequest
{
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string ModuloAfectado { get; set; } = string.Empty;
}

public class VotarSugerenciaRequest
{
    public string? MotivoVoto { get; set; }
}

public class AddClienteSolicitanteRequest
{
    public int ClienteId { get; set; }
    public decimal ValorAnualRepresentado { get; set; }
}

public class PriorizarSugerenciaRequest
{
    public int EsfuerzoTiDias { get; set; } // 1 a 60 días
    public int ImpactoComercialScore { get; set; } // 1 a 10
}

public class SugerenciaFilterDto
{
    public int? EstadoId { get; set; }
    public string? ModuloAfectado { get; set; }
    public string? SearchTerm { get; set; }
    public string? OrderBy { get; set; } // 'prioridad', 'reciente', 'votos', 'arr'
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
