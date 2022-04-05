using System;
using System.Threading.Tasks;
using MarketingBox.Redistribution.Service.Grpc;
using MarketingBox.Redistribution.Service.Grpc.Models;
using MarketingBox.Redistribution.Service.Postgres;
using MarketingBox.Sdk.Common.Models;
using MarketingBox.Sdk.Common.Models.Grpc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MarketingBox.Redistribution.Service.Services
{
    public class RedistributionService : IRedistributionService
    {
        private readonly DatabaseContextFactory _databaseContextFactory;
        private readonly ILogger<RedistributionService> _logger;

        public RedistributionService(DatabaseContextFactory databaseContextFactory, ILogger<RedistributionService> logger)
        {
            _databaseContextFactory = databaseContextFactory;
            _logger = logger;
        }

        public async Task CreateRedistributionAsync(Domain.Models.Redistribution entity)
        {
            try
            {
                await using var ctx = _databaseContextFactory.Create();
                ctx.RedistributionCollection.Upsert(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        public async Task<Response<Domain.Models.Redistribution>> UpdateRedistributionStateAsync(UpdateRedistributionStateRequest request)
        {
            try
            {
                await using var ctx = _databaseContextFactory.Create();
                var entity = await ctx.RedistributionCollection
                    .FirstOrDefaultAsync(e => e.Id == request.RedistributionId);

                if (entity == null)
                    return new Response<Domain.Models.Redistribution>()
                    {
                        Status = ResponseStatus.NotFound
                    };

                entity.Status = request.Status;
                await ctx.SaveChangesAsync();

                return new Response<Domain.Models.Redistribution>()
                {
                    Status = ResponseStatus.Ok,
                    Data = entity
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new Response<Domain.Models.Redistribution>()
                {
                    Status = ResponseStatus.InternalError,
                    Error = new Error()
                    {
                        ErrorMessage = ex.Message
                    }
                };
            }
        }
    }
}