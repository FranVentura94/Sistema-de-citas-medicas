namespace Core.Features.Medicos.Dtos;

public record EspecialidadDto(int EspecialidadID, string Codigo, string Nombre, string? Descripcion);

public record MedicoEspecialidadDto(int EspecialidadID, string Nombre, bool Principal);

public record MedicoDto(
    int MedicoID,
    int? UsuarioID,
    string Nombres,
    string Apellidos,
    string NombreCompleto,
    string NumeroColegiatura,
    string Identification,
    string Telefono,
    string Email,
    bool Activo,
    List<MedicoEspecialidadDto> Especialidades);