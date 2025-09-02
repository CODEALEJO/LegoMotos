using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LavaderoMotos.Data.Migrations
{
    /// <inheritdoc />
    public partial class Addmodificacionordenconcaja : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrdenTrabajoId",
                table: "MovimientoCajas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovimientoCajas_OrdenTrabajoId",
                table: "MovimientoCajas",
                column: "OrdenTrabajoId");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientoCajas_OrdenesTrabajo_OrdenTrabajoId",
                table: "MovimientoCajas",
                column: "OrdenTrabajoId",
                principalTable: "OrdenesTrabajo",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovimientoCajas_OrdenesTrabajo_OrdenTrabajoId",
                table: "MovimientoCajas");

            migrationBuilder.DropIndex(
                name: "IX_MovimientoCajas_OrdenTrabajoId",
                table: "MovimientoCajas");

            migrationBuilder.DropColumn(
                name: "OrdenTrabajoId",
                table: "MovimientoCajas");
        }
    }
}
