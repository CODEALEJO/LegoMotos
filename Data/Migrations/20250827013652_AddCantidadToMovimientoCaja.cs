using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LavaderoMotos.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCantidadToMovimientoCaja : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Cantidad",
                table: "MovimientoCajas",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cantidad",
                table: "MovimientoCajas");
        }
    }
}
