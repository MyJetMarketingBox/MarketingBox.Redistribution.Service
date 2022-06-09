using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarketingBox.Affiliate.Service.Client.Interfaces;
using MarketingBox.Auth.Service.Client.Interfaces;
using MarketingBox.Redistribution.Service.Domain.Models;
using MarketingBox.Redistribution.Service.Grpc;
using MarketingBox.Redistribution.Service.Grpc.Models;
using MarketingBox.Redistribution.Service.Storage;
using MarketingBox.Reporting.Service.Grpc;
using MarketingBox.Sdk.Common.Extensions;
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
        private readonly IAffiliateClient _affiliateClient;
        private readonly IUserClient _userClient;
        private readonly ICampaignClient _campaignClient;

        public RedistributionService(ILogger<RedistributionService> logger,
            RedistributionStorage redistributionStorage,
            IRegistrationService registrationService,
            ICampaignClient campaignClient,
            IAffiliateClient affiliateClient,
            IUserClient userClient)
        {
            _logger = logger;
            _redistributionStorage = redistributionStorage;
            _registrationService = registrationService;
            _campaignClient = campaignClient;
            _affiliateClient = affiliateClient;
            _userClient = userClient;
        }

        public async Task<Response<RedistributionEntity>> CreateRedistributionAsync(CreateRedistributionRequest request)
        {
            try
            {
                request.ValidateEntity();

                var affiliateMessage = await _affiliateClient.GetAffiliateById(request.AffiliateId.Value, request.TenantId, true);
                var createdBy = await _userClient.GetUser(request.CreatedBy.Value, request.TenantId, true);
                var campaignMessage = await _campaignClient.GetCampaignById(request.CampaignId.Value, request.TenantId, true);

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
                    RedistributionName = request.Name,
                    AffiliateId = (long) request.AffiliateId,
                    CampaignId = (long) request.CampaignId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = (long) request.CreatedBy,
                    DayLimit = (int) request.DayLimit,
                    FilesIds = request.FilesIds,
                    Frequency = (RedistributionFrequency) request.Frequency,
                    Status = request.Status,
                    PortionLimit = (int) request.PortionLimit,
                    RegistrationsIds = regIds,
                    UseAutologin = (bool) request.UseAutologin,
                    TenantId = request.TenantId,
                    AffiliateName = affiliateMessage.GeneralInfo.Username,
                    CampaignName = campaignMessage.Name,
                    CreatedByUserName = createdBy.Username
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

        public async Task<Response<RedistributionEntity>> UpdateRedistributionStateAsync(
            UpdateRedistributionStateRequest request)
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
                return ex.FailedResponse<RedistributionEntity>();
            }
        }

        public async Task<Response<List<RedistributionEntity>>> GetRedistributionsAsync(
            GetRedistributionsRequest request)
        {
            try
            {
                var (collection, total) = await _redistributionStorage.Search(request);

                return new Response<List<RedistributionEntity>>
                {
                    Status = ResponseStatus.Ok,
                    Data = collection,
                    Total = total
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return ex.FailedResponse<List<RedistributionEntity>>();
            }
        }
    }
}