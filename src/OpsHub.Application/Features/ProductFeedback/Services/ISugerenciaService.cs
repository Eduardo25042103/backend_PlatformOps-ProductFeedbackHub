using OpsHub.Application.Features.Incidents.DTOs;
using OpsHub.Application.Features.ProductFeedback.DTOs;

namespace OpsHub.Application.Features.ProductFeedback.Services;

public interface ISugerenciaService
{
    Task<List<SugerenciaSummaryDto>> GetSugerenciasAsync(SugerenciaFilterDto filter, CancellationToken ct = default);
    Task<SugerenciaDto> GetSugerenciaByIdAsync(int id, CancellationToken ct = default);
    Task<SugerenciaDto> CreateSugerenciaAsync(CreateSugerenciaRequest request, int usuarioId, CancellationToken ct = default);
    Task<SugerenciaDto> ToggleVoteAsync(int sugerenciaId, int usuarioId, string? motivoVoto, CancellationToken ct = default);
    Task<SugerenciaDto> AddClienteSolicitanteAsync(int sugerenciaId, AddClienteSolicitanteRequest request, CancellationToken ct = default);
    Task<SugerenciaDto> PrioritizeAsync(int sugerenciaId, PriorizarSugerenciaRequest request, int evaluadoPorId, CancellationToken ct = default);
    Task<List<CatalogItemDto>> GetEstadosSugerenciaAsync(CancellationToken ct = default);
}
