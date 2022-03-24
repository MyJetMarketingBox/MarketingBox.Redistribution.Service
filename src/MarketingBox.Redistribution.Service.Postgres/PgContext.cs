using MarketingBox.Redistribution.Service.Domain.Models;
using Microsoft.EntityFrameworkCore;
using MyJetWallet.Sdk.Postgres;

namespace MarketingBox.Redistribution.Service.Postgres
{
    public class PgContext : MyDbContext
    {
        public const string Schema = "redistribution-service";
        
        private const string RegistrationsFileTableName = "registrations-file";
        
        public DbSet<RegistrationsFile> RegistrationsFileCollection { get; set; }

        public PgContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(Schema);
            
            SetCustomStrategyTable(modelBuilder);
            
            base.OnModelCreating(modelBuilder);
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