using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketingBox.Redistribution.Service.Postgres.Migrations
{
    public partial class RenameCreatedBy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedByName",
                schema: "redistribution-service",
                table: "redistribution",
                newName: "CreatedByUserName");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                schema: "redistribution-service",
                table: "redistribution",
                newName: "CreatedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_redistribution_CreatedBy",
                schema: "redistribution-service",
                table: "redistribution",
                newName: "IX_redistribution_CreatedByUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedByUserName",
                schema: "redistribution-service",
                table: "redistribution",
                newName: "CreatedByName");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                schema: "redistribution-service",
                table: "redistribution",
                newName: "CreatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_redistribution_CreatedByUserId",
                schema: "redistribution-service",
                table: "redistribution",
                newName: "IX_redistribution_CreatedBy");
        }
    }
}
