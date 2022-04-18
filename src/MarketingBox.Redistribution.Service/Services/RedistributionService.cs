using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarketingBox.Redistribution.Service.Domain.Models;
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

        public async Task<Response<RedistributionEntity>> CreateRedistributionAsync(CreateRedistributionRequest request)
        {
            try
            {
                var entity = new RedistributionEntity()
                {
                    AffiliateId = request.AffiliateId,
                    CampaignId = request.CampaignId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = request.CreatedBy,
                    DayLimit = request.DayLimit,
                    FilesIds = request.FilesIds,
                    Frequency = request.Frequency,
                    Status = request.Status,
                    PortionLimit = request.PortionLimit,
                    RegistrationsIds = request.RegistrationsIds
                };
                
                var newEntity = await _redistributionStorage.Save(entity);

                return new Response<RedistributionEntity>()
                {
                    Status = ResponseStatus.Ok,
                    Data = newEntity
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new Response<RedistributionEntity>
                {
                    Status = ResponseStatus.InternalError,
                    Error = new Error
                    {
                        ErrorMessage = ex.Message
                    }
                };
            }
        }

        public async Task<Response<RedistributionEntity>> UpdateRedistributionStateAsync(UpdateRedistributionStateRequest request)
        {
            try
            {
                var entity = await _redistributionStorage
                    .UpdateState(request.RedistributionId, request.Status);

                if (entity == null)
                    return new Response<RedistributionEntity>
                    {
                        Status = ResponseStatus.NotFound
                    };
                
                return new Response<RedistributionEntity>
                {
                    Status = ResponseStatus.Ok,
                    Data = entity
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new Response<RedistributionEntity>
                {
                    Status = ResponseStatus.InternalError,
                    Error = new Error
                    {
                        ErrorMessage = ex.Message
                    }
                };
            }
        }

        public async Task<Response<List<RedistributionEntity>>> GetRedistributionsAsync(GetRedistributionsRequest request)
        {
            try
            {
                var collection = await _redistributionStorage
                    .Get(request.CreatedBy, request.AffiliateId, request.CampaignId);
                
                return new Response<List<RedistributionEntity>>
                {
                    Status = ResponseStatus.Ok,
                    Data = collection
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new Response<List<RedistributionEntity>>
                {
                    Status = ResponseStatus.InternalError,
                    Error = new Error
                    {
                        ErrorMessage = ex.Message
                    }
                };
            }
        }
    }
}