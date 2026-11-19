using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using OpsHub.Application.Features.Announcements.Services;
using OpsHub.Application.Features.Auth.Services;
using OpsHub.Application.Features.Incidents.Services;
using OpsHub.Application.Features.Integrations.Zoho.Services;
using OpsHub.Application.Features.Operations.Services;
using OpsHub.Application.Features.ProductFeedback.Services;

namespace OpsHub.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<ISugerenciaService, SugerenciaService>();
        services.AddScoped<IZohoWebhookService, ZohoWebhookService>();
        services.AddScoped<IAnnouncementService, AnnouncementService>();
        services.AddScoped<IOperationsService, OperationsService>();
        return services;
    }
}
