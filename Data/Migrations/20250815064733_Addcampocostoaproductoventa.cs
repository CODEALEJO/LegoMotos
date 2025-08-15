using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LavaderoMotos.Data.Migrations
{
    /// <inheritdoc />
    public partial class Addcampocostoaproductoventa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           migrationBuilder.Sql(@"
                UPDATE ProductoVentas pv
                SET Costo = p.Costo
                FROM Productos p
                WHERE pv.Producto = p.Nombre AND pv.Costo = 0
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Costo",
                table: "ProductosVenta");
        }
    }
}
