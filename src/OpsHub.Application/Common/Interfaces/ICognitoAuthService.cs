namespace OpsHub.Application.Common.Interfaces;

public interface ICognitoAuthService
{
    Task<string> RegisterUserAsync(string email, string password, string nombre, string apellido, CancellationToken ct = default);
    Task<bool> ValidateCredentialsAsync(string email, string password, CancellationToken ct = default);
    Task DeleteUserAsync(string email, CancellationToken ct = default);
}
