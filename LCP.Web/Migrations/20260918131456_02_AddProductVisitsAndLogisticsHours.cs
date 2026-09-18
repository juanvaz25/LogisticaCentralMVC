using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LCP.Web.Migrations
{
    /// <inheritdoc />
    public partial class _02_AddProductVisitsAndLogisticsHours : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NumeroVisitas",
                table: "Productos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "HorarioAtencion",
                table: "NodosDistribucion",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumeroVisitas",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "HorarioAtencion",
                table: "NodosDistribucion");
        }
    }
}
