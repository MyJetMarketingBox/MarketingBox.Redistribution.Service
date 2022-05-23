using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketingBox.Redistribution.Service.Postgres.Migrations
{
    public partial class Add_names : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AffiliateName",
                schema: "redistribution-service",
                table: "redistribution",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CampaignName",
                schema: "redistribution-service",
                table: "redistribution",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                schema: "redistribution-service",
                table: "redistribution",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AffiliateName",
                schema: "redistribution-service",
                table: "redistribution");

            migrationBuilder.DropColumn(
                name: "CampaignName",
                schema: "redistribution-service",
                table: "redistribution");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                schema: "redistribution-service",
                table: "redistribution");
        }
    }
}
