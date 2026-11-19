# ==============================================================================
# Dockerfile Multi-Etapa: OpsHub Backend (.NET 10)
# Compatible con AWS Lambda Container Images, AWS App Runner y Amazon ECS Fargate
# ==============================================================================

# Etapa 1: Compilación y Publicación
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar archivos de proyecto para restaurar dependencias aprovechando la caché
COPY ["src/OpsHub.Domain/OpsHub.Domain.csproj", "src/OpsHub.Domain/"]
COPY ["src/OpsHub.Application/OpsHub.Application.csproj", "src/OpsHub.Application/"]
COPY ["src/OpsHub.Infrastructure/OpsHub.Infrastructure.csproj", "src/OpsHub.Infrastructure/"]
COPY ["src/OpsHub.Api/OpsHub.Api.csproj", "src/OpsHub.Api/"]

RUN dotnet restore "src/OpsHub.Api/OpsHub.Api.csproj"

# Copiar todo el código fuente
COPY . .

# Compilar en modo Release
WORKDIR "/src/src/OpsHub.Api"
RUN dotnet publish "OpsHub.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa 2: Imagen Final de Ejecución
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Variables de entorno por defecto
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OpsHub.Api.dll"]
