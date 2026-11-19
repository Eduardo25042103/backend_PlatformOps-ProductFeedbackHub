using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using OpsHub.Application.Common.Interfaces;
using OpsHub.Domain.Entities.OperationsAndAudit;

namespace OpsHub.Infrastructure.Persistence.Interceptors;

public class AuditLogsInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;
    private List<PendingAudit>? _pendingAudits;

    public AuditLogsInterceptor(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context == null) return base.SavingChangesAsync(eventData, result, cancellationToken);

        _pendingAudits = CollectAudits(context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context != null && _pendingAudits != null && _pendingAudits.Count > 0)
        {
            var auditsToSave = _pendingAudits;
            _pendingAudits = null; // Evitar reentrada

            var logEntities = new List<LogActividad>();

            foreach (var audit in auditsToSave)
            {
                var registroId = audit.RegistroId;
                if (audit.IsAdded && audit.Entry != null)
                {
                    var pkProp = audit.Entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
                    if (pkProp?.CurrentValue is int idInt)
                    {
                        registroId = idInt;
                    }
                }

                logEntities.Add(new LogActividad
                {
                    UsuarioId = audit.UsuarioId,
                    Accion = audit.Accion,
                    TablaAfectada = audit.TablaAfectada,
                    RegistroId = registroId,
                    IpOrigen = audit.IpOrigen,
                    Plataforma = audit.Plataforma,
                    ValoresAnterioresJson = audit.ValoresAnteriores.Count > 0
                        ? JsonSerializer.Serialize(audit.ValoresAnteriores)
                        : null,
                    ValoresNuevosJson = audit.ValoresNuevos.Count > 0
                        ? JsonSerializer.Serialize(audit.ValoresNuevos)
                        : null,
                    CreatedAt = DateTime.UtcNow
                });
            }

            if (logEntities.Count > 0)
            {
                context.Set<LogActividad>().AddRange(logEntities);
                await context.SaveChangesAsync(cancellationToken);
            }
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private List<PendingAudit> CollectAudits(DbContext context)
    {
        var audits = new List<PendingAudit>();
        var entries = context.ChangeTracker.Entries();

        foreach (var entry in entries)
        {
            // Omitir entidades de auditoría y no modificadas
            if (entry.Entity is LogActividad || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
            {
                continue;
            }

            var tableName = entry.Metadata.GetTableName() ?? entry.Metadata.ClrType.Name;
            var accion = entry.State switch
            {
                EntityState.Added => "CREATE",
                EntityState.Modified => "UPDATE",
                EntityState.Deleted => "DELETE",
                _ => "UNKNOWN"
            };

            var audit = new PendingAudit
            {
                Entry = entry,
                IsAdded = entry.State == EntityState.Added,
                Accion = accion,
                TablaAfectada = tableName,
                UsuarioId = _currentUserService.UserId,
                IpOrigen = _currentUserService.IpAddress,
                Plataforma = _currentUserService.Platform
            };

            var pkProp = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
            if (pkProp?.CurrentValue is int idVal)
            {
                audit.RegistroId = idVal;
            }

            foreach (var prop in entry.Properties)
            {
                var propName = prop.Metadata.Name;
                var isSensitive = propName.Contains("Password", StringComparison.OrdinalIgnoreCase) ||
                                  propName.Contains("Secret", StringComparison.OrdinalIgnoreCase) ||
                                  propName.Contains("Token", StringComparison.OrdinalIgnoreCase);

                if (entry.State == EntityState.Added)
                {
                    if (!prop.Metadata.IsPrimaryKey())
                    {
                        audit.ValoresNuevos[propName] = isSensitive ? "***" : prop.CurrentValue;
                    }
                }
                else if (entry.State == EntityState.Deleted)
                {
                    audit.ValoresAnteriores[propName] = isSensitive ? "***" : prop.OriginalValue;
                }
                else if (entry.State == EntityState.Modified && prop.IsModified)
                {
                    audit.ValoresAnteriores[propName] = isSensitive ? "***" : prop.OriginalValue;
                    audit.ValoresNuevos[propName] = isSensitive ? "***" : prop.CurrentValue;
                }
            }

            audits.Add(audit);
        }

        return audits;
    }

    private sealed class PendingAudit
    {
        public EntityEntry? Entry { get; set; }
        public bool IsAdded { get; set; }
        public string Accion { get; set; } = string.Empty;
        public string TablaAfectada { get; set; } = string.Empty;
        public int RegistroId { get; set; }
        public int? UsuarioId { get; set; }
        public string? IpOrigen { get; set; }
        public string? Plataforma { get; set; }
        public Dictionary<string, object?> ValoresAnteriores { get; } = new();
        public Dictionary<string, object?> ValoresNuevos { get; } = new();
    }
}
