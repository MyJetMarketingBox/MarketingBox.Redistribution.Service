using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MarketingBox.Redistribution.Service.Postgres.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "redistribution-service");

            migrationBuilder.CreateTable(
                name: "redistribution",
                schema: "redistribution-service",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AffiliateId = table.Column<long>(type: "bigint", nullable: false),
                    CampaignId = table.Column<long>(type: "bigint", nullable: false),
                    Frequency = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PortionLimit = table.Column<int>(type: "integer", nullable: false),
                    DayLimit = table.Column<int>(type: "integer", nullable: false),
                    RegistrationsIds = table.Column<List<long>>(type: "bigint[]", nullable: true),
                    FilesIds = table.Column<List<long>>(type: "bigint[]", nullable: true),
                    Metadata = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_redistribution", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "redistribution-log",
                schema: "redistribution-service",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RedistributionId = table.Column<long>(type: "bigint", nullable: false),
                    SendDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Storage = table.Column<int>(type: "integer", nullable: false),
                    EntityId = table.Column<string>(type: "text", nullable: false),
                    Result = table.Column<int>(type: "integer", nullable: false),
                    Metadata = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_redistribution-log", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "registrations-file",
                schema: "redistribution-service",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    File = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_registrations-file", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_redistribution_CreatedBy",
                schema: "redistribution-service",
                table: "redistribution",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_redistribution-log_RedistributionId",
                schema: "redistribution-service",
                table: "redistribution-log",
                column: "RedistributionId");

            migrationBuilder.CreateIndex(
                name: "IX_redistribution-log_RedistributionId_Storage_EntityId",
                schema: "redistribution-service",
                table: "redistribution-log",
                columns: new[] { "RedistributionId", "Storage", "EntityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_redistribution-log_Result",
                schema: "redistribution-service",
                table: "redistribution-log",
                column: "Result");

            migrationBuilder.CreateIndex(
                name: "IX_redistribution-log_SendDate",
                schema: "redistribution-service",
                table: "redistribution-log",
                column: "SendDate");

            migrationBuilder.CreateIndex(
                name: "IX_registrations-file_CreatedBy",
                schema: "redistribution-service",
                table: "registrations-file",
                column: "CreatedBy");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "redistribution",
                schema: "redistribution-service");

            migrationBuilder.DropTable(
                name: "redistribution-log",
                schema: "redistribution-service");

            migrationBuilder.DropTable(
                name: "registrations-file",
                schema: "redistribution-service");
        }
    }
}
