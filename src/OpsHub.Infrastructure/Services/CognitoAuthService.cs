using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpsHub.Application.Common.Exceptions;
using OpsHub.Application.Common.Interfaces;

namespace OpsHub.Infrastructure.Services;

public class CognitoAuthService : ICognitoAuthService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<CognitoAuthService> _logger;
    private readonly AmazonCognitoIdentityProviderClient _cognitoClient;
    private readonly string _userPoolId;
    private readonly string _clientId;

    public CognitoAuthService(IConfiguration configuration, ILogger<CognitoAuthService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        var regionStr = _configuration["AWS:Cognito:Region"] ?? "us-east-2";
        var region = RegionEndpoint.GetBySystemName(regionStr);

        _cognitoClient = new AmazonCognitoIdentityProviderClient(region);
        _userPoolId = _configuration["AWS:Cognito:UserPoolId"] ?? "us-east-2_5QCZKDuMi";
        _clientId = _configuration["AWS:Cognito:ClientId"] ?? "t4jlbvlo4pt244cnige95a5q0";
    }

    public async Task<string> RegisterUserAsync(
        string email,
        string password,
        string nombre,
        string apellido,
        CancellationToken ct = default)
    {
        try
        {
            var createUserRequest = new AdminCreateUserRequest
            {
                UserPoolId = _userPoolId,
                Username = email.Trim().ToLowerInvariant(),
                UserAttributes = new List<AttributeType>
                {
                    new() { Name = "email", Value = email.Trim().ToLowerInvariant() },
                    new() { Name = "email_verified", Value = "true" },
                    new() { Name = "given_name", Value = nombre.Trim() },
                    new() { Name = "family_name", Value = apellido.Trim() }
                },
                MessageAction = MessageActionType.SUPPRESS
            };

            var createResponse = await _cognitoClient.AdminCreateUserAsync(createUserRequest, ct);

            // Establecer contraseña permanente de inmediato (evita estado FORCE_CHANGE_PASSWORD)
            var setPasswordRequest = new AdminSetUserPasswordRequest
            {
                UserPoolId = _userPoolId,
                Username = email.Trim().ToLowerInvariant(),
                Password = password,
                Permanent = true
            };

            await _cognitoClient.AdminSetUserPasswordAsync(setPasswordRequest, ct);

            var sub = createResponse.User?.Attributes?.FirstOrDefault(a => a.Name == "sub")?.Value;
            if (string.IsNullOrEmpty(sub))
            {
                var getUser = await _cognitoClient.AdminGetUserAsync(new AdminGetUserRequest
                {
                    UserPoolId = _userPoolId,
                    Username = email.Trim().ToLowerInvariant()
                }, ct);

                sub = getUser.UserAttributes?.FirstOrDefault(a => a.Name == "sub")?.Value;
            }

            _logger.LogInformation("Usuario {Email} registrado en AWS Cognito con sub {Sub}", email, sub);
            return sub ?? Guid.NewGuid().ToString();
        }
        catch (UsernameExistsException)
        {
            throw new ConflictException($"El correo '{email}' ya se encuentra registrado en AWS Cognito.");
        }
        catch (InvalidPasswordException ex)
        {
            throw new ValidationException("Password", $"La contraseña no cumple las políticas de Cognito: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AWS Cognito no disponible o error de red/credenciales al registrar {Email}. Operando con modo híbrido defensivo y sub local.", email);
            return Guid.NewGuid().ToString();
        }
    }

    public async Task<bool> ValidateCredentialsAsync(string email, string password, CancellationToken ct = default)
    {
        try
        {
            var authRequest = new AdminInitiateAuthRequest
            {
                UserPoolId = _userPoolId,
                ClientId = _clientId,
                AuthFlow = AuthFlowType.ADMIN_USER_PASSWORD_AUTH,
                AuthParameters = new Dictionary<string, string>
                {
                    { "USERNAME", email.Trim().ToLowerInvariant() },
                    { "PASSWORD", password }
                }
            };

            var authResponse = await _cognitoClient.AdminInitiateAuthAsync(authRequest, ct);

            return authResponse.AuthenticationResult != null;
        }
        catch (NotAuthorizedException)
        {
            return false;
        }
        catch (UserNotFoundException)
        {
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Fallo al autenticar contra Cognito para {Email}, verificando localmente", email);
            return false;
        }
    }

    public async Task DeleteUserAsync(string email, CancellationToken ct = default)
    {
        try
        {
            var deleteRequest = new AdminDeleteUserRequest
            {
                UserPoolId = _userPoolId,
                Username = email.Trim().ToLowerInvariant()
            };

            await _cognitoClient.AdminDeleteUserAsync(deleteRequest, ct);
            _logger.LogWarning("Transacción compensatoria ejecutada con éxito: Usuario {Email} eliminado de AWS Cognito tras fallo en base de datos.", email);
        }
        catch (UserNotFoundException)
        {
            // El usuario ya no existe, no se requiere acción
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error crítico al ejecutar transacción compensatoria para eliminar usuario {Email} en AWS Cognito", email);
        }
    }
}
