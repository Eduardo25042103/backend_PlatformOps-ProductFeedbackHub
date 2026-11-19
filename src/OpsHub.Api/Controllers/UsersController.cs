using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpsHub.Application.Common.Interfaces;
using OpsHub.Application.Common.Models;
using OpsHub.Application.Features.Auth.DTOs;
using OpsHub.Application.Features.Auth.Services;
using OpsHub.Domain.Entities.AccessControl;

namespace OpsHub.Api.Controllers;

[Authorize]
[Route("api/v1/[controller]")]
public class UsersController : BaseApiController
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthService _authService;
    private readonly IPasswordHasher _passwordHasher;

    public UsersController(
        IApplicationDbContext context,
        IAuthService authService,
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _authService = authService;
        _passwordHasher = passwordHasher;
    }

    /// <summary>
    /// Lista todos los usuarios internos con su departamento, rol y estado.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<UserDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers([FromQuery] int? departamentoId, [FromQuery] int? rolId, CancellationToken ct)
    {
        var query = _context.UsuariosInternos
            .AsNoTracking()
            .Include(u => u.Departamento)
            .Include(u => u.Rol)
                .ThenInclude(r => r.RolesPermisos)
                    .ThenInclude(rp => rp.Permiso)
            .AsQueryable();

        if (departamentoId.HasValue)
            query = query.Where(u => u.DepartamentoId == departamentoId.Value);

        if (rolId.HasValue)
            query = query.Where(u => u.RolId == rolId.Value);

        var list = await query
            .OrderBy(u => u.Nombre)
            .ThenBy(u => u.Apellido)
            .ToListAsync(ct);

        var dtos = list.Select(u => new UserDto
        {
            Id = u.Id,
            Email = u.Email,
            Nombre = u.Nombre,
            Apellido = u.Apellido,
            Telefono = u.Telefono,
            DepartamentoId = u.DepartamentoId,
            DepartamentoCodigo = u.Departamento?.Codigo ?? string.Empty,
            DepartamentoNombre = u.Departamento?.Nombre ?? string.Empty,
            RolId = u.RolId,
            RolCodigo = u.Rol?.Codigo ?? string.Empty,
            RolNombre = u.Rol?.Nombre ?? string.Empty,
            CognitoSub = u.CognitoSub,
            IsActive = u.IsActive,
            UltimoLogin = u.UltimoLogin,
            Permisos = u.Rol?.RolesPermisos
                .Select(rp => rp.Permiso.Codigo)
                .Distinct()
                .ToList() ?? new List<string>()
        }).ToList();

        return Ok(ApiResponse<List<UserDto>>.Ok(dtos, "Usuarios internos recuperados exitosamente"));
    }

    /// <summary>
    /// Registra un nuevo usuario interno en el sistema con su rol y departamento (Solo Administradores).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserInternalRequest request, CancellationToken ct)
    {
        if (!User.IsInRole("ADMIN"))
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail("Acceso denegado: Se requiere rol de Administrador (ADMIN) para registrar colaboradores."));

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var emailExists = await _context.UsuariosInternos.AnyAsync(u => u.Email == normalizedEmail, ct);
        if (emailExists)
            return Conflict(ApiResponse<object>.Fail($"El correo corporativo '{request.Email}' ya está registrado."));

        var depto = await _context.Departamentos.FirstOrDefaultAsync(d => d.Id == request.DepartamentoId && d.IsActive, ct);
        if (depto == null)
            return BadRequest(ApiResponse<object>.Fail($"El departamento especificado (ID: {request.DepartamentoId}) no existe o está inactivo."));

        var rol = await _context.Roles.FirstOrDefaultAsync(r => r.Id == request.RolId && r.IsActive, ct);
        if (rol == null)
            return BadRequest(ApiResponse<object>.Fail($"El rol especificado (ID: {request.RolId}) no existe o está inactivo."));

        var tempPassword = string.IsNullOrWhiteSpace(request.Password) ? "TuNegocio2026!" : request.Password;
        var passwordHash = _passwordHasher.HashPassword(tempPassword);

        var nuevo = new UsuarioInterno
        {
            Email = normalizedEmail,
            PasswordHash = passwordHash,
            Nombre = request.Nombre.Trim(),
            Apellido = request.Apellido.Trim(),
            Telefono = request.Telefono?.Trim(),
            DepartamentoId = request.DepartamentoId,
            RolId = request.RolId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.UsuariosInternos.Add(nuevo);
        await _context.SaveChangesAsync(ct);

        var created = await _context.UsuariosInternos
            .Include(u => u.Departamento)
            .Include(u => u.Rol)
                .ThenInclude(r => r.RolesPermisos)
                    .ThenInclude(rp => rp.Permiso)
            .FirstAsync(u => u.Id == nuevo.Id, ct);

        var dto = new UserDto
        {
            Id = created.Id,
            Email = created.Email,
            Nombre = created.Nombre,
            Apellido = created.Apellido,
            Telefono = created.Telefono,
            DepartamentoId = created.DepartamentoId,
            DepartamentoCodigo = created.Departamento?.Codigo ?? string.Empty,
            DepartamentoNombre = created.Departamento?.Nombre ?? string.Empty,
            RolId = created.RolId,
            RolCodigo = created.Rol?.Codigo ?? string.Empty,
            RolNombre = created.Rol?.Nombre ?? string.Empty,
            CognitoSub = created.CognitoSub,
            IsActive = created.IsActive,
            UltimoLogin = created.UltimoLogin,
            Permisos = created.Rol?.RolesPermisos.Select(rp => rp.Permiso.Codigo).Distinct().ToList() ?? new List<string>()
        };

        return StatusCode(StatusCodes.Status201Created, ApiResponse<UserDto>.Ok(dto, "Usuario interno creado con éxito"));
    }

    /// <summary>
    /// Activa o desactiva la cuenta de un usuario interno (Solo Administradores).
    /// </summary>
    [HttpPut("{id:int}/status")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleUserStatus(int id, [FromBody] ToggleStatusRequest request, CancellationToken ct)
    {
        if (!User.IsInRole("ADMIN"))
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail("Acceso denegado: Se requiere rol de Administrador (ADMIN) para modificar el estado de colaboradores."));

        var user = await _context.UsuariosInternos.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user == null)
            return NotFound(ApiResponse<object>.Fail($"Usuario con ID {id} no encontrado"));

        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        return Ok(ApiResponse<bool>.Ok(user.IsActive, $"Usuario {(user.IsActive ? "activado" : "desactivado")} exitosamente"));
    }

    /// <summary>
    /// Lista los departamentos de la organización con conteo de usuarios asignados.
    /// </summary>
    [HttpGet("departments")]
    [ProducesResponseType(typeof(ApiResponse<List<DepartmentSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDepartments(CancellationToken ct)
    {
        var deptos = await _context.Departamentos
            .AsNoTracking()
            .Include(d => d.Usuarios)
            .OrderBy(d => d.Nombre)
            .ToListAsync(ct);

        var result = deptos.Select(d => new DepartmentSummaryDto
        {
            Id = d.Id,
            Codigo = d.Codigo,
            Nombre = d.Nombre,
            Descripcion = d.Descripcion,
            IsActive = d.IsActive,
            TotalUsuarios = d.Usuarios.Count(u => u.IsActive),
            CreatedAt = d.CreatedAt
        }).ToList();

        return Ok(ApiResponse<List<DepartmentSummaryDto>>.Ok(result, "Departamentos obtenidos exitosamente"));
    }

    /// <summary>
    /// Registra un nuevo departamento corporativo (Solo Administradores).
    /// </summary>
    [HttpPost("departments")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(typeof(ApiResponse<DepartmentSummaryDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentRequest request, CancellationToken ct)
    {
        if (!User.IsInRole("ADMIN"))
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail("Acceso denegado: Se requiere rol de Administrador (ADMIN) para crear departamentos."));

        var codigoUpper = request.Codigo.Trim().ToUpperInvariant();
        var exists = await _context.Departamentos.AnyAsync(d => d.Codigo == codigoUpper, ct);
        if (exists)
            return BadRequest(ApiResponse<object>.Fail($"Ya existe un departamento con el código '{codigoUpper}'."));

        var depto = new Departamento
        {
            Codigo = codigoUpper,
            Nombre = request.Nombre.Trim(),
            Descripcion = request.Descripcion?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Departamentos.Add(depto);
        await _context.SaveChangesAsync(ct);

        var dto = new DepartmentSummaryDto
        {
            Id = depto.Id,
            Codigo = depto.Codigo,
            Nombre = depto.Nombre,
            Descripcion = depto.Descripcion,
            IsActive = depto.IsActive,
            TotalUsuarios = 0,
            CreatedAt = depto.CreatedAt
        };

        return StatusCode(StatusCodes.Status201Created, ApiResponse<DepartmentSummaryDto>.Ok(dto, "Departamento creado exitosamente"));
    }

    /// <summary>
    /// Lista los roles y sus permisos asociados (Matriz RBAC).
    /// </summary>
    [HttpGet("roles")]
    [ProducesResponseType(typeof(ApiResponse<List<RoleWithPermissionsDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoles(CancellationToken ct)
    {
        var roles = await _context.Roles
            .AsNoTracking()
            .Include(r => r.RolesPermisos)
                .ThenInclude(rp => rp.Permiso)
            .Include(r => r.Usuarios)
            .OrderBy(r => r.Nombre)
            .ToListAsync(ct);

        var result = roles.Select(r => new RoleWithPermissionsDto
        {
            Id = r.Id,
            Codigo = r.Codigo,
            Nombre = r.Nombre,
            Descripcion = r.Descripcion,
            IsActive = r.IsActive,
            TotalUsuarios = r.Usuarios.Count(u => u.IsActive),
            Permisos = r.RolesPermisos.Select(rp => new PermissionItemDto
            {
                Id = rp.Permiso.Id,
                Codigo = rp.Permiso.Codigo,
                Modulo = rp.Permiso.Modulo,
                Descripcion = rp.Permiso.Descripcion
            }).ToList()
        }).ToList();

        return Ok(ApiResponse<List<RoleWithPermissionsDto>>.Ok(result, "Roles y permisos obtenidos exitosamente"));
    }
}

public class CreateUserInternalRequest
{
    public string Email { get; set; } = string.Empty;
    public string? Password { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public int DepartamentoId { get; set; }
    public int RolId { get; set; }
}

public class ToggleStatusRequest
{
    public bool IsActive { get; set; }
}

public class CreateDepartmentRequest
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}

public class DepartmentSummaryDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool IsActive { get; set; }
    public int TotalUsuarios { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class RoleWithPermissionsDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool IsActive { get; set; }
    public int TotalUsuarios { get; set; }
    public List<PermissionItemDto> Permisos { get; set; } = new();
}

public class PermissionItemDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Modulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
}
