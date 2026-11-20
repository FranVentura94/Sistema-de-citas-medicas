using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MsAgenda.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InicialAgenda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "Consultorios",
                schema: "dbo",
                columns: table => new
                {
                    ConsultorioId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Ubicacion = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consultorios", x => x.ConsultorioId);
                });

            migrationBuilder.CreateTable(
                name: "EstadosCita",
                schema: "dbo",
                columns: table => new
                {
                    EstadoCitaId = table.Column<int>(type: "int", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Color = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadosCita", x => x.EstadoCitaId);
                });

            migrationBuilder.CreateTable(
                name: "HorariosMedicos",
                schema: "dbo",
                columns: table => new
                {
                    HorarioMedicoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MedicoId = table.Column<int>(type: "int", nullable: false),
                    MedicoNombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ConsultorioId = table.Column<int>(type: "int", nullable: false),
                    DiaSemana = table.Column<int>(type: "int", nullable: false),
                    HoraInicio = table.Column<TimeOnly>(type: "time", nullable: false),
                    HoraFin = table.Column<TimeOnly>(type: "time", nullable: false),
                    DuracionCitaMinutos = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HorariosMedicos", x => x.HorarioMedicoId);
                    table.ForeignKey(
                        name: "FK_HorariosMedicos_Consultorios_ConsultorioId",
                        column: x => x.ConsultorioId,
                        principalSchema: "dbo",
                        principalTable: "Consultorios",
                        principalColumn: "ConsultorioId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Citas",
                schema: "dbo",
                columns: table => new
                {
                    CitaId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PacienteId = table.Column<long>(type: "bigint", nullable: false),
                    PacienteNombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PacienteDocumento = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    MedicoId = table.Column<int>(type: "int", nullable: false),
                    MedicoNombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    EspecialidadId = table.Column<int>(type: "int", nullable: true),
                    EspecialidadNombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ConsultorioId = table.Column<int>(type: "int", nullable: false),
                    FechaHoraInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaHoraFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EstadoCitaId = table.Column<int>(type: "int", nullable: false),
                    MotivoConsulta = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notas = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UsuarioRegistro = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Citas", x => x.CitaId);
                    table.ForeignKey(
                        name: "FK_Citas_Consultorios_ConsultorioId",
                        column: x => x.ConsultorioId,
                        principalSchema: "dbo",
                        principalTable: "Consultorios",
                        principalColumn: "ConsultorioId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Citas_EstadosCita_EstadoCitaId",
                        column: x => x.EstadoCitaId,
                        principalSchema: "dbo",
                        principalTable: "EstadosCita",
                        principalColumn: "EstadoCitaId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "Consultorios",
                columns: new[] { "ConsultorioId", "Activo", "Codigo", "Nombre", "Ubicacion" },
                values: new object[,]
                {
                    { 1, true, "C-101", "Consultorio 101", "Primer nivel" },
                    { 2, true, "C-102", "Consultorio 102", "Primer nivel" },
                    { 3, true, "C-201", "Consultorio 201", "Segundo nivel" }
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "EstadosCita",
                columns: new[] { "EstadoCitaId", "Codigo", "Color", "Nombre", "Orden" },
                values: new object[,]
                {
                    { 1, "PROGRAMADA", "#3B82F6", "Programada", 1 },
                    { 2, "CONFIRMADA", "#6366F1", "Confirmada", 2 },
                    { 3, "EN_ESPERA", "#F59E0B", "En espera", 3 },
                    { 4, "EN_ATENCION", "#8B5CF6", "En atención", 4 },
                    { 5, "ATENDIDA", "#10B981", "Atendida", 5 },
                    { 6, "CANCELADA", "#EF4444", "Cancelada", 6 },
                    { 7, "NO_ASISTIO", "#6B7280", "No asistió", 7 }
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "HorariosMedicos",
                columns: new[] { "HorarioMedicoId", "Activo", "ConsultorioId", "DiaSemana", "DuracionCitaMinutos", "HoraFin", "HoraInicio", "MedicoId", "MedicoNombre" },
                values: new object[,]
                {
                    { 1, true, 1, 1, 30, new TimeOnly(12, 0, 0), new TimeOnly(8, 0, 0), 1, "Dr(a). Carlos Rivas" },
                    { 2, true, 1, 1, 30, new TimeOnly(17, 0, 0), new TimeOnly(14, 0, 0), 1, "Dr(a). Carlos Rivas" },
                    { 3, true, 1, 2, 30, new TimeOnly(12, 0, 0), new TimeOnly(8, 0, 0), 1, "Dr(a). Carlos Rivas" },
                    { 4, true, 1, 2, 30, new TimeOnly(17, 0, 0), new TimeOnly(14, 0, 0), 1, "Dr(a). Carlos Rivas" },
                    { 5, true, 1, 3, 30, new TimeOnly(12, 0, 0), new TimeOnly(8, 0, 0), 1, "Dr(a). Carlos Rivas" },
                    { 6, true, 1, 3, 30, new TimeOnly(17, 0, 0), new TimeOnly(14, 0, 0), 1, "Dr(a). Carlos Rivas" },
                    { 7, true, 1, 4, 30, new TimeOnly(12, 0, 0), new TimeOnly(8, 0, 0), 1, "Dr(a). Carlos Rivas" },
                    { 8, true, 1, 4, 30, new TimeOnly(17, 0, 0), new TimeOnly(14, 0, 0), 1, "Dr(a). Carlos Rivas" },
                    { 9, true, 1, 5, 30, new TimeOnly(12, 0, 0), new TimeOnly(8, 0, 0), 1, "Dr(a). Carlos Rivas" },
                    { 10, true, 1, 5, 30, new TimeOnly(17, 0, 0), new TimeOnly(14, 0, 0), 1, "Dr(a). Carlos Rivas" },
                    { 11, true, 1, 6, 30, new TimeOnly(12, 0, 0), new TimeOnly(8, 0, 0), 1, "Dr(a). Carlos Rivas" },
                    { 12, true, 2, 1, 30, new TimeOnly(13, 0, 0), new TimeOnly(8, 0, 0), 2, "Dr(a). Andrea López" },
                    { 13, true, 2, 3, 30, new TimeOnly(13, 0, 0), new TimeOnly(8, 0, 0), 2, "Dr(a). Andrea López" },
                    { 14, true, 2, 5, 30, new TimeOnly(13, 0, 0), new TimeOnly(8, 0, 0), 2, "Dr(a). Andrea López" },
                    { 15, true, 3, 2, 30, new TimeOnly(15, 0, 0), new TimeOnly(9, 0, 0), 3, "Dr(a). Roberto Méndez" },
                    { 16, true, 3, 4, 30, new TimeOnly(15, 0, 0), new TimeOnly(9, 0, 0), 3, "Dr(a). Roberto Méndez" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Citas_ConsultorioId",
                schema: "dbo",
                table: "Citas",
                column: "ConsultorioId");

            migrationBuilder.CreateIndex(
                name: "IX_Citas_EstadoCitaId",
                schema: "dbo",
                table: "Citas",
                column: "EstadoCitaId");

            migrationBuilder.CreateIndex(
                name: "IX_Citas_MedicoId_FechaHoraInicio",
                schema: "dbo",
                table: "Citas",
                columns: new[] { "MedicoId", "FechaHoraInicio" });

            migrationBuilder.CreateIndex(
                name: "IX_Citas_PacienteId",
                schema: "dbo",
                table: "Citas",
                column: "PacienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Consultorios_Codigo",
                schema: "dbo",
                table: "Consultorios",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EstadosCita_Codigo",
                schema: "dbo",
                table: "EstadosCita",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HorariosMedicos_ConsultorioId",
                schema: "dbo",
                table: "HorariosMedicos",
                column: "ConsultorioId");

            migrationBuilder.CreateIndex(
                name: "IX_HorariosMedicos_MedicoId_DiaSemana",
                schema: "dbo",
                table: "HorariosMedicos",
                columns: new[] { "MedicoId", "DiaSemana" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Citas",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "HorariosMedicos",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "EstadosCita",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Consultorios",
                schema: "dbo");
        }
    }
}
