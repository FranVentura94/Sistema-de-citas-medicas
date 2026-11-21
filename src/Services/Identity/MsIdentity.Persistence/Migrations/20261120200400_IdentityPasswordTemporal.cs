using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MsIdentity.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class IdentityPasswordTemporal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DebeCambiarPassword",
                schema: "dbo",
                table: "Usuarios",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordTemporalVence",
                schema: "dbo",
                table: "Usuarios",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DebeCambiarPassword",
                schema: "dbo",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "PasswordTemporalVence",
                schema: "dbo",
                table: "Usuarios");
        }
    }
}
