using OpsHub.Application.Features.Incidents.DTOs;

namespace OpsHub.Application.Features.Incidents.Services;

public interface ITicketService
{
    Task<List<TicketSummaryDto>> GetTicketsAsync(TicketFilterDto filter, CancellationToken ct = default);
    Task<TicketDto> GetTicketByIdAsync(int id, CancellationToken ct = default);
    Task<TicketDto> CreateTicketAsync(CreateTicketRequest request, int reportadoPorId, CancellationToken ct = default);
    Task<TicketDto> UpdateTicketStatusAsync(int id, UpdateTicketStatusRequest request, int usuarioId, CancellationToken ct = default);
    Task<TicketDto> EscalateTicketAsync(int id, EscalateTicketRequest request, int escaladoPorId, CancellationToken ct = default);
    Task<TicketComentarioDto> AddCommentAsync(int ticketId, AddCommentRequest request, int usuarioId, CancellationToken ct = default);
    Task<PresignedUrlResponse> GetPresignedUploadUrlAsync(int ticketId, PresignedUrlRequest request, int usuarioId, CancellationToken ct = default);
    Task<TicketAdjuntoDto> ConfirmAttachmentAsync(int ticketId, ConfirmAttachmentRequest request, int usuarioId, CancellationToken ct = default);

    Task<List<CatalogItemDto>> GetPrioridadesAsync(CancellationToken ct = default);
    Task<List<CatalogItemDto>> GetEstadosAsync(CancellationToken ct = default);
    Task<List<CatalogItemDto>> GetCategoriasAsync(CancellationToken ct = default);
}
