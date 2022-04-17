using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarketingBox.Redistribution.Service.Grpc;
using MarketingBox.Redistribution.Service.Grpc.Models;
using MarketingBox.Redistribution.Service.Storage;
using MarketingBox.Sdk.Common.Models;
using MarketingBox.Sdk.Common.Models.Grpc;
using Microsoft.Extensions.Logging;

namespace MarketingBox.Redistribution.Service.Services
{
    public class RedistributionService : IRedistributionService
    {
        private readonly ILogger<RedistributionService> _logger;
        private readonly RedistributionStorage _redistributionStorage;

        public RedistributionService(ILogger<RedistributionService> logger, 
            RedistributionStorage redistributionStorage)
        {
            _logger = logger;
            _redistributionStorage = redistributionStorage;
        }

        public async Task CreateRedistributionAsync(Domain.Models.Redistribution entity)
        {
            try
            {
                await _redistributionStorage.Save(entity);
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
                var entity = await _redistributionStorage
                    .UpdateState(request.RedistributionId, request.Status);

                if (entity == null)
                    return new Response<Domain.Models.Redistribution>()
                    {
                        Status = ResponseStatus.NotFound
                    };
                
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

        public async Task<Response<List<Domain.Models.Redistribution>>> GetRedistributionsAsync(GetRedistributionsRequest request)
        {
            try
            {
                var collection = await _redistributionStorage
                    .Get(request.CreatedBy, request.AffiliateId, request.CampaignId);
                
                return new Response<List<Domain.Models.Redistribution>>()
                {
                    Status = ResponseStatus.Ok,
                    Data = collection
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new Response<List<Domain.Models.Redistribution>>()
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