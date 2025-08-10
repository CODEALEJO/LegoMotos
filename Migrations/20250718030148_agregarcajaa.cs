using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LavaderoMotos.Migrations
{
    /// <inheritdoc />
    public partial class agregarcajaa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovimientoCaja_Cajas_CajaId",
                table: "MovimientoCaja");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimientoCaja_Ventas_VentaId",
                table: "MovimientoCaja");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MovimientoCaja",
                table: "MovimientoCaja");

            migrationBuilder.RenameTable(
                name: "MovimientoCaja",
                newName: "MovimientoCajas");

            migrationBuilder.RenameIndex(
                name: "IX_MovimientoCaja_VentaId",
                table: "MovimientoCajas",
                newName: "IX_MovimientoCajas_VentaId");

            migrationBuilder.RenameIndex(
                name: "IX_MovimientoCaja_CajaId",
                table: "MovimientoCajas",
                newName: "IX_MovimientoCajas_CajaId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MovimientoCajas",
                table: "MovimientoCajas",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientoCajas_Cajas_CajaId",
                table: "MovimientoCajas",
                column: "CajaId",
                principalTable: "Cajas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientoCajas_Ventas_VentaId",
                table: "MovimientoCajas",
                column: "VentaId",
                principalTable: "Ventas",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovimientoCajas_Cajas_CajaId",
                table: "MovimientoCajas");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimientoCajas_Ventas_VentaId",
                table: "MovimientoCajas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MovimientoCajas",
                table: "MovimientoCajas");

            migrationBuilder.RenameTable(
                name: "MovimientoCajas",
                newName: "MovimientoCaja");

            migrationBuilder.RenameIndex(
                name: "IX_MovimientoCajas_VentaId",
                table: "MovimientoCaja",
                newName: "IX_MovimientoCaja_VentaId");

            migrationBuilder.RenameIndex(
                name: "IX_MovimientoCajas_CajaId",
                table: "MovimientoCaja",
                newName: "IX_MovimientoCaja_CajaId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MovimientoCaja",
                table: "MovimientoCaja",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientoCaja_Cajas_CajaId",
                table: "MovimientoCaja",
                column: "CajaId",
                principalTable: "Cajas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientoCaja_Ventas_VentaId",
                table: "MovimientoCaja",
                column: "VentaId",
                principalTable: "Ventas",
                principalColumn: "Id");
        }
    }
}
