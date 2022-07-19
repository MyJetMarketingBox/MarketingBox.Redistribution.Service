using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarketingBox.Redistribution.Service.Domain.Models;
using MarketingBox.Redistribution.Service.Grpc.Models;
using MarketingBox.Redistribution.Service.Logic;
using MarketingBox.Redistribution.Service.Postgres;
using MarketingBox.Sdk.Common.Exceptions;
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
                        Result = RedistributionResult.InQueue,
                        TenantId = entity.TenantId
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
                            Result = RedistributionResult.InQueue,
                            TenantId = entity.TenantId,
                        });
                    }
                }
            }

            await UpsertRedistributionLog(logs);

            return entity;
        }

        private async Task<List<string>> GetFileEntities(long fileId)
        {
            var (registrations,total)= await _fileStorage.ParseFile(new GetRegistrationsFromFileRequest
            {
                FileId = fileId,
                Asc = true
            });

            if (!registrations.Any())
                return new List<string>();

            return registrations.Select(FileEntityUniqGenerator.GenerateUniq).ToList();
        }

        public async Task<RedistributionEntity?> UpdateState(
            long redistributionId,
            RedistributionState status,
            string metadata = "")
        {
            await using var ctx = _databaseContextFactory.Create();
            var entity = await ctx.RedistributionCollection
                .FirstOrDefaultAsync(e => e.Id == redistributionId);

            if (entity == null)
                return null;

            if (status == RedistributionState.Enable &&
                entity.Status != RedistributionState.Disable)
            {
                throw new BadRequestException("Enabling redistribution is allowed only once");
            }
            entity.Status = status;

            if (!string.IsNullOrEmpty(metadata))
                entity.Metadata = metadata;

            await ctx.SaveChangesAsync();

            return entity;
        }

        public async Task<(List<RedistributionEntity> result, int total)> Search(
            GetRedistributionsRequest request)
        {
            await using var ctx = _databaseContextFactory.Create();

            IQueryable<RedistributionEntity> query = ctx.RedistributionCollection;

            if (request.CreatedBy.HasValue && request.CreatedBy != 0)
                query = query.Where(e => e.CreatedByUserId == request.CreatedBy.Value);
            if (request.AffiliateId.HasValue && request.AffiliateId != 0)
                query = query.Where(e => e.AffiliateId == request.AffiliateId.Value);
            if (request.CampaignId.HasValue && request.CampaignId != 0)
                query = query.Where(e => e.CampaignId == request.CampaignId.Value);
            if (!string.IsNullOrEmpty(request.TenantId))
                query = query.Where(e => e.TenantId.Equals(request.TenantId));
            if (!string.IsNullOrEmpty(request.Name))
                query = query.Where(e => e.RedistributionName.ToLower().Contains(request.Name.ToLowerInvariant()));
            
            var total = query.Count();

            if (request.Asc)
            {
                if (request.Cursor != null)
                {
                    query = query.Where(x => x.Id > request.Cursor);
                }

                query = query.OrderBy(x => x.Id);
            }
            else
            {
                if (request.Cursor != null)
                {
                    query = query.Where(x => x.Id < request.Cursor);
                }

                query = query.OrderByDescending(x => x.Id);
            }

            if (request.Take.HasValue)
            {
                query = query.Take(request.Take.Value);
            }

            await query.LoadAsync();

            var result = query.ToList();

            return (result, total);
        }

        public async Task<List<RedistributionLog>> GetLogs(long redistributionId, string? tenantId)
        {
            await using var ctx = _databaseContextFactory.Create();
            return await ctx.RedistributionLogCollection
                .Where(e => e.TenantId.Equals(tenantId) &&
                            e.RedistributionId == redistributionId)
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