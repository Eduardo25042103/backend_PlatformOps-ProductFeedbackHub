using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpsHub.Application.Common.Interfaces;
using OpsHub.Application.Common.Models;
using OpsHub.Application.Features.Incidents.DTOs;
using OpsHub.Application.Features.Incidents.Services;

namespace OpsHub.Api.Controllers;

[AllowAnonymous]
public class CatalogsController : BaseApiController
{
    private readonly ITicketService _ticketService;
    private readonly IApplicationDbContext _context;

    public CatalogsController(ITicketService ticketService, IApplicationDbContext context)
    {
        _ticketService = ticketService;
        _context = context;
    }

    [HttpGet("departamentos")]
    [ProducesResponseType(typeof(ApiResponse<List<CatalogItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDepartamentos(CancellationToken ct)
    {
        var deptos = await _context.Departamentos
            .AsNoTracking()
            .Where(d => d.IsActive)
            .OrderBy(d => d.Nombre)
            .Select(d => new CatalogItemDto
            {
                Id = d.Id,
                Codigo = d.Codigo,
                Nombre = d.Nombre,
                Descripcion = d.Descripcion
            })
            .ToListAsync(ct);

        return Ok(ApiResponse<List<CatalogItemDto>>.Ok(deptos, "Catálogo de departamentos"));
    }

    [HttpGet("prioridades")]
    [ProducesResponseType(typeof(ApiResponse<List<CatalogItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPrioridades(CancellationToken ct)
    {
        var result = await _ticketService.GetPrioridadesAsync(ct);
        return Ok(ApiResponse<List<CatalogItemDto>>.Ok(result, "Catálogo de prioridades"));
    }

    [HttpGet("estados-ticket")]
    [ProducesResponseType(typeof(ApiResponse<List<CatalogItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEstadosTicket(CancellationToken ct)
    {
        var result = await _ticketService.GetEstadosAsync(ct);
        return Ok(ApiResponse<List<CatalogItemDto>>.Ok(result, "Catálogo de estados de ticket"));
    }

    [HttpGet("categorias-incidencia")]
    [ProducesResponseType(typeof(ApiResponse<List<CatalogItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategoriasIncidencia(CancellationToken ct)
    {
        var result = await _ticketService.GetCategoriasAsync(ct);
        return Ok(ApiResponse<List<CatalogItemDto>>.Ok(result, "Catálogo de categorías de incidencia"));
    }
}
