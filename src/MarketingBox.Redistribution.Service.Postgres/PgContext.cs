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
        
        public DbSet<RegistrationsFile> RegistrationsFileCollection { get; set; }
        public DbSet<Domain.Models.Redistribution> RedistributionCollection { get; set; }

        public PgContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(Schema);
            
            SetCustomStrategyTable(modelBuilder);
            SetRedistributionTable(modelBuilder);
            
            base.OnModelCreating(modelBuilder);
        }

        private void SetRedistributionTable(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Domain.Models.Redistribution>().ToTable(RedistributionTableName);

            modelBuilder.Entity<Domain.Models.Redistribution>().Property(e => e.Id).UseIdentityColumn();
            modelBuilder.Entity<Domain.Models.Redistribution>().HasKey(e => e.Id);
            
            modelBuilder.Entity<Domain.Models.Redistribution>().HasIndex(e => e.CreatedBy);
        }

        private void SetCustomStrategyTable(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RegistrationsFile>().ToTable(RegistrationsFileTableName);

            modelBuilder.Entity<RegistrationsFile>().Property(e => e.Id).UseIdentityColumn();
            modelBuilder.Entity<RegistrationsFile>().HasKey(e => e.Id);
            
            modelBuilder.Entity<RegistrationsFile>().HasIndex(e => e.CreatedBy);
        }
    }
}