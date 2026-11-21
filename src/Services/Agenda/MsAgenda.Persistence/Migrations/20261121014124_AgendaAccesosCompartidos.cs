using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MsAgenda.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgendaAccesosCompartidos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccesosAgenda",
                schema: "dbo",
                columns: table => new
                {
                    AccesoAgendaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MedicoPropietarioId = table.Column<int>(type: "int", nullable: false),
                    MedicoPropietarioNombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    MedicoAutorizadoId = table.Column<int>(type: "int", nullable: false),
                    MedicoAutorizadoNombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Alcance = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Motivo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaRevocacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccesosAgenda", x => x.AccesoAgendaId);
                });

            migrationBuilder.CreateTable(
                name: "AccesoPacientes",
                schema: "dbo",
                columns: table => new
                {
                    AccesoAgendaId = table.Column<int>(type: "int", nullable: false),
                    PacienteId = table.Column<long>(type: "bigint", nullable: false),
                    PacienteNombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccesoPacientes", x => new { x.AccesoAgendaId, x.PacienteId });
                    table.ForeignKey(
                        name: "FK_AccesoPacientes_AccesosAgenda_AccesoAgendaId",
                        column: x => x.AccesoAgendaId,
                        principalSchema: "dbo",
                        principalTable: "AccesosAgenda",
                        principalColumn: "AccesoAgendaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccesosAgenda_MedicoAutorizadoId",
                schema: "dbo",
                table: "AccesosAgenda",
                column: "MedicoAutorizadoId");

            migrationBuilder.CreateIndex(
                name: "IX_AccesosAgenda_MedicoPropietarioId",
                schema: "dbo",
                table: "AccesosAgenda",
                column: "MedicoPropietarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccesoPacientes",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "AccesosAgenda",
                schema: "dbo");
        }
    }
}
