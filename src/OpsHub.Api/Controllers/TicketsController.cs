using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpsHub.Application.Common.Interfaces;
using OpsHub.Application.Common.Models;
using OpsHub.Application.Features.Incidents.DTOs;
using OpsHub.Application.Features.Incidents.Services;

namespace OpsHub.Api.Controllers;

[Authorize]
public class TicketsController : BaseApiController
{
    private readonly ITicketService _ticketService;
    private readonly ICurrentUserService _currentUserService;

    public TicketsController(ITicketService ticketService, ICurrentUserService currentUserService)
    {
        _ticketService = ticketService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<TicketSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTickets([FromQuery] TicketFilterDto filter, CancellationToken ct)
    {
        var result = await _ticketService.GetTicketsAsync(filter, ct);
        return Ok(ApiResponse<List<TicketSummaryDto>>.Ok(result, "Tickets obtenidos correctamente"));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<TicketDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTicketById(int id, CancellationToken ct)
    {
        var result = await _ticketService.GetTicketByIdAsync(id, ct);
        return Ok(ApiResponse<TicketDto>.Ok(result, "Detalle del ticket obtenido"));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TicketDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTicket([FromBody] CreateTicketRequest request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId ?? 1;
        var result = await _ticketService.CreateTicketAsync(request, userId, ct);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<TicketDto>.Ok(result, "Ticket creado con éxito"));
    }

    [HttpPut("{id:int}/status")]
    [ProducesResponseType(typeof(ApiResponse<TicketDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateTicketStatus(int id, [FromBody] UpdateTicketStatusRequest request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId ?? 1;
        var result = await _ticketService.UpdateTicketStatusAsync(id, request, userId, ct);
        return Ok(ApiResponse<TicketDto>.Ok(result, "Estado del ticket actualizado exitosamente"));
    }

    [HttpPost("{id:int}/escalate")]
    [ProducesResponseType(typeof(ApiResponse<TicketDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> EscalateTicket(int id, [FromBody] EscalateTicketRequest request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId ?? 1;
        var result = await _ticketService.EscalateTicketAsync(id, request, userId, ct);
        return Ok(ApiResponse<TicketDto>.Ok(result, "Ticket escalado a TI correctamente"));
    }

    [HttpPost("{id:int}/comments")]
    [ProducesResponseType(typeof(ApiResponse<TicketComentarioDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddComment(int id, [FromBody] AddCommentRequest request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId ?? 1;
        var result = await _ticketService.AddCommentAsync(id, request, userId, ct);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<TicketComentarioDto>.Ok(result, "Comentario añadido"));
    }

    [HttpPost("{id:int}/attachments/presigned-url")]
    [ProducesResponseType(typeof(ApiResponse<PresignedUrlResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPresignedUploadUrl(int id, [FromBody] PresignedUrlRequest request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId ?? 1;
        var result = await _ticketService.GetPresignedUploadUrlAsync(id, request, userId, ct);
        return Ok(ApiResponse<PresignedUrlResponse>.Ok(result, "URL prefirmada generada con vigencia de 5 minutos"));
    }

    [HttpPost("{id:int}/attachments")]
    [ProducesResponseType(typeof(ApiResponse<TicketAdjuntoDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> ConfirmAttachment(int id, [FromBody] ConfirmAttachmentRequest request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId ?? 1;
        var result = await _ticketService.ConfirmAttachmentAsync(id, request, userId, ct);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<TicketAdjuntoDto>.Ok(result, "Adjunto confirmado"));
    }
}
