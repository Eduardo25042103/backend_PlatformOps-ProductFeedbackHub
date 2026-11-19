using System.Security.Claims;
using OpsHub.Application.Common.Interfaces;

namespace OpsHub.Api.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? UserId
    {
        get
        {
            var idClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out var id) ? id : null;
        }
    }

    public string? Email =>
        _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;

    public string? RolCodigo =>
        _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;

    public string? DepartamentoCodigo =>
        _httpContextAccessor.HttpContext?.User?.FindFirst("departamento")?.Value;

    public string? IpAddress
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            var forwarded = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(forwarded))
            {
                return forwarded.Split(',')[0].Trim();
            }

            return context.Connection.RemoteIpAddress?.ToString();
        }
    }

    public string? Platform
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return "WEB_API";

            var customHeader = context.Request.Headers["X-Client-Platform"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(customHeader))
            {
                return customHeader;
            }

            var userAgent = context.Request.Headers["User-Agent"].ToString().ToLowerInvariant();
            if (userAgent.Contains("android")) return "REACT_NATIVE_ANDROID";
            if (userAgent.Contains("iphone") || userAgent.Contains("ipad")) return "REACT_NATIVE_IOS";
            if (userAgent.Contains("electron") || userAgent.Contains("windows") || userAgent.Contains("macintosh")) return "ANGULAR_DESKTOP";

            return "WEB_API";
        }
    }
}
