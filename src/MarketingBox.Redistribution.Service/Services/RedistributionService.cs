using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarketingBox.Affiliate.Service.Grpc;
using MarketingBox.Affiliate.Service.Grpc.Requests.Affiliates;
using MarketingBox.Affiliate.Service.Grpc.Requests.Campaigns;
using MarketingBox.Redistribution.Service.Domain.Models;
using MarketingBox.Redistribution.Service.Grpc;
using MarketingBox.Redistribution.Service.Grpc.Models;
using MarketingBox.Redistribution.Service.Storage;
using MarketingBox.Reporting.Service.Grpc;
using MarketingBox.Sdk.Common.Extensions;
using MarketingBox.Sdk.Common.Models;
using MarketingBox.Sdk.Common.Models.Grpc;
using Microsoft.Extensions.Logging;
using IAffiliateService = MarketingBox.Affiliate.Service.Grpc.IAffiliateService;

namespace MarketingBox.Redistribution.Service.Services
{
    public class RedistributionService : IRedistributionService
    {
        private readonly ILogger<RedistributionService> _logger;
        private readonly RedistributionStorage _redistributionStorage;
        private readonly IRegistrationService _registrationService;
        private readonly IAffiliateService _affiliateService;
        private readonly ICampaignService _campaignService;

        public RedistributionService(ILogger<RedistributionService> logger, 
            RedistributionStorage redistributionStorage, 
            IRegistrationService registrationService, 
            IAffiliateService affiliateService, 
            ICampaignService campaignService)
        {
            _logger = logger;
            _redistributionStorage = redistributionStorage;
            _registrationService = registrationService;
            _affiliateService = affiliateService;
            _campaignService = campaignService;
        }

        public async Task<Response<RedistributionEntity>> CreateRedistributionAsync(CreateRedistributionRequest request)
        {
            try
            {
                request.ValidateEntity();
                
                var affiliateResponse = await _affiliateService.GetAsync(new AffiliateByIdRequest()
                {
                    AffiliateId = request.AffiliateId
                });
                if (affiliateResponse.Status != ResponseStatus.Ok ||
                    affiliateResponse.Data == null)
                    return new Response<RedistributionEntity>()
                    {
                        Status = ResponseStatus.BadRequest,
                        Error = new Error()
                        {
                            ErrorMessage = "Cant find affiliate."
                        }
                    };

                var campaignResponse = await _campaignService.GetAsync(new CampaignByIdRequest()
                {
                    CampaignId = request.CampaignId
                });
                if (campaignResponse.Status != ResponseStatus.Ok ||
                    campaignResponse.Data == null)
                    return new Response<RedistributionEntity>()
                    {
                        Status = ResponseStatus.BadRequest,
                        Error = new Error()
                        {
                            ErrorMessage = "Cant find campaign."
                        }
                    };
                

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
                
                if (!regIds.Any() &&
                    (request.FilesIds == null || !request.FilesIds.Any()))
                    return new Response<RedistributionEntity>()
                    {
                        Status = ResponseStatus.BadRequest,
                        Error = new Error()
                        {
                            ErrorMessage = "Cant create Redistribution without Registrations and Files."
                        }
                    };
                
                var entity = new RedistributionEntity()
                {
                    AffiliateId = (long)request.AffiliateId,
                    CampaignId = (long)request.CampaignId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = (long)request.CreatedBy,
                    DayLimit = (int)request.DayLimit,
                    FilesIds = request.FilesIds,
                    Frequency = (RedistributionFrequency)request.Frequency,
                    Status = request.Status,
                    PortionLimit = (int)request.PortionLimit,
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
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating partner {@context}", request);
                return e.FailedResponse<RedistributionEntity>();
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