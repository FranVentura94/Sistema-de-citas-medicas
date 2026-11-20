using Microsoft.EntityFrameworkCore;
using MsIdentity.Core.Entities;
using MsIdentity.Persistence.Context;

namespace MsIdentity.Persistence.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(MsIdentityDbContext db)
    {
        // Si ya hay roles, la base ya fue sembrada
        if (await db.Roles.AnyAsync())
            return;

        // ---------- Permisos ----------
        var permisos = new List<Permiso>
        {
            new() { Codigo = "USUARIOS_GESTIONAR",   Nombre = "Gestionar usuarios",        Modulo = "Identity"  },

            new() { Codigo = "PACIENTES_VER",        Nombre = "Ver pacientes",             Modulo = "Directory" },
            new() { Codigo = "PACIENTES_CREAR",      Nombre = "Registrar pacientes",       Modulo = "Directory" },
            new() { Codigo = "PACIENTES_EDITAR",     Nombre = "Editar pacientes",          Modulo = "Directory" },
            new() { Codigo = "MEDICOS_VER",          Nombre = "Ver médicos",               Modulo = "Directory" },
            new() { Codigo = "MEDICOS_GESTIONAR",    Nombre = "Gestionar médicos",         Modulo = "Directory" },

            new() { Codigo = "HORARIOS_VER",         Nombre = "Ver horarios",              Modulo = "Agenda"    },
            new() { Codigo = "HORARIOS_GESTIONAR",   Nombre = "Gestionar horarios",        Modulo = "Agenda"    },
            new() { Codigo = "CITAS_VER",            Nombre = "Ver citas",                 Modulo = "Agenda"    },
            new() { Codigo = "CITAS_CREAR",          Nombre = "Agendar citas",             Modulo = "Agenda"    },
            new() { Codigo = "CITAS_CAMBIAR_ESTADO", Nombre = "Cambiar estado de citas",   Modulo = "Agenda"    },

            new() { Codigo = "ATENCION_VER",         Nombre = "Ver atenciones",            Modulo = "Clinical"  },
            new() { Codigo = "ATENCION_REGISTRAR",   Nombre = "Registrar atención médica", Modulo = "Clinical"  },
            new() { Codigo = "RECETA_CREAR",         Nombre = "Generar recetas",           Modulo = "Clinical"  }
        };
        db.Permisos.AddRange(permisos);

        Permiso P(string codigo) => permisos.First(p => p.Codigo == codigo);

        // ---------- Roles ----------
        var admin = new Rol { Nombre = "Administrador", Descripcion = "Acceso total al sistema" };
        foreach (var p in permisos)
            admin.RolPermisos.Add(new RolPermiso { Permiso = p });

        var recepcion = new Rol { Nombre = "Recepcion", Descripcion = "Registro de pacientes y agenda de citas" };
        foreach (var codigo in new[] { "PACIENTES_VER", "PACIENTES_CREAR", "PACIENTES_EDITAR", "MEDICOS_VER",
                                       "HORARIOS_VER", "CITAS_VER", "CITAS_CREAR", "CITAS_CAMBIAR_ESTADO" })
            recepcion.RolPermisos.Add(new RolPermiso { Permiso = P(codigo) });

        var medico = new Rol { Nombre = "Medico", Descripcion = "Atención clínica de pacientes" };
        foreach (var codigo in new[] { "PACIENTES_VER", "HORARIOS_VER", "CITAS_VER", "CITAS_CAMBIAR_ESTADO",
                                       "ATENCION_VER", "ATENCION_REGISTRAR", "RECETA_CREAR" })
            medico.RolPermisos.Add(new RolPermiso { Permiso = P(codigo) });

        db.Roles.AddRange(admin, recepcion, medico);

        // ---------- Usuarios de prueba ----------
        db.Usuarios.AddRange(
            new Usuario
            {
                NombreUsuario = "admin",
                NombreCompleto = "Administrador del Sistema",
                Email = "admin@clinica.test",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123*"),
                Rol = admin
            },
            new Usuario
            {
                NombreUsuario = "recepcion",
                NombreCompleto = "Laura Martínez",
                Email = "recepcion@clinica.test",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Recepcion123*"),
                Rol = recepcion
            },
            new Usuario
            {
                NombreUsuario = "medico",
                NombreCompleto = "Dr. Carlos Rivas",
                Email = "medico@clinica.test",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Medico123*"),
                Rol = medico,
                MedicoId = 1 // se enlaza con el médico 1 de MS_Directory
            });

        await db.SaveChangesAsync();
    }
}