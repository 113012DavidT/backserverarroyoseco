using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace arroyoSeco.Infrastructure.Migrations.AuthDb
{
    /// <inheritdoc />
    public partial class AgregarRequiereCambioPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RequiereCambioPassword",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaPrimerLogin",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequiereCambioPassword",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FechaPrimerLogin",
                table: "AspNetUsers");
        }
    }
}
