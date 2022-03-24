using Microsoft.EntityFrameworkCore;

namespace MarketingBox.Redistribution.Service.Postgres
{
    public class DatabaseContextFactory
    {
        private readonly DbContextOptionsBuilder<PgContext> _dbContextOptionsBuilder;

        public DatabaseContextFactory(DbContextOptionsBuilder<PgContext> dbContextOptionsBuilder)
        {
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
        }

        public PgContext Create()
        {
            return new PgContext(_dbContextOptionsBuilder.Options);
        }
    }
}