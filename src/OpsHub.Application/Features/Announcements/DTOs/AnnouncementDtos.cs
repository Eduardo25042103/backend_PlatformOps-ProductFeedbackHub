namespace OpsHub.Application.Features.Announcements.DTOs;

public record TipoComunicadoDto(
    int Id,
    string Codigo,
    string Nombre,
    string ColorHex
);

public record DestinatarioDto(
    int Id,
    int? DepartamentoId,
    string? DepartamentoNombre,
    int? ClienteId,
    string? ClienteRazonSocial
);

public record ComunicadoDto(
    int Id,
    int TipoId,
    string TipoCodigo,
    string TipoNombre,
    string ColorHex,
    string Titulo,
    string Mensaje,
    bool AfectaServicio,
    int PublicadoPorId,
    string PublicadoPorNombre,
    DateTime? InicioEvento,
    DateTime? FinEstimado,
    DateTime CreatedAt,
    List<DestinatarioDto> Destinatarios
);

public record CreateComunicadoRequest(
    int TipoId,
    string Titulo,
    string Mensaje,
    bool AfectaServicio,
    DateTime? InicioEvento,
    DateTime? FinEstimado,
    List<int>? DepartamentoIds,
    List<int>? ClienteIds
);

public record ChangelogDto(
    int Id,
    string VersionTag,
    string Titulo,
    string Resumen,
    int RedactadoPorId,
    string RedactadoPorNombre,
    DateOnly FechaLanzamiento,
    DateTime CreatedAt
);

public record CreateChangelogRequest(
    string VersionTag,
    string Titulo,
    string Resumen,
    DateOnly FechaLanzamiento
);
