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

        public async Task<RedistributionEntity> Save(RedistributionEntity entity)
        {
            await using var ctx = _databaseContextFactory.Create();
            ctx.RedistributionCollection.Add(entity);
            await ctx.SaveChangesAsync();

            var logs = new List<RedistributionLog>();
            
            if (entity.RegistrationsIds != null && entity.RegistrationsIds.Any())
                logs.AddRange(entity.RegistrationsIds.Select(e => new RedistributionLog()
                {
                    RedistributionId = entity.Id,
                    Type = RedistributionEntityType.Registration,
                    EntityId = e,
                    Result = RedistributionResult.InQueue
                }));
            
            if (entity.FilesIds != null && entity.FilesIds.Any())
                logs.AddRange(entity.FilesIds.Select(e => new RedistributionLog()
                {
                    RedistributionId = entity.Id,
                    Type = RedistributionEntityType.File,
                    EntityId = e,
                    Result = RedistributionResult.InQueue
                }));

            await ctx.RedistributionLogCollection.AddRangeAsync(logs);
            await ctx.SaveChangesAsync();

            return entity;
        }

        public async Task<RedistributionEntity?> UpdateState(long redistributionId, RedistributionState status)
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

        public async Task<List<RedistributionEntity>> Get(long? createdBy = null, long? affiliateId = null, long? campaignId = null)
        {
            await using var ctx = _databaseContextFactory.Create();

            IQueryable<RedistributionEntity> query = ctx.RedistributionCollection;

            if (createdBy.HasValue && createdBy != 0)
                query = query.Where(e => e.CreatedBy == createdBy.Value);
            if (affiliateId.HasValue && affiliateId != 0)
                query = query.Where(e => e.AffiliateId == affiliateId.Value);
            if (campaignId.HasValue && campaignId != 0)
                query = query.Where(e => e.CampaignId == campaignId.Value);

            return query.ToList();
        }

        public async Task<List<RedistributionLog>> GetLogs(long redistributionId)
        {
            await using var ctx = _databaseContextFactory.Create();
            return await ctx.RedistributionLogCollection
                .Where(e => e.RedistributionId == redistributionId)
                .ToListAsync();
        }

        public async Task SaveLogs(IEnumerable<RedistributionLog> logs)
        {
            await using var ctx = _databaseContextFactory.Create();
            ctx.RedistributionLogCollection.UpsertRange(logs);
        }
    }
}