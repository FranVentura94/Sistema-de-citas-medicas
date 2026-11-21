namespace Core.Features.Empleados.Dtos;

// Lo que llena el Administrador en la ficha (todo opcional menos el nombre)
public record EmpleadoDatos(
    string Nombres,
    string? Apellidos,
    string? Identificacion,
    DateTime? FechaNacimiento,
    string? Genero,
    string? Telefono,
    string? Email,
    string? Direccion,
    string? Cargo,
    DateTime? FechaIngreso,
    string? Notas,
    string? EmergenciaNombre,
    string? EmergenciaParentesco,
    string? EmergenciaTelefono,
    bool Activo,
    // Solo si el cargo es Médico
    string? NumeroColegiatura,
    List<int>? EspecialidadIds,
    int? EspecialidadPrincipalId,
    // Solo si el cargo es Enfermería
    List<int>? MedicosAsignados);

public record MedicoAsignadoDto(int MedicoID, string Nombre);

public record EmpleadoDto(
    int EmpleadoID,
    string Nombres,
    string? Apellidos,
    string NombreCompleto,
    string? Identificacion,
    DateTime? FechaNacimiento,
    string? Genero,
    string? Telefono,
    string? Email,
    string? Direccion,
    string Cargo,
    DateTime? FechaIngreso,
    string? Notas,
    string? EmergenciaNombre,
    string? EmergenciaParentesco,
    string? EmergenciaTelefono,
    bool Activo,
    DateTime FechaRegistro,
    int? MedicoID,
    int? UsuarioID,
    string? NumeroColegiatura,
    List<int> EspecialidadIds,
    int? EspecialidadPrincipalId,
    List<MedicoAsignadoDto> MedicosAsignados);

// Datos para saludar a la persona que inició sesión
public record MiPerfilDto(
    int EmpleadoID,
    string Nombres,
    string? Apellidos,
    string? Genero,
    string Cargo,
    DateTime? FechaNacimiento,
    DateTime? FechaIngreso,
    List<MedicoAsignadoDto> MedicosAsignados);

public record AsignarUsuarioRequest(int? UsuarioID);