using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketingBox.Redistribution.Service.Postgres.Migrations
{
    public partial class RegistrationFile_CreatedByUserName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                schema: "redistribution-service",
                table: "registrations-file",
                newName: "CreatedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_registrations-file_CreatedBy",
                schema: "redistribution-service",
                table: "registrations-file",
                newName: "IX_registrations-file_CreatedByUserId");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserName",
                schema: "redistribution-service",
                table: "registrations-file",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedByUserName",
                schema: "redistribution-service",
                table: "registrations-file");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                schema: "redistribution-service",
                table: "registrations-file",
                newName: "CreatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_registrations-file_CreatedByUserId",
                schema: "redistribution-service",
                table: "registrations-file",
                newName: "IX_registrations-file_CreatedBy");
        }
    }
}
