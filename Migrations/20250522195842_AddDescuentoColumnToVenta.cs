using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LavaderoMotos.Migrations
{
    /// <inheritdoc />
    public partial class AddDescuentoColumnToVenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Descuento",
                table: "Ventas",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Descuento",
                table: "Ventas");
        }
    }
}
