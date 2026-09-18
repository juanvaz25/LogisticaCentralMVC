using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LCP.Web.Migrations
{
    /// <inheritdoc />
    public partial class _04_AddSupportUrgencyAndLogisticsOperator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OperadorResponsable",
                table: "HistorialSeguimientos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EsUrgente",
                table: "ConsultasSoporte",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OperadorResponsable",
                table: "HistorialSeguimientos");

            migrationBuilder.DropColumn(
                name: "EsUrgente",
                table: "ConsultasSoporte");
        }
    }
}
