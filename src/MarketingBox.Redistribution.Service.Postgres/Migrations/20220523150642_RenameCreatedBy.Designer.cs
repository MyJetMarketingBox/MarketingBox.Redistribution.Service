// <auto-generated />
using System;
using System.Collections.Generic;
using MarketingBox.Redistribution.Service.Postgres;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MarketingBox.Redistribution.Service.Postgres.Migrations
{
    [DbContext(typeof(PgContext))]
    [Migration("20220523150642_RenameCreatedBy")]
    partial class RenameCreatedBy
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("redistribution-service")
                .HasAnnotation("ProductVersion", "6.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("MarketingBox.Redistribution.Service.Domain.Models.RedistributionEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<long>("AffiliateId")
                        .HasColumnType("bigint");

                    b.Property<string>("AffiliateName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("CampaignId")
                        .HasColumnType("bigint");

                    b.Property<string>("CampaignName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long>("CreatedByUserId")
                        .HasColumnType("bigint");

                    b.Property<string>("CreatedByUserName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("DayLimit")
                        .HasColumnType("integer");

                    b.Property<List<long>>("FilesIds")
                        .HasColumnType("bigint[]");

                    b.Property<int>("Frequency")
                        .HasColumnType("integer");

                    b.Property<string>("Metadata")
                        .HasColumnType("text");

                    b.Property<int>("PortionLimit")
                        .HasColumnType("integer");

                    b.Property<List<long>>("RegistrationsIds")
                        .HasColumnType("bigint[]");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<string>("TenantId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("UseAutologin")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.HasIndex("CreatedByUserId");

                    b.HasIndex("Status");

                    b.HasIndex("TenantId");

                    b.ToTable("redistribution", "redistribution-service");
                });

            modelBuilder.Entity("MarketingBox.Redistribution.Service.Domain.Models.RedistributionLog", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<int?>("AutologinResult")
                        .HasColumnType("integer");

                    b.Property<string>("EntityId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Metadata")
                        .HasColumnType("text");

                    b.Property<long>("RedistributionId")
                        .HasColumnType("bigint");

                    b.Property<int?>("RegistrationStatus")
                        .HasColumnType("integer");

                    b.Property<int>("Result")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("SendDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Storage")
                        .HasColumnType("integer");

                    b.Property<string>("TenantId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("Result");

                    b.HasIndex("SendDate");

                    b.HasIndex("TenantId", "RedistributionId");

                    b.HasIndex("RedistributionId", "Storage", "EntityId")
                        .IsUnique();

                    b.ToTable("redistribution-log", "redistribution-service");
                });

            modelBuilder.Entity("MarketingBox.Redistribution.Service.Domain.Models.RegistrationsFile", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long>("CreatedBy")
                        .HasColumnType("bigint");

                    b.Property<byte[]>("File")
                        .IsRequired()
                        .HasColumnType("bytea");

                    b.Property<string>("TenantId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("CreatedBy");

                    b.ToTable("registrations-file", "redistribution-service");
                });
#pragma warning restore 612, 618
        }
    }
}
