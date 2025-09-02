using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LavaderoMotos.Data.Migrations
{
    /// <inheritdoc />
    public partial class Addnotas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Notas",
                table: "OrdenesTrabajo",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notas",
                table: "OrdenesTrabajo");
        }
    }
}
