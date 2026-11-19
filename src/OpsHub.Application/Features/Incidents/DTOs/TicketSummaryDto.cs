namespace OpsHub.Application.Features.Incidents.DTOs;

public class TicketSummaryDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string ClienteRazonSocial { get; set; } = string.Empty;
    public string PrioridadCodigo { get; set; } = string.Empty;
    public string PrioridadNombre { get; set; } = string.Empty;
    public string PrioridadColorHex { get; set; } = string.Empty;
    public string EstadoCodigo { get; set; } = string.Empty;
    public string EstadoNombre { get; set; } = string.Empty;
    public string EstadoColorHex { get; set; } = string.Empty;
    public string CategoriaNombre { get; set; } = string.Empty;
    public string? AsignadoANombre { get; set; }
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; }
}
