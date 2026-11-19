using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpsHub.Application.Common.Models;
using OpsHub.Application.Features.Announcements.DTOs;
using OpsHub.Application.Features.Announcements.Services;

namespace OpsHub.Api.Controllers;

[Route("api/v1/announcements")]
public class AnnouncementsController : BaseApiController
{
    private readonly IAnnouncementService _announcementService;

    public AnnouncementsController(IAnnouncementService announcementService)
    {
        _announcementService = announcementService;
    }

    /// <summary>
    /// Obtiene los comunicados del sistema con filtros opcionales de tipo y afectación de servicio.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ComunicadoDto>>>> GetAnnouncements(
        [FromQuery] int? tipoId = null,
        [FromQuery] bool? afectaServicio = null,
        CancellationToken cancellationToken = default)
    {
        var data = await _announcementService.GetAnnouncementsAsync(tipoId, afectaServicio, cancellationToken);
        return Ok(ApiResponse<List<ComunicadoDto>>.Ok(data));
    }

    /// <summary>
    /// Obtiene los comunicados activos actuales (degradación de servicio o emitidos en las últimas 72 horas).
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<ApiResponse<List<ComunicadoDto>>>> GetActiveAnnouncements(
        CancellationToken cancellationToken = default)
    {
        var data = await _announcementService.GetActiveAnnouncementsAsync(cancellationToken);
        return Ok(ApiResponse<List<ComunicadoDto>>.Ok(data));
    }

    /// <summary>
    /// Publica un nuevo comunicado oficial o banner de estado del sistema.
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ApiResponse<ComunicadoDto>>> CreateAnnouncement(
        [FromBody] CreateComunicadoRequest request,
        CancellationToken cancellationToken = default)
    {
        var data = await _announcementService.CreateAnnouncementAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetAnnouncements), new { id = data.Id }, ApiResponse<ComunicadoDto>.Ok(data, "Comunicado publicado exitosamente."));
    }

    /// <summary>
    /// Consulta el historial de versiones del sistema (Changelog de producto).
    /// </summary>
    [HttpGet("changelog")]
    public async Task<ActionResult<ApiResponse<List<ChangelogDto>>>> GetChangelog(
        CancellationToken cancellationToken = default)
    {
        var data = await _announcementService.GetChangelogsAsync(cancellationToken);
        return Ok(ApiResponse<List<ChangelogDto>>.Ok(data));
    }

    /// <summary>
    /// Registra una nueva versión de actualización en el Changelog.
    /// </summary>
    [HttpPost("changelog")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<ChangelogDto>>> CreateChangelog(
        [FromBody] CreateChangelogRequest request,
        CancellationToken cancellationToken = default)
    {
        var data = await _announcementService.CreateChangelogAsync(request, cancellationToken);
        return Ok(ApiResponse<ChangelogDto>.Ok(data, "Anuncio de versión registrado en el Changelog."));
    }

    /// <summary>
    /// Obtiene el catálogo de tipos de comunicados (Mantenimiento, Incidencia General, Informativo, etc.).
    /// </summary>
    [HttpGet("types")]
    public async Task<ActionResult<ApiResponse<List<TipoComunicadoDto>>>> GetTypes(
        CancellationToken cancellationToken = default)
    {
        var data = await _announcementService.GetTiposComunicadoAsync(cancellationToken);
        return Ok(ApiResponse<List<TipoComunicadoDto>>.Ok(data));
    }
}
