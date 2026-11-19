using OpsHub.Domain.Entities.AccessControl;

namespace OpsHub.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(UsuarioInterno usuario);
}
