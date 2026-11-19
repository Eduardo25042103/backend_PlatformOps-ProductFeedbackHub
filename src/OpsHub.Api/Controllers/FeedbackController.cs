using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpsHub.Application.Common.Interfaces;
using OpsHub.Application.Common.Models;
using OpsHub.Application.Features.Incidents.DTOs;
using OpsHub.Application.Features.ProductFeedback.DTOs;
using OpsHub.Application.Features.ProductFeedback.Services;

namespace OpsHub.Api.Controllers;

[Authorize]
public class FeedbackController : BaseApiController
{
    private readonly ISugerenciaService _sugerenciaService;
    private readonly ICurrentUserService _currentUserService;

    public FeedbackController(ISugerenciaService sugerenciaService, ICurrentUserService currentUserService)
    {
        _sugerenciaService = sugerenciaService;
        _currentUserService = currentUserService;
    }

    [HttpGet("sugerencias")]
    [ProducesResponseType(typeof(ApiResponse<List<SugerenciaSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSugerencias([FromQuery] SugerenciaFilterDto filter, CancellationToken ct)
    {
        var result = await _sugerenciaService.GetSugerenciasAsync(filter, ct);
        return Ok(ApiResponse<List<SugerenciaSummaryDto>>.Ok(result, "Sugerencias de producto obtenidas"));
    }

    [HttpGet("sugerencias/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SugerenciaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSugerenciaById(int id, CancellationToken ct)
    {
        var result = await _sugerenciaService.GetSugerenciaByIdAsync(id, ct);
        return Ok(ApiResponse<SugerenciaDto>.Ok(result, "Detalle de la sugerencia obtenido"));
    }

    [HttpPost("sugerencias")]
    [ProducesResponseType(typeof(ApiResponse<SugerenciaDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSugerencia([FromBody] CreateSugerenciaRequest request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId ?? 1;
        var result = await _sugerenciaService.CreateSugerenciaAsync(request, userId, ct);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<SugerenciaDto>.Ok(result, "Sugerencia registrada con éxito"));
    }

    [HttpPost("sugerencias/{id:int}/votar")]
    [ProducesResponseType(typeof(ApiResponse<SugerenciaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ToggleVote(int id, [FromBody] VotarSugerenciaRequest? request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId ?? 1;
        var result = await _sugerenciaService.ToggleVoteAsync(id, userId, request?.MotivoVoto, ct);
        return Ok(ApiResponse<SugerenciaDto>.Ok(result, "Voto procesado correctamente"));
    }

    [HttpPost("sugerencias/{id:int}/clientes")]
    [ProducesResponseType(typeof(ApiResponse<SugerenciaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddClienteSolicitante(int id, [FromBody] AddClienteSolicitanteRequest request, CancellationToken ct)
    {
        var result = await _sugerenciaService.AddClienteSolicitanteAsync(id, request, ct);
        return Ok(ApiResponse<SugerenciaDto>.Ok(result, "Cliente solicitante vinculado a la sugerencia"));
    }

    [HttpPut("sugerencias/{id:int}/priorizar")]
    [ProducesResponseType(typeof(ApiResponse<SugerenciaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Prioritize(int id, [FromBody] PriorizarSugerenciaRequest request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId ?? 1;
        var result = await _sugerenciaService.PrioritizeAsync(id, request, userId, ct);
        return Ok(ApiResponse<SugerenciaDto>.Ok(result, "Priorización técnica y comercial recalculada"));
    }

    [HttpGet("estados")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<List<CatalogItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEstadosSugerencia(CancellationToken ct)
    {
        var result = await _sugerenciaService.GetEstadosSugerenciaAsync(ct);
        return Ok(ApiResponse<List<CatalogItemDto>>.Ok(result, "Catálogo de estados de sugerencias"));
    }
}
