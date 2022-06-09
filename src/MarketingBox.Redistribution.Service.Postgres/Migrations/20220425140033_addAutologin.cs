using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketingBox.Redistribution.Service.Postgres.Migrations
{
    public partial class addAutologin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AutologinResult",
                schema: "redistribution-service",
                table: "redistribution-log",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "UseAutologin",
                schema: "redistribution-service",
                table: "redistribution",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutologinResult",
                schema: "redistribution-service",
                table: "redistribution-log");

            migrationBuilder.DropColumn(
                name: "UseAutologin",
                schema: "redistribution-service",
                table: "redistribution");
        }
    }
}
