using MarketingBox.Redistribution.Service.Domain.Models;
using Microsoft.EntityFrameworkCore;
using MyJetWallet.Sdk.Postgres;

namespace MarketingBox.Redistribution.Service.Postgres
{
    public class PgContext : MyDbContext
    {
        public const string Schema = "redistribution-service";
        
        private const string RegistrationsFileTableName = "registrations-file";
        private const string RedistributionTableName = "redistribution";
        private const string RedistributionLogTableName = "redistribution-log";
        
        public DbSet<RegistrationsFile> RegistrationsFileCollection { get; set; }
        public DbSet<RedistributionEntity> RedistributionCollection { get; set; }
        public DbSet<RedistributionLog> RedistributionLogCollection { get; set; }

        public PgContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(Schema);
            
            SetCustomStrategyTable(modelBuilder);
            SetRedistributionTable(modelBuilder);
            SetRedistributionLogTable(modelBuilder);
            
            base.OnModelCreating(modelBuilder);
        }

        private void SetRedistributionLogTable(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RedistributionLog>().ToTable(RedistributionLogTableName);
            modelBuilder.Entity<RedistributionLog>().HasKey(e => e.Id);
            
            modelBuilder.Entity<RedistributionLog>().HasIndex(e => new {e.RedistributionId, Type = e.Storage, e.EntityId}).IsUnique();
            modelBuilder.Entity<RedistributionLog>().HasIndex(e => new {e.TenantId,e.RedistributionId});
            modelBuilder.Entity<RedistributionLog>().HasIndex(e => e.SendDate);
            modelBuilder.Entity<RedistributionLog>().HasIndex(e => e.Result);
        }

        private void SetRedistributionTable(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RedistributionEntity>().ToTable(RedistributionTableName);

            modelBuilder.Entity<RedistributionEntity>().HasKey(e => e.Id);
            modelBuilder.Entity<RedistributionEntity>().HasIndex(e => e.CreatedByUserId);
            modelBuilder.Entity<RedistributionEntity>().HasIndex(e => e.TenantId);
            modelBuilder.Entity<RedistributionEntity>().HasIndex(e => e.Status);
            modelBuilder.Entity<RedistributionEntity>().HasIndex(e => e.RedistributionName);
        }

        private void SetCustomStrategyTable(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RegistrationsFile>().ToTable(RegistrationsFileTableName);

            modelBuilder.Entity<RegistrationsFile>().HasKey(e => e.Id);
            
            modelBuilder.Entity<RegistrationsFile>().HasIndex(e => e.CreatedBy);
            modelBuilder.Entity<RegistrationsFile>().HasIndex(e => e.FileName);
        }
    }
}