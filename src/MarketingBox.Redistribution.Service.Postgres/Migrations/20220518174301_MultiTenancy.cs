using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketingBox.Redistribution.Service.Postgres.Migrations
{
    public partial class MultiTenancy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                schema: "redistribution-service",
                table: "registrations-file",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                schema: "redistribution-service",
                table: "redistribution-log",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                schema: "redistribution-service",
                table: "redistribution",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "redistribution-service",
                table: "registrations-file");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "redistribution-service",
                table: "redistribution-log");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "redistribution-service",
                table: "redistribution");
        }
    }
}
