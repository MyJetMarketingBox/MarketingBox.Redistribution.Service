using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarketingBox.Redistribution.Service.Domain.Models;
using MarketingBox.Redistribution.Service.Grpc;
using MarketingBox.Redistribution.Service.Grpc.Models;
using MarketingBox.Redistribution.Service.Storage;
using MarketingBox.Reporting.Service.Grpc;
using MarketingBox.Reporting.Service.Grpc.Requests.Registrations;
using MarketingBox.Sdk.Common.Models;
using MarketingBox.Sdk.Common.Models.Grpc;
using Microsoft.Extensions.Logging;

namespace MarketingBox.Redistribution.Service.Services
{
    public class RedistributionService : IRedistributionService
    {
        private readonly ILogger<RedistributionService> _logger;
        private readonly RedistributionStorage _redistributionStorage;
        private readonly IRegistrationService _registrationService;

        public RedistributionService(ILogger<RedistributionService> logger, 
            RedistributionStorage redistributionStorage, 
            IRegistrationService registrationService)
        {
            _logger = logger;
            _redistributionStorage = redistributionStorage;
            _registrationService = registrationService;
        }

        public async Task<Response<RedistributionEntity>> CreateRedistributionAsync(CreateRedistributionRequest request)
        {
            try
            {
                var regIds = new List<long>();
                
                if (request.RegistrationsIds != null && request.RegistrationsIds.Any())
                    regIds.AddRange(request.RegistrationsIds);
                
                if (request.RegistrationSearchRequest != null)
                {
                    var response = await _registrationService.SearchAsync(request.RegistrationSearchRequest);
                    if (response.Status == ResponseStatus.Ok &&
                        response.Data != null && response.Data.Any())
                        regIds.AddRange(response.Data.Select(e => e.RegistrationId));
                }

                regIds = regIds.Distinct().ToList();
                
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
                    RegistrationsIds = regIds,
                    UseAutologin = request.UseAutologin
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