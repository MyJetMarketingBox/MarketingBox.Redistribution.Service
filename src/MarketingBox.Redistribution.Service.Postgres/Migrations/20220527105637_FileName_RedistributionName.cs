using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketingBox.Redistribution.Service.Postgres.Migrations
{
    public partial class FileName_RedistributionName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileName",
                schema: "redistribution-service",
                table: "registrations-file",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RedistributionName",
                schema: "redistribution-service",
                table: "redistribution",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_registrations-file_FileName",
                schema: "redistribution-service",
                table: "registrations-file",
                column: "FileName");

            migrationBuilder.CreateIndex(
                name: "IX_redistribution_RedistributionName",
                schema: "redistribution-service",
                table: "redistribution",
                column: "RedistributionName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_registrations-file_FileName",
                schema: "redistribution-service",
                table: "registrations-file");

            migrationBuilder.DropIndex(
                name: "IX_redistribution_RedistributionName",
                schema: "redistribution-service",
                table: "redistribution");

            migrationBuilder.DropColumn(
                name: "FileName",
                schema: "redistribution-service",
                table: "registrations-file");

            migrationBuilder.DropColumn(
                name: "RedistributionName",
                schema: "redistribution-service",
                table: "redistribution");
        }
    }
}
