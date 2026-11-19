using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpsHub.Application.Common.Interfaces;

namespace OpsHub.Infrastructure.Services;

public class ZohoSignatureValidator : IZohoSignatureValidator
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ZohoSignatureValidator> _logger;

    public ZohoSignatureValidator(IConfiguration configuration, ILogger<ZohoSignatureValidator> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public bool ValidateSignature(string payload, string? signatureHeader)
    {
        if (string.IsNullOrWhiteSpace(signatureHeader))
            return false;

        var secret = _configuration["Zoho:WebhookSecret"];
        if (string.IsNullOrWhiteSpace(secret))
        {
            _logger.LogWarning("Zoho:WebhookSecret no está configurado. Rechazando webhook por seguridad.");
            return false;
        }

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var hexSignature = Convert.ToHexString(hash).ToLowerInvariant();
        var base64Signature = Convert.ToBase64String(hash);

        var cleanedHeader = signatureHeader.Trim();

        // Soporta firmas en formato Hexadecimal (sha256=...) o Base64
        if (cleanedHeader.StartsWith("sha256=", StringComparison.OrdinalIgnoreCase))
            cleanedHeader = cleanedHeader.Substring(7);

        return string.Equals(cleanedHeader, hexSignature, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(cleanedHeader, base64Signature, StringComparison.Ordinal);
    }
}
