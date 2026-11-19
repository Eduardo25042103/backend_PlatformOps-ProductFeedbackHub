using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpsHub.Api.Middlewares;
using OpsHub.Api.Services;
using OpsHub.Application;
using OpsHub.Application.Common.Interfaces;
using OpsHub.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// 1. Inyección de dependencias Clean Architecture
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// 2. Servicios de contexto de usuario actual
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// 3. Autenticación y Autorización con JWT Bearer
var jwtSecretKey = builder.Configuration["JwtSettings:SecretKey"]
    ?? "OpsHub_Dev_Secret_Key_For_Local_Testing_Min_32_Characters!";
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "OpsHubApi";
var jwtAudience = builder.Configuration["JwtSettings:Audience"] ?? "OpsHubClients";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// 4. Soporte nativo para AWS Lambda con API Gateway (HTTP API v2 o REST)
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

// 4.1 Configuración de CORS para Frontend Web Angular y Mobile
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 5. Controladores y Swagger UI con soporte para JWT Bearer (Botón Authorize)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PlatformOps & Product Feedback Hub API",
        Version = "v1",
        Description = "API REST de PlatformOps Hub (.NET 10) para TuNegocio.pe - Desplegado en AWS Lambda tras API Gateway.",
        Contact = new OpenApiContact
        {
            Name = "Equipo OpsHub - IDAT VI Ciclo"
        }
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Autenticación JWT usando el esquema Bearer.\r\n\r\nIngrese 'Bearer' [espacio] y luego su token.\r\nEjemplo: 'Bearer eyJhbGciOi...'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

var app = builder.Build();

// 6. Middleware global de excepciones
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Habilitar Swagger en desarrollo o mediante configuración
if (app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("EnableSwaggerInProd"))
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("v1/swagger.json", "OpsHub API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseCors("AllowAll");
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
