using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DirectoryAuditoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Auditoria",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Servicio = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Accion = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Entidad = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    EntidadId = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Cambios = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UsuarioId = table.Column<int>(type: "int", nullable: true),
                    UsuarioLogin = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UsuarioNombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Rol = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Ip = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Dispositivo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Ruta = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Exito = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auditoria", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Auditoria_Entidad_EntidadId",
                schema: "dbo",
                table: "Auditoria",
                columns: new[] { "Entidad", "EntidadId" });

            migrationBuilder.CreateIndex(
                name: "IX_Auditoria_Fecha",
                schema: "dbo",
                table: "Auditoria",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_Auditoria_UsuarioId",
                schema: "dbo",
                table: "Auditoria",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Auditoria",
                schema: "dbo");
        }
    }
}
