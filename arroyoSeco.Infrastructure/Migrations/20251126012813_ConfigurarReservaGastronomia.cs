using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace arroyoSeco.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConfigurarReservaGastronomia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReservasGastronomia_Mesas_MesaId",
                table: "ReservasGastronomia");

            migrationBuilder.AlterColumn<decimal>(
                name: "Total",
                table: "ReservasGastronomia",
                type: "numeric(65,30)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "ReservasGastronomia",
                type: "text",
                nullable: false,
                defaultValue: "Pendiente",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddForeignKey(
                name: "FK_ReservasGastronomia_Mesas_MesaId",
                table: "ReservasGastronomia",
                column: "MesaId",
                principalTable: "Mesas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReservasGastronomia_Mesas_MesaId",
                table: "ReservasGastronomia");

            migrationBuilder.AlterColumn<decimal>(
                name: "Total",
                table: "ReservasGastronomia",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(65,30)",
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "ReservasGastronomia",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "Pendiente");

            migrationBuilder.AddForeignKey(
                name: "FK_ReservasGastronomia_Mesas_MesaId",
                table: "ReservasGastronomia",
                column: "MesaId",
                principalTable: "Mesas",
                principalColumn: "Id");
        }
    }
}
