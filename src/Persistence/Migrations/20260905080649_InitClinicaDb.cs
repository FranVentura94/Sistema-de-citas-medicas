using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitClinicaDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "Medicos",
                schema: "dbo",
                columns: table => new
                {
                    MedicoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioID = table.Column<int>(type: "int", nullable: true),
                    Nombres = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Apellidos = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumeroColegiatura = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Identification = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaIngreso = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medicos", x => x.MedicoID);
                });

            migrationBuilder.CreateTable(
                name: "Pacientes",
                schema: "dbo",
                columns: table => new
                {
                    PacienteID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodigoPaciente = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TipoDocumento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumeroDocumento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nombres = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Apellidos = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaNacimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Sexo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EstadoCivil = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TelefonoSecundario = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Direccion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ciudad = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pais = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ocupacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TipoSangre = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pacientes", x => x.PacienteID);
                });

            migrationBuilder.CreateTable(
                name: "Citas",
                schema: "dbo",
                columns: table => new
                {
                    CitaID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PacienteID = table.Column<long>(type: "bigint", nullable: false),
                    MedicoID = table.Column<int>(type: "int", nullable: false),
                    EspecialidadID = table.Column<int>(type: "int", nullable: true),
                    FechaHoraInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaHoraFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MotivoConsulta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NotasCancelacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Citas", x => x.CitaID);
                    table.ForeignKey(
                        name: "FK_Citas_Medicos_MedicoID",
                        column: x => x.MedicoID,
                        principalSchema: "dbo",
                        principalTable: "Medicos",
                        principalColumn: "MedicoID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Citas_Pacientes_PacienteID",
                        column: x => x.PacienteID,
                        principalSchema: "dbo",
                        principalTable: "Pacientes",
                        principalColumn: "PacienteID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Atenciones",
                schema: "dbo",
                columns: table => new
                {
                    AtencionID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PacienteID = table.Column<long>(type: "bigint", nullable: false),
                    MedicoID = table.Column<int>(type: "int", nullable: false),
                    CitaID = table.Column<long>(type: "bigint", nullable: true),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MotivoAtencion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Atenciones", x => x.AtencionID);
                    table.ForeignKey(
                        name: "FK_Atenciones_Citas_CitaID",
                        column: x => x.CitaID,
                        principalSchema: "dbo",
                        principalTable: "Citas",
                        principalColumn: "CitaID");
                    table.ForeignKey(
                        name: "FK_Atenciones_Medicos_MedicoID",
                        column: x => x.MedicoID,
                        principalSchema: "dbo",
                        principalTable: "Medicos",
                        principalColumn: "MedicoID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Atenciones_Pacientes_PacienteID",
                        column: x => x.PacienteID,
                        principalSchema: "dbo",
                        principalTable: "Pacientes",
                        principalColumn: "PacienteID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Atenciones_CitaID",
                schema: "dbo",
                table: "Atenciones",
                column: "CitaID");

            migrationBuilder.CreateIndex(
                name: "IX_Atenciones_MedicoID",
                schema: "dbo",
                table: "Atenciones",
                column: "MedicoID");

            migrationBuilder.CreateIndex(
                name: "IX_Atenciones_PacienteID",
                schema: "dbo",
                table: "Atenciones",
                column: "PacienteID");

            migrationBuilder.CreateIndex(
                name: "IX_Citas_MedicoID",
                schema: "dbo",
                table: "Citas",
                column: "MedicoID");

            migrationBuilder.CreateIndex(
                name: "IX_Citas_PacienteID",
                schema: "dbo",
                table: "Citas",
                column: "PacienteID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Atenciones",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Citas",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Medicos",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Pacientes",
                schema: "dbo");
        }
    }
}
