using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LavaderoMotos.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddManoDeObraToVenta_Simple : Migration
    {
        /// <inheritdoc />
       protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.AddColumn<decimal>(
        name: "ManoDeObra",
        table: "Ventas",
        type: "decimal(18,2)",
        nullable: false,
        defaultValue: 0m);
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropColumn(
        name: "ManoDeObra",
        table: "Ventas");
}
    }
}
