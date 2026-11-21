using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MsClinical.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ClinicalTriaje : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SignosTomadosPor",
                schema: "dbo",
                table: "Atenciones",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Triajes",
                schema: "dbo",
                columns: table => new
                {
                    TriajeId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CitaId = table.Column<long>(type: "bigint", nullable: false),
                    PacienteId = table.Column<long>(type: "bigint", nullable: false),
                    PacienteNombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MedicoId = table.Column<int>(type: "int", nullable: false),
                    MedicoNombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PresionSistolica = table.Column<int>(type: "int", nullable: true),
                    PresionDiastolica = table.Column<int>(type: "int", nullable: true),
                    FrecuenciaCardiaca = table.Column<int>(type: "int", nullable: true),
                    FrecuenciaRespiratoria = table.Column<int>(type: "int", nullable: true),
                    Temperatura = table.Column<decimal>(type: "decimal(4,1)", precision: 4, scale: 1, nullable: true),
                    SaturacionOxigeno = table.Column<int>(type: "int", nullable: true),
                    Peso = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Talla = table.Column<decimal>(type: "decimal(5,1)", precision: 5, scale: 1, nullable: true),
                    Notas = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RegistradoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    RegistradoPorNombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    RegistradoPorRol = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Triajes", x => x.TriajeId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Triajes_CitaId",
                schema: "dbo",
                table: "Triajes",
                column: "CitaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Triajes_PacienteId",
                schema: "dbo",
                table: "Triajes",
                column: "PacienteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Triajes",
                schema: "dbo");

            migrationBuilder.DropColumn(
                name: "SignosTomadosPor",
                schema: "dbo",
                table: "Atenciones");
        }
    }
}
