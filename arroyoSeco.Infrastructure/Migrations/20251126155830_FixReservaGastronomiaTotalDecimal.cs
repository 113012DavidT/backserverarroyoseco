using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace arroyoSeco.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixReservaGastronomiaTotalDecimal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Total",
                table: "ReservasGastronomia",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(65,30)",
                oldDefaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Total",
                table: "ReservasGastronomia",
                type: "numeric(65,30)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldDefaultValue: 0m);
        }
    }
}
