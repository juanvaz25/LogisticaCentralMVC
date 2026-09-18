using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LCP.Web.Migrations
{
    /// <inheritdoc />
    public partial class _03_AddOrderRatingAndAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CalificacionComprador",
                table: "Pedidos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaUltimaActualizacion",
                table: "Pedidos",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CalificacionComprador",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "FechaUltimaActualizacion",
                table: "Pedidos");
        }
    }
}
