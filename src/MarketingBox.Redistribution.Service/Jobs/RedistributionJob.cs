using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using MarketingBox.Affiliate.Service.Grpc;
using MarketingBox.Affiliate.Service.Grpc.Requests.Affiliates;
using MarketingBox.Redistribution.Service.Domain.Models;
using MarketingBox.Redistribution.Service.Logic;
using MarketingBox.Redistribution.Service.Storage;
using MarketingBox.Registration.Service.Domain.Models.Affiliate;
using MarketingBox.Registration.Service.Domain.Models.Registrations;
using MarketingBox.Registration.Service.Grpc;
using MarketingBox.Registration.Service.Grpc.Requests.Registration;
using MarketingBox.Reporting.Service.Grpc.Requests.Registrations;
using MarketingBox.Sdk.Common.Enums;
using MarketingBox.Sdk.Common.Models.Grpc;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service.Tools;
using Newtonsoft.Json;

namespace MarketingBox.Redistribution.Service.Jobs
{
    public class RedistributionJob : IStartable
    {
        private readonly MyTaskTimer _timer;
        private readonly ILogger<RedistributionJob> _logger;
        private readonly RedistributionStorage _redistributionStorage;
        private readonly FileStorage _fileStorage;
        private readonly IRegistrationService _registrationService;
        private readonly Reporting.Service.Grpc.IRegistrationService _reportingService;
        private readonly IAffiliateService _affiliateService;
        
        private static bool _activeProcessing;

        public RedistributionJob(ILogger<RedistributionJob> logger, 
            RedistributionStorage redistributionStorage, 
            FileStorage fileStorage, 
            IRegistrationService registrationService, 
            Reporting.Service.Grpc.IRegistrationService reportingService,
            IAffiliateService affiliateService)
        {
            _logger = logger;
            _redistributionStorage = redistributionStorage;
            _fileStorage = fileStorage;
            _registrationService = registrationService;
            _reportingService = reportingService;
            _affiliateService = affiliateService;
            _timer = new MyTaskTimer(nameof(RedistributionJob), 
                TimeSpan.FromSeconds(60),
                logger, DoTime);
        }

        private async Task DoTime()
        {
            if (_activeProcessing)
            {
                return;
            }
            _activeProcessing = true;
            await ProcessRedistribution();
            _activeProcessing = false;
        }

        private async Task ProcessRedistribution()
        {
            var collection = await _redistributionStorage.GetActual();

            foreach (var entity in collection)
            {
                var logs = await _redistributionStorage.GetLogs(entity.Id);

                var todaySent = logs.Count(e => e.SendDate?.Date == DateTime.UtcNow.Date);

                if (todaySent >= entity.DayLimit)
                    continue;

                var canSendToday = entity.DayLimit - todaySent;

                var nextPortion = canSendToday > entity.PortionLimit
                    ? entity.PortionLimit
                    : canSendToday;

                switch (entity.Frequency)
                {
                    case RedistributionFrequency.Minute:
                        await ProcessRedistribution(entity, logs, nextPortion);
                        break;
                    case RedistributionFrequency.Hour:
                        if (logs.Max(e => e.SendDate) <= DateTime.UtcNow.AddHours(-1))
                            await ProcessRedistribution(entity, logs, nextPortion);
                        break;
                    case RedistributionFrequency.Day:
                        if (logs.Max(e => e.SendDate) <= DateTime.UtcNow.AddDays(-1))
                            await ProcessRedistribution(entity, logs, nextPortion);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                await _redistributionStorage.SaveLogs(logs);
            }
        }

        private async Task ProcessRedistribution(RedistributionEntity entity, 
            IEnumerable<RedistributionLog> logs,
            int portionSize)
        {
            var affiliateResponse = await _affiliateService.GetAsync(new AffiliateByIdRequest()
            {
                AffiliateId = entity.AffiliateId
            });

            if (affiliateResponse.Status != ResponseStatus.Ok || affiliateResponse.Data == null)
            {
                await FailRedistribution(entity);
                return;
            }
            
            var portion = logs
                .Where(e => e.Result == RedistributionResult.InQueue)
                .Take(portionSize);
            foreach (var log in portion)
            {
                switch (log.Storage)
                {
                    case EntityStorage.File:
                        await ProcessFile(log, entity, affiliateResponse.Data);
                        break;
                    case EntityStorage.Database:
                        await ProcessRegistration(log, entity, affiliateResponse.Data);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private async Task FailRedistribution(RedistributionEntity entity)
        {
            entity.Status = RedistributionState.Error;
            entity.Metadata = "Cannot find affiliate.";

            await _redistributionStorage.Save(entity);
        }

        private async Task ProcessRegistration(RedistributionLog log, RedistributionEntity redistributionEntity,
            Affiliate.Service.Domain.Models.Affiliates.Affiliate affiliate)
        {
            try
            {
                var reportingResponse = await _reportingService.SearchAsync(new RegistrationSearchRequest()
                {
                    RegistrationId = long.Parse(log.EntityId)
                });

                if (reportingResponse.Status == ResponseStatus.Ok && reportingResponse.Data.Any())
                {
                    var entity = reportingResponse.Data.First();

                    var registrationResponse = await _registrationService.CreateAsync(new RegistrationCreateRequest()
                    {
                        RegistrationMode = RegistrationMode.Auto,
                        GeneralInfo = new RegistrationGeneralInfo()
                        {
                            FirstName = entity.FirstName,
                            LastName = entity.LastName,
                            Password = entity.Password,
                            Email = entity.Email,
                            Phone = entity.Phone,
                            Ip = entity.Ip,
                            CountryCode = entity.CountryAlfa2Code,
                            CountryCodeType = CountryCodeType.Alfa2Code
                        },
                        AuthInfo = new AffiliateAuthInfo()
                        {
                            AffiliateId = redistributionEntity.AffiliateId,
                            ApiKey = affiliate.ApiKey,
                            CampaignId = redistributionEntity.CampaignId
                        },
                        AdditionalInfo = new RegistrationAdditionalInfo()
                        {
                            Sub1 = entity.Sub1,
                            Sub2 = entity.Sub2,
                            Sub3 = entity.Sub3,
                            Sub4 = entity.Sub4,
                            Sub5 = entity.Sub5,
                            Sub6 = entity.Sub6,
                            Sub7 = entity.Sub7,
                            Sub8 = entity.Sub8,
                            Sub9 = entity.Sub9,
                            Sub10 = entity.Sub10,
                            Funnel = entity.Funnel,
                            AffCode = entity.AffCode,
                        }
                    });
                    SuccessLog(log, JsonConvert.SerializeObject(registrationResponse));
                }
                else
                {
                    FailLog(log, "Cannot find registration.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                FailLog(log, JsonConvert.SerializeObject(ex));
            }
        }

        private static void FailLog(RedistributionLog log, string metadata)
        {
            log.SendDate = DateTime.UtcNow;
            log.Result = RedistributionResult.Error;
            log.Metadata = metadata;
        }

        private static void SuccessLog(RedistributionLog log, string metadata)
        {
            log.SendDate = DateTime.UtcNow;
            log.Result = RedistributionResult.Success;
            log.Metadata = metadata;
        }

        private async Task ProcessFile(RedistributionLog log, RedistributionEntity redistribution,
            Affiliate.Service.Domain.Models.Affiliates.Affiliate affiliate)
        {
            var registrations = await _fileStorage
                .ParseFile(FileEntityUniqGenerator.GetFileId(log.EntityId));

            if (registrations == null || !registrations.Any())
            {
                FailLog(log, "File is empty.");
                return;
            }

            var entity =
                registrations.FirstOrDefault(e => FileEntityUniqGenerator.GenerateUniq(e) == log.EntityId);

            if (entity == null)
            {
                FailLog(log, "Cannot find entity in file.");
                return;
            }
            
            var registrationResponse = await _registrationService.CreateAsync(new RegistrationCreateRequest()
            {
                RegistrationMode = RegistrationMode.Auto,
                GeneralInfo = new RegistrationGeneralInfo()
                {
                    FirstName = entity.FirstName,
                    LastName = entity.LastName,
                    Password = entity.Password,
                    Email = entity.Email,
                    Phone = entity.Phone,
                    Ip = entity.Ip,
                    CountryCode = entity.CountryAlfa2Code,
                    CountryCodeType = CountryCodeType.Alfa2Code
                },
                AuthInfo = new AffiliateAuthInfo()
                {
                    AffiliateId = redistribution.AffiliateId,
                    ApiKey = affiliate.ApiKey,
                    CampaignId = redistribution.CampaignId
                }
            });
            SuccessLog(log, JsonConvert.SerializeObject(registrationResponse));
        }

        public void Start()
        {
            _timer.Start();
        }
    }
}