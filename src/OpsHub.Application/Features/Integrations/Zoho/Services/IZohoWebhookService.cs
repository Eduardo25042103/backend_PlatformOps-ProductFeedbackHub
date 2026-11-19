using OpsHub.Application.Features.Integrations.Zoho.DTOs;

namespace OpsHub.Application.Features.Integrations.Zoho.Services;

public interface IZohoWebhookService
{
    Task<ZohoWebhookResult> ProcessWebhookAsync(ZohoWebhookPayload payload, string rawBody, string? signatureHeader, CancellationToken ct = default);
}
