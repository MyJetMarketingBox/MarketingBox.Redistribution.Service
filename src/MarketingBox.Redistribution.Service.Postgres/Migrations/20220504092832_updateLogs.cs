using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketingBox.Redistribution.Service.Postgres.Migrations
{
    public partial class updateLogs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RegistrationStatus",
                schema: "redistribution-service",
                table: "redistribution-log",
                type: "integer",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RegistrationStatus",
                schema: "redistribution-service",
                table: "redistribution-log");
        }
    }
}
