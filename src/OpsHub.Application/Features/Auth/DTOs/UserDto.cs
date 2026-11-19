namespace OpsHub.Application.Features.Auth.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public int DepartamentoId { get; set; }
    public string DepartamentoCodigo { get; set; } = string.Empty;
    public string DepartamentoNombre { get; set; } = string.Empty;
    public int RolId { get; set; }
    public string RolCodigo { get; set; } = string.Empty;
    public string RolNombre { get; set; } = string.Empty;
    public string? CognitoSub { get; set; }
    public bool IsActive { get; set; }
    public DateTime? UltimoLogin { get; set; }
    public List<string> Permisos { get; set; } = new();
}
