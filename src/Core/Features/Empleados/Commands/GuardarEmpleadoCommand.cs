using System.Text.RegularExpressions;
using Core.Exceptions;
using Core.Features.Empleados.Dtos;
using Core.Features.Empleados.Interfaces;
using Domain.Entities;
using MediatR;

namespace Core.Features.Empleados.Commands;

// Crea (EmpleadoID = null) o actualiza la ficha de un empleado. Solo Administrador.
public record GuardarEmpleadoCommand(int? EmpleadoID, EmpleadoDatos Datos) : IRequest<EmpleadoDto>;

public class GuardarEmpleadoCommandHandler : IRequestHandler<GuardarEmpleadoCommand, EmpleadoDto>
{
    private readonly IEmpleadoRepository _repository;

    public GuardarEmpleadoCommandHandler(IEmpleadoRepository repository)
    {
        _repository = repository;
    }

    public async Task<EmpleadoDto> Handle(GuardarEmpleadoCommand request, CancellationToken ct)
    {
        var d = request.Datos;

        // ---------- Validaciones ----------
        if (string.IsNullOrWhiteSpace(d.Nombres))
            throw new DomainException(Errores.DATOS_INVALIDOS, "Escribe al menos el nombre de la persona.");

        var email = Limpiar(d.Email);
        if (email is not null && !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new DomainException(Errores.DATOS_INVALIDOS, "El correo electrónico no es válido.");

        var genero = Limpiar(d.Genero)?.ToUpperInvariant();
        if (genero is not null && genero is not ("F" or "M"))
            throw new DomainException(Errores.DATOS_INVALIDOS, "El género debe ser F o M.");

        var cargo = Limpiar(d.Cargo) ?? Cargos.Otro;
        if (!Cargos.Todos.Contains(cargo))
            throw new DomainException(Errores.DATOS_INVALIDOS, $"Cargo no válido. Opciones: {string.Join(", ", Cargos.Todos)}.");

        var identificacion = Limpiar(d.Identificacion);
        if (identificacion is not null && await _repository.ExisteIdentificacionAsync(identificacion, request.EmpleadoID, ct))
            throw new DomainException(Errores.REGISTRO_DUPLICADO, "Ya existe un empleado con esa identificación.");

        var especialidades = (d.EspecialidadIds ?? new List<int>()).Distinct().ToList();
        if (cargo == Cargos.Medico && especialidades.Count == 0)
            throw new DomainException(Errores.DATOS_INVALIDOS, "Un médico necesita al menos una especialidad para poder recibir citas.");

        // ---------- Ficha ----------
        var nuevo = request.EmpleadoID is null;
        var empleado = nuevo
            ? new Empleado { FechaRegistro = DateTime.Now }
            : await _repository.GetByIdAsync(request.EmpleadoID!.Value, ct)
              ?? throw new DomainException(Errores.RECURSO_NO_ENCONTRADO, "El empleado no existe.");

        empleado.Nombres = d.Nombres.Trim();
        empleado.Apellidos = Limpiar(d.Apellidos);
        empleado.Identificacion = identificacion;
        empleado.FechaNacimiento = d.FechaNacimiento?.Date;
        empleado.Genero = genero;
        empleado.Telefono = Limpiar(d.Telefono);
        empleado.Email = email;
        empleado.Direccion = Limpiar(d.Direccion);
        empleado.Cargo = cargo;
        empleado.FechaIngreso = d.FechaIngreso?.Date;
        empleado.Notas = Limpiar(d.Notas);
        empleado.EmergenciaNombre = Limpiar(d.EmergenciaNombre);
        empleado.EmergenciaParentesco = Limpiar(d.EmergenciaParentesco);
        empleado.EmergenciaTelefono = Limpiar(d.EmergenciaTelefono);
        empleado.Activo = d.Activo;

        // ---------- Si es médico: su ficha de médico para la agenda ----------
        if (cargo == Cargos.Medico)
        {
            var medico = empleado.Medico;
            if (medico is null)
            {
                medico = new Medico { FechaIngreso = empleado.FechaIngreso ?? DateTime.Now };
                empleado.Medico = medico;
            }

            medico.Nombres = empleado.Nombres;
            medico.Apellidos = empleado.Apellidos ?? string.Empty;
            medico.NumeroColegiatura = Limpiar(d.NumeroColegiatura) ?? string.Empty;
            medico.Identification = identificacion ?? string.Empty;
            medico.Telefono = empleado.Telefono ?? string.Empty;
            medico.Email = email ?? string.Empty;
            medico.UsuarioID = empleado.UsuarioID;
            medico.Activo = empleado.Activo;

            var principal = d.EspecialidadPrincipalId is int p && especialidades.Contains(p) ? p : especialidades[0];

            foreach (var quitar in medico.Especialidades.Where(e => !especialidades.Contains(e.EspecialidadID)).ToList())
                medico.Especialidades.Remove(quitar);

            foreach (var id in especialidades.Where(id => medico.Especialidades.All(e => e.EspecialidadID != id)))
                medico.Especialidades.Add(new MedicoEspecialidad { EspecialidadID = id });

            foreach (var e in medico.Especialidades)
                e.Principal = e.EspecialidadID == principal;
        }
        else if (empleado.Medico is not null)
        {
            // Dejó de ser médico: ya no recibe citas, pero su historial se conserva
            empleado.Medico.Activo = false;
        }

        // ---------- Si es enfermera(o): médicos a los que asiste ----------
        var asignados = cargo == Cargos.Enfermeria
            ? (d.MedicosAsignados ?? new List<int>()).Distinct().ToList()
            : new List<int>();

        if (asignados.Count > 0)
        {
            var existentes = await _repository.MedicosExistentesAsync(asignados, ct);
            if (existentes.Count != asignados.Count)
                throw new DomainException(Errores.DATOS_INVALIDOS, "Alguno de los médicos asignados no existe.");
        }

        foreach (var quitar in empleado.MedicosAsignados.Where(a => !asignados.Contains(a.MedicoID)).ToList())
            empleado.MedicosAsignados.Remove(quitar);

        foreach (var id in asignados.Where(id => empleado.MedicosAsignados.All(a => a.MedicoID != id)))
            empleado.MedicosAsignados.Add(new EnfermeraMedico { MedicoID = id });

        // ---------- Guardar ----------
        if (nuevo)
            await _repository.AddAsync(empleado, ct);
        else
            await _repository.GuardarCambiosAsync(ct);

        var guardado = await _repository.GetByIdAsync(empleado.EmpleadoID, ct);
        return guardado!.ToDto();
    }

    // "  " -> null ; " Ana " -> "Ana"
    private static string? Limpiar(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}