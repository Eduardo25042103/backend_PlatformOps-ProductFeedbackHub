using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OpsHub.Application.Common.Exceptions;
using OpsHub.Application.Common.Interfaces;
using OpsHub.Application.Features.Auth.DTOs;
using OpsHub.Domain.Entities.AccessControl;
using ValidationException = OpsHub.Application.Common.Exceptions.ValidationException;

namespace OpsHub.Application.Features.Auth.Services;

public class AuthService : IAuthService
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ICognitoAuthService _cognitoAuthService;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IValidator<RegisterRequest> _registerValidator;

    public AuthService(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        ICognitoAuthService cognitoAuthService,
        IValidator<LoginRequest> loginValidator,
        IValidator<RegisterRequest> registerValidator)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _cognitoAuthService = cognitoAuthService;
        _loginValidator = loginValidator;
        _registerValidator = registerValidator;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _loginValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var usuario = await _context.UsuariosInternos
            .Include(u => u.Departamento)
            .Include(u => u.Rol)
                .ThenInclude(r => r.RolesPermisos)
                    .ThenInclude(rp => rp.Permiso)
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (usuario == null || !usuario.IsActive)
            throw new UnauthorizedAccessException("Credenciales inválidas o cuenta de usuario desactivada.");

        // Validar credenciales contra AWS Cognito si el usuario está federado/vinculado
        var isCognitoValid = false;
        if (!string.IsNullOrEmpty(usuario.CognitoSub))
        {
            isCognitoValid = await _cognitoAuthService.ValidateCredentialsAsync(request.Email, request.Password, cancellationToken);
        }

        // Si Cognito validó exitosamente o si pasa la verificación local con BCrypt
        var isLocalValid = !string.IsNullOrEmpty(usuario.PasswordHash) && _passwordHasher.VerifyPassword(request.Password, usuario.PasswordHash);

        if (!isCognitoValid && !isLocalValid)
            throw new UnauthorizedAccessException("Credenciales inválidas.");

        usuario.UltimoLogin = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        var token = _jwtTokenGenerator.GenerateToken(usuario);

        return new AuthResponseDto
        {
            Token = token,
            User = MapToUserDto(usuario)
        };
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _registerValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var emailExists = await _context.UsuariosInternos
            .AnyAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (emailExists)
            throw new ConflictException($"El correo '{request.Email}' ya se encuentra registrado.");

        var depto = await _context.Departamentos
            .FirstOrDefaultAsync(d => d.Id == request.DepartamentoId && d.IsActive, cancellationToken);

        if (depto == null)
            throw new NotFoundException("Departamento", request.DepartamentoId);

        int finalRolId;
        if (request.RolId.HasValue && request.RolId.Value > 0)
        {
            var rolExists = await _context.Roles
                .AnyAsync(r => r.Id == request.RolId.Value && r.IsActive, cancellationToken);

            if (!rolExists)
                throw new NotFoundException("Rol", request.RolId.Value);

            finalRolId = request.RolId.Value;
        }
        else
        {
            // Asignación inteligente del rol base según departamento
            var rolEncontrado = depto.Codigo switch
            {
                "TI" => await _context.Roles.FirstOrDefaultAsync(r => r.Codigo == "DEV_INGENIERO" && r.IsActive, cancellationToken),
                "VENTAS" => await _context.Roles.FirstOrDefaultAsync(r => r.Codigo == "EJECUTIVO_VENTAS" && r.IsActive, cancellationToken),
                "MKT" => await _context.Roles.FirstOrDefaultAsync(r => r.Codigo == "MARKETING_LEAD" && r.IsActive, cancellationToken),
                _ => await _context.Roles.FirstOrDefaultAsync(r => r.Codigo == "AGENTE_SOPORTE" && r.IsActive, cancellationToken)
            };

            finalRolId = rolEncontrado?.Id ?? 2; // Por defecto Agente de Soporte (Id 2)
        }

        // 1. Registrar usuario en AWS Cognito User Pool
        var cognitoSub = await _cognitoAuthService.RegisterUserAsync(
            request.Email,
            request.Password,
            request.Nombre,
            request.Apellido,
            cancellationToken);

        try
        {
            // 2. Hash local defensivo con BCrypt
            var passwordHash = _passwordHasher.HashPassword(request.Password);

            // 3. Crear registro en MySQL vinculado al cognito_sub
            var nuevoUsuario = new UsuarioInterno
            {
                CognitoSub = cognitoSub,
                Email = normalizedEmail,
                PasswordHash = passwordHash,
                Nombre = request.Nombre.Trim(),
                Apellido = request.Apellido.Trim(),
                Telefono = request.Telefono?.Trim(),
                DepartamentoId = request.DepartamentoId,
                RolId = finalRolId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.UsuariosInternos.Add(nuevoUsuario);
            await _context.SaveChangesAsync(cancellationToken);

            // Recargar con relaciones
            var usuarioCargado = await _context.UsuariosInternos
                .Include(u => u.Departamento)
                .Include(u => u.Rol)
                    .ThenInclude(r => r.RolesPermisos)
                        .ThenInclude(rp => rp.Permiso)
                .FirstAsync(u => u.Id == nuevoUsuario.Id, cancellationToken);

            var token = _jwtTokenGenerator.GenerateToken(usuarioCargado);

            return new AuthResponseDto
            {
                Token = token,
                User = MapToUserDto(usuarioCargado)
            };
        }
        catch (Exception)
        {
            // Transacción Compensatoria (Saga Pattern):
            // Si la base de datos falla al guardar, revertir la creación en Cognito
            // para evitar dejar un usuario huérfano.
            await _cognitoAuthService.DeleteUserAsync(request.Email, CancellationToken.None);
            throw;
        }
    }

    public async Task<UserDto> GetCurrentUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var usuario = await _context.UsuariosInternos
            .Include(u => u.Departamento)
            .Include(u => u.Rol)
                .ThenInclude(r => r.RolesPermisos)
                    .ThenInclude(rp => rp.Permiso)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (usuario == null)
            throw new NotFoundException("Usuario", userId);

        return MapToUserDto(usuario);
    }

    private static UserDto MapToUserDto(UsuarioInterno u)
    {
        return new UserDto
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
        };
    }
}
