using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OpsHub.Application.Common.Interfaces;
using OpsHub.Domain.Entities.AccessControl;

namespace OpsHub.Infrastructure.Services;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(UsuarioInterno usuario)
    {
        var secretKey = _configuration["JwtSettings:SecretKey"]
            ?? throw new InvalidOperationException("JwtSettings:SecretKey no está configurado.");
        var issuer = _configuration["JwtSettings:Issuer"] ?? "OpsHubApi";
        var audience = _configuration["JwtSettings:Audience"] ?? "OpsHubClients";
        var expirationMinutes = int.TryParse(_configuration["JwtSettings:ExpirationMinutes"], out var minutes)
            ? minutes
            : 480;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Email),
            new(JwtRegisteredClaimNames.GivenName, usuario.Nombre),
            new(JwtRegisteredClaimNames.FamilyName, usuario.Apellido),
            new(ClaimTypes.Role, usuario.Rol?.Codigo ?? string.Empty),
            new("departamento", usuario.Departamento?.Codigo ?? string.Empty),
            new("departamentoId", usuario.DepartamentoId.ToString()),
            new("rolId", usuario.RolId.ToString())
        };

        if (!string.IsNullOrEmpty(usuario.CognitoSub))
        {
            claims.Add(new Claim("cognito_sub", usuario.CognitoSub));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
