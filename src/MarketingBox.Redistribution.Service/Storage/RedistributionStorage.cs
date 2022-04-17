using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarketingBox.Redistribution.Service.Domain.Models;
using MarketingBox.Redistribution.Service.Postgres;
using Microsoft.EntityFrameworkCore;

namespace MarketingBox.Redistribution.Service.Storage
{
    public class RedistributionStorage
    {
        private readonly DatabaseContextFactory _databaseContextFactory;

        public RedistributionStorage(DatabaseContextFactory databaseContextFactory)
        {
            _databaseContextFactory = databaseContextFactory;
        }

        public async Task Save(Domain.Models.Redistribution entity)
        {
            await using var ctx = _databaseContextFactory.Create();
            ctx.RedistributionCollection.Upsert(entity);
        }

        public async Task<Domain.Models.Redistribution?> UpdateState(long redistributionId, RedistributionState status)
        {
            await using var ctx = _databaseContextFactory.Create();
            var entity = await ctx.RedistributionCollection
                .FirstOrDefaultAsync(e => e.Id == redistributionId);

            if (entity == null)
                return null;
            
            entity.Status = status;
            await ctx.SaveChangesAsync();

            return entity;
        }

        public async Task<List<Domain.Models.Redistribution>> Get(long? createdBy, long? affiliateId, long? campaignId)
        {
            await using var ctx = _databaseContextFactory.Create();

            IQueryable<Domain.Models.Redistribution> query = ctx.RedistributionCollection;

            if (createdBy.HasValue && createdBy != 0)
                query = query.Where(e => e.CreatedBy == createdBy.Value);
            if (affiliateId.HasValue && affiliateId != 0)
                query = query.Where(e => e.AffiliateId == affiliateId.Value);
            if (campaignId.HasValue && campaignId != 0)
                query = query.Where(e => e.CampaignId == campaignId.Value);

            return query.ToList();
        }
    }
}