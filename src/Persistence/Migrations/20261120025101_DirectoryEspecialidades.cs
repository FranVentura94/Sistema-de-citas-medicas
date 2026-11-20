using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DirectoryEspecialidades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Atenciones",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Citas",
                schema: "dbo");

            migrationBuilder.CreateTable(
                name: "Especialidades",
                schema: "dbo",
                columns: table => new
                {
                    EspecialidadID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Especialidades", x => x.EspecialidadID);
                });

            migrationBuilder.CreateTable(
                name: "MedicoEspecialidades",
                schema: "dbo",
                columns: table => new
                {
                    MedicoID = table.Column<int>(type: "int", nullable: false),
                    EspecialidadID = table.Column<int>(type: "int", nullable: false),
                    Principal = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicoEspecialidades", x => new { x.MedicoID, x.EspecialidadID });
                    table.ForeignKey(
                        name: "FK_MedicoEspecialidades_Especialidades_EspecialidadID",
                        column: x => x.EspecialidadID,
                        principalSchema: "dbo",
                        principalTable: "Especialidades",
                        principalColumn: "EspecialidadID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MedicoEspecialidades_Medicos_MedicoID",
                        column: x => x.MedicoID,
                        principalSchema: "dbo",
                        principalTable: "Medicos",
                        principalColumn: "MedicoID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "Especialidades",
                columns: new[] { "EspecialidadID", "Activo", "Codigo", "Descripcion", "Nombre" },
                values: new object[,]
                {
                    { 1, true, "MG", "Atención primaria y consulta general", "Medicina General" },
                    { 2, true, "PED", "Atención de niños y adolescentes", "Pediatría" },
                    { 3, true, "CAR", "Enfermedades del corazón", "Cardiología" },
                    { 4, true, "DER", "Enfermedades de la piel", "Dermatología" },
                    { 5, true, "GIN", "Salud de la mujer", "Ginecología" },
                    { 6, true, "TRA", "Lesiones de huesos y articulaciones", "Traumatología" }
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "Medicos",
                columns: new[] { "MedicoID", "Activo", "Apellidos", "Email", "FechaIngreso", "Identification", "Nombres", "NumeroColegiatura", "Telefono", "UsuarioID" },
                values: new object[,]
                {
                    { 1, true, "Rivas", "medico@clinica.test", new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "04512378-9", "Carlos", "JVPM-10234", "7012-3456", 3 },
                    { 2, true, "López", "andrea.lopez@clinica.test", new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "03456789-1", "Andrea", "JVPM-11872", "7123-9876", null },
                    { 3, true, "Méndez", "roberto.mendez@clinica.test", new DateTime(2026, 3, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "02345678-5", "Roberto", "JVPM-09561", "7234-5678", null }
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "MedicoEspecialidades",
                columns: new[] { "EspecialidadID", "MedicoID", "Principal" },
                values: new object[,]
                {
                    { 1, 1, true },
                    { 2, 2, true },
                    { 1, 3, false },
                    { 3, 3, true }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Especialidades_Codigo",
                schema: "dbo",
                table: "Especialidades",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedicoEspecialidades_EspecialidadID",
                schema: "dbo",
                table: "MedicoEspecialidades",
                column: "EspecialidadID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MedicoEspecialidades",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Especialidades",
                schema: "dbo");

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Medicos",
                keyColumn: "MedicoID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Medicos",
                keyColumn: "MedicoID",
                keyValue: 2);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Medicos",
                keyColumn: "MedicoID",
                keyValue: 3);

            migrationBuilder.CreateTable(
                name: "Citas",
                schema: "dbo",
                columns: table => new
                {
                    CitaID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MedicoID = table.Column<int>(type: "int", nullable: false),
                    PacienteID = table.Column<long>(type: "bigint", nullable: false),
                    EspecialidadID = table.Column<int>(type: "int", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaHoraFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaHoraInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MotivoConsulta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NotasCancelacion = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                    CitaID = table.Column<long>(type: "bigint", nullable: true),
                    MedicoID = table.Column<int>(type: "int", nullable: false),
                    PacienteID = table.Column<long>(type: "bigint", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MotivoAtencion = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
    }
}
