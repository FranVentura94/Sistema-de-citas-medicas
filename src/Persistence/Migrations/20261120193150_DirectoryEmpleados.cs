using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DirectoryEmpleados : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Empleados",
                schema: "dbo",
                columns: table => new
                {
                    EmpleadoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombres = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Apellidos = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Identificacion = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    FechaNacimiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Genero = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Direccion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Cargo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaIngreso = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notas = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EmergenciaNombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EmergenciaParentesco = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EmergenciaTelefono = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    MedicoID = table.Column<int>(type: "int", nullable: true),
                    UsuarioID = table.Column<int>(type: "int", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empleados", x => x.EmpleadoID);
                    table.ForeignKey(
                        name: "FK_Empleados_Medicos_MedicoID",
                        column: x => x.MedicoID,
                        principalSchema: "dbo",
                        principalTable: "Medicos",
                        principalColumn: "MedicoID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EnfermeraMedicos",
                schema: "dbo",
                columns: table => new
                {
                    EmpleadoID = table.Column<int>(type: "int", nullable: false),
                    MedicoID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnfermeraMedicos", x => new { x.EmpleadoID, x.MedicoID });
                    table.ForeignKey(
                        name: "FK_EnfermeraMedicos_Empleados_EmpleadoID",
                        column: x => x.EmpleadoID,
                        principalSchema: "dbo",
                        principalTable: "Empleados",
                        principalColumn: "EmpleadoID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EnfermeraMedicos_Medicos_MedicoID",
                        column: x => x.MedicoID,
                        principalSchema: "dbo",
                        principalTable: "Medicos",
                        principalColumn: "MedicoID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "Empleados",
                columns: new[] { "EmpleadoID", "Activo", "Apellidos", "Cargo", "Direccion", "Email", "EmergenciaNombre", "EmergenciaParentesco", "EmergenciaTelefono", "FechaIngreso", "FechaNacimiento", "FechaRegistro", "Genero", "Identificacion", "MedicoID", "Nombres", "Notas", "Telefono", "UsuarioID" },
                values: new object[,]
                {
                    { 1, true, "del Sistema", "Administrativo", null, "admin@clinica.test", null, null, null, null, null, new DateTime(2026, 11, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, null, "Administrador", null, null, 1 },
                    { 2, true, "Martínez", "Recepción", null, "recepcion@clinica.test", null, null, null, null, null, new DateTime(2026, 11, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "F", null, null, "Laura", null, null, 2 },
                    { 3, true, "Rivas", "Médico", null, "medico@clinica.test", null, null, null, new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2026, 11, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "M", "04512378-9", 1, "Carlos", null, "7012-3456", 3 },
                    { 4, true, "López", "Médico", null, "andrea.lopez@clinica.test", null, null, null, new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2026, 11, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "F", "03456789-1", 2, "Andrea", null, "7123-9876", null },
                    { 5, true, "Méndez", "Médico", null, "roberto.mendez@clinica.test", null, null, null, new DateTime(2026, 3, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2026, 11, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "M", "02345678-5", 3, "Roberto", null, "7234-5678", null },
                    { 6, true, "Pérez", "Enfermería", null, null, null, null, null, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2026, 11, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "F", null, null, "Ana", null, "7345-1122", null }
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "EnfermeraMedicos",
                columns: new[] { "EmpleadoID", "MedicoID" },
                values: new object[] { 6, 1 });

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_MedicoID",
                schema: "dbo",
                table: "Empleados",
                column: "MedicoID",
                unique: true,
                filter: "[MedicoID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_UsuarioID",
                schema: "dbo",
                table: "Empleados",
                column: "UsuarioID",
                unique: true,
                filter: "[UsuarioID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EnfermeraMedicos_MedicoID",
                schema: "dbo",
                table: "EnfermeraMedicos",
                column: "MedicoID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EnfermeraMedicos",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Empleados",
                schema: "dbo");
        }
    }
}
