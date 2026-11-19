using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpsHub.Application.Common.Interfaces;
using OpsHub.Infrastructure.Persistence.Context;
using OpsHub.Infrastructure.Persistence.Interceptors;
using OpsHub.Infrastructure.Services;

namespace OpsHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddScoped<AuditLogsInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            var auditInterceptor = sp.GetRequiredService<AuditLogsInterceptor>();
            options.AddInterceptors(auditInterceptor);

            if (!string.IsNullOrEmpty(connectionString))
            {
                options.UseMySql(
                    connectionString,
                    new MySqlServerVersion(new Version(8, 4, 0)),
                    mySqlOptions =>
                    {
                        mySqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(5),
                            errorNumbersToAdd: null);
                    });
            }
        });

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        // Servicios de seguridad y autenticación
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<ICognitoAuthService, CognitoAuthService>();
        services.AddScoped<IS3StorageService, S3StorageService>();
        services.AddScoped<IZohoSignatureValidator, ZohoSignatureValidator>();

        return services;
    }
}
