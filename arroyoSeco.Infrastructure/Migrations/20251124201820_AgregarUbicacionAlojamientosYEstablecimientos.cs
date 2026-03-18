using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace arroyoSeco.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarUbicacionAlojamientosYEstablecimientos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Agregar columnas a Alojamientos
            migrationBuilder.AddColumn<double>(
                name: "Latitud",
                table: "Alojamientos",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitud",
                table: "Alojamientos",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Direccion",
                table: "Alojamientos",
                type: "text",
                nullable: true);

            // Agregar columnas a Establecimientos
            migrationBuilder.AddColumn<double>(
                name: "Latitud",
                table: "Establecimientos",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitud",
                table: "Establecimientos",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Direccion",
                table: "Establecimientos",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitud",
                table: "Alojamientos");

            migrationBuilder.DropColumn(
                name: "Longitud",
                table: "Alojamientos");

            migrationBuilder.DropColumn(
                name: "Direccion",
                table: "Alojamientos");

            migrationBuilder.DropColumn(
                name: "Latitud",
                table: "Establecimientos");

            migrationBuilder.DropColumn(
                name: "Longitud",
                table: "Establecimientos");

            migrationBuilder.DropColumn(
                name: "Direccion",
                table: "Establecimientos");
        }
    }
}
