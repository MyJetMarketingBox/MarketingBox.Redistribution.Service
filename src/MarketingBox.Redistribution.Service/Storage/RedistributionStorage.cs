using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarketingBox.Redistribution.Service.Domain.Models;
using MarketingBox.Redistribution.Service.Logic;
using MarketingBox.Redistribution.Service.Postgres;
using Microsoft.EntityFrameworkCore;

namespace MarketingBox.Redistribution.Service.Storage
{
    public class RedistributionStorage
    {
        private readonly DatabaseContextFactory _databaseContextFactory;
        private readonly FileStorage _fileStorage;

        public RedistributionStorage(DatabaseContextFactory databaseContextFactory, 
            FileStorage fileStorage)
        {
            _databaseContextFactory = databaseContextFactory;
            _fileStorage = fileStorage;
        }

        public async Task<RedistributionEntity> Save(RedistributionEntity entity)
        {
            await using var ctx = _databaseContextFactory.Create();
            ctx.RedistributionCollection.Add(entity);
            await ctx.SaveChangesAsync();

            var logs = new List<RedistributionLog>();
            
            if (entity.RegistrationsIds != null && entity.RegistrationsIds.Any())
                logs.AddRange(entity.RegistrationsIds
                    .Distinct()
                    .Select(e => new RedistributionLog()
                {
                    RedistributionId = entity.Id,
                    Storage = EntityStorage.Database,
                    EntityId = e.ToString(),
                    Result = RedistributionResult.InQueue
                }));

            if (entity.FilesIds != null && entity.FilesIds.Any())
            {
                foreach (var fileId in entity.FilesIds)
                {
                    var entitiesIds = await GetFileEntities(fileId);

                    if (!entitiesIds.Any()) 
                        continue;
                    foreach (var element in entitiesIds.Where(element => logs.All(e => e.EntityId != element)))
                    {
                        logs.Add(new RedistributionLog
                        {
                            RedistributionId = entity.Id, 
                            Storage = EntityStorage.File, 
                            EntityId = element, 
                            Result = RedistributionResult.InQueue
                        });
                    }
                }
            }
            await UpsertRedistributionLog(logs);

            return entity;
        }

        private async Task<List<string> > GetFileEntities(long fileId)
        {
            var registrations = await _fileStorage.ParseFile(fileId);

            if (registrations == null || !registrations.Any())
                return new List<string>();
            
            return registrations.Select(FileEntityUniqGenerator.GenerateUniq).ToList();
        }

        public async Task<RedistributionEntity?> UpdateState(long redistributionId, RedistributionState status, string metadata = "")
        {
            await using var ctx = _databaseContextFactory.Create();
            var entity = await ctx.RedistributionCollection
                .FirstOrDefaultAsync(e => e.Id == redistributionId);

            if (entity == null)
                return null;
            
            entity.Status = status;
            
            if (!string.IsNullOrEmpty(metadata))
                entity.Metadata = metadata;
            
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
            await UpsertRedistributionLog(logs);
        }

        private async Task UpsertRedistributionLog(IEnumerable<RedistributionLog> logs)
        {
            await using var ctx = _databaseContextFactory.Create();
            await ctx.RedistributionLogCollection
                .UpsertRange(logs)
                .On(e => new {e.RedistributionId, e.Storage, e.EntityId})
                .RunAsync();
        }

        public async Task<List<RedistributionEntity>> GetActual()
        {
            await using var ctx = _databaseContextFactory.Create();
            return await ctx.RedistributionCollection
                .Where(e => e.Status == RedistributionState.Enable)
                .ToListAsync();
        }
    }
}