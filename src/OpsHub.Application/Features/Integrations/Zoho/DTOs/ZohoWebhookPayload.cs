namespace OpsHub.Application.Features.Integrations.Zoho.DTOs;

public class ZohoWebhookPayload
{
    public string Ruc { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public decimal MontoAnual { get; set; }
    public string RequerimientoTitulo { get; set; } = string.Empty;
    public string RequerimientoDescripcion { get; set; } = string.Empty;
    public string ModuloAfectado { get; set; } = "GENERAL";
}

public class ZohoWebhookResult
{
    public bool Processed { get; set; }
    public string Message { get; set; } = string.Empty;
    public int ClienteId { get; set; }
    public int SugerenciaId { get; set; }
    public string SugerenciaCodigo { get; set; } = string.Empty;
    public decimal PuntuacionActualizada { get; set; }
}
