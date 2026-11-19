namespace OpsHub.Application.Common.Interfaces;

public interface ICurrentUserService
{
    int? UserId { get; }
    string? Email { get; }
    string? RolCodigo { get; }
    string? DepartamentoCodigo { get; }
    string? IpAddress { get; }
    string? Platform { get; }
}
