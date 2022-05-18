using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketingBox.Redistribution.Service.Postgres.Migrations
{
    public partial class Fix_indecies : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_redistribution-log_RedistributionId",
                schema: "redistribution-service",
                table: "redistribution-log");

            migrationBuilder.CreateIndex(
                name: "IX_redistribution-log_TenantId_RedistributionId",
                schema: "redistribution-service",
                table: "redistribution-log",
                columns: new[] { "TenantId", "RedistributionId" });

            migrationBuilder.CreateIndex(
                name: "IX_redistribution_Status",
                schema: "redistribution-service",
                table: "redistribution",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_redistribution_TenantId",
                schema: "redistribution-service",
                table: "redistribution",
                column: "TenantId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_redistribution-log_TenantId_RedistributionId",
                schema: "redistribution-service",
                table: "redistribution-log");

            migrationBuilder.DropIndex(
                name: "IX_redistribution_Status",
                schema: "redistribution-service",
                table: "redistribution");

            migrationBuilder.DropIndex(
                name: "IX_redistribution_TenantId",
                schema: "redistribution-service",
                table: "redistribution");

            migrationBuilder.CreateIndex(
                name: "IX_redistribution-log_RedistributionId",
                schema: "redistribution-service",
                table: "redistribution-log",
                column: "RedistributionId");
        }
    }
}
