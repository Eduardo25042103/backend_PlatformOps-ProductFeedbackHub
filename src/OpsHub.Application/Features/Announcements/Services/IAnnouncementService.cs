using OpsHub.Application.Features.Announcements.DTOs;

namespace OpsHub.Application.Features.Announcements.Services;

public interface IAnnouncementService
{
    Task<List<ComunicadoDto>> GetActiveAnnouncementsAsync(CancellationToken cancellationToken = default);
    Task<List<ComunicadoDto>> GetAnnouncementsAsync(int? tipoId = null, bool? afectaServicio = null, CancellationToken cancellationToken = default);
    Task<ComunicadoDto> CreateAnnouncementAsync(CreateComunicadoRequest request, CancellationToken cancellationToken = default);
    Task<List<ChangelogDto>> GetChangelogsAsync(CancellationToken cancellationToken = default);
    Task<ChangelogDto> CreateChangelogAsync(CreateChangelogRequest request, CancellationToken cancellationToken = default);
    Task<List<TipoComunicadoDto>> GetTiposComunicadoAsync(CancellationToken cancellationToken = default);
}
