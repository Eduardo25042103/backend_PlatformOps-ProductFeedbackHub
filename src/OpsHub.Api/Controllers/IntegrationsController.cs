using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpsHub.Application.Common.Models;
using OpsHub.Application.Features.Integrations.Zoho.DTOs;
using OpsHub.Application.Features.Integrations.Zoho.Services;

namespace OpsHub.Api.Controllers;

[AllowAnonymous]
public class IntegrationsController : BaseApiController
{
    private readonly IZohoWebhookService _zohoWebhookService;

    public IntegrationsController(IZohoWebhookService zohoWebhookService)
    {
        _zohoWebhookService = zohoWebhookService;
    }

    [HttpPost("zoho/webhook")]
    [ProducesResponseType(typeof(ApiResponse<ZohoWebhookResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReceiveZohoWebhook(
        [FromHeader(Name = "X-Zoho-Signature")] string? signature,
        CancellationToken ct)
    {
        // Leer el cuerpo crudo de la petición para verificación estricta de HMAC-SHA256
        using var reader = new StreamReader(Request.Body);
        var rawBody = await reader.ReadToEndAsync(ct);

        if (string.IsNullOrWhiteSpace(rawBody))
            return BadRequest(ApiResponse<object>.Fail("El cuerpo del webhook de Zoho no puede estar vacío."));

        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        ZohoWebhookPayload? payload;

        try
        {
            payload = JsonSerializer.Deserialize<ZohoWebhookPayload>(rawBody, jsonOptions);
        }
        catch (JsonException ex)
        {
            return BadRequest(ApiResponse<object>.Fail($"Formato JSON inválido en el payload de Zoho: {ex.Message}"));
        }

        if (payload == null)
            return BadRequest(ApiResponse<object>.Fail("No se pudo interpretar el payload de Zoho."));

        var result = await _zohoWebhookService.ProcessWebhookAsync(payload, rawBody, signature, ct);
        return Ok(ApiResponse<ZohoWebhookResult>.Ok(result, "Webhook de Zoho CRM procesado correctamente"));
    }
}
