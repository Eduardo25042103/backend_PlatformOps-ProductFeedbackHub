namespace OpsHub.Application.Common.Interfaces;

public interface IZohoSignatureValidator
{
    bool ValidateSignature(string payload, string? signatureHeader);
}
