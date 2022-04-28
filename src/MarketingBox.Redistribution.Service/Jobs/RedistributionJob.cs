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

            foreach (var redistribution in collection)
            {
                var logs = await _redistributionStorage.GetLogs(redistribution.Id);

                var todaySent = logs.Count(e => e.SendDate?.Date == DateTime.UtcNow.Date);

                if (todaySent >= redistribution.DayLimit)
                    continue;

                var canSendToday = redistribution.DayLimit - todaySent;

                var nextPortion = canSendToday > redistribution.PortionLimit
                    ? redistribution.PortionLimit
                    : canSendToday;

                switch (redistribution.Frequency)
                {
                    case RedistributionFrequency.Minute:
                        await ProcessRedistribution(redistribution, logs, nextPortion);
                        break;
                    case RedistributionFrequency.Hour:
                        if (logs.Max(e => e.SendDate) <= DateTime.UtcNow.AddHours(-1))
                            await ProcessRedistribution(redistribution, logs, nextPortion);
                        break;
                    case RedistributionFrequency.Day:
                        if (logs.Max(e => e.SendDate) <= DateTime.UtcNow.AddDays(-1))
                            await ProcessRedistribution(redistribution, logs, nextPortion);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                await _redistributionStorage.SaveLogs(logs);
            }
        }

        private async Task ProcessRedistribution(RedistributionEntity redistribution,
            IEnumerable<RedistributionLog> logs,
            int portionSize)
        {
            var affiliateResponse = await _affiliateService.GetAsync(new AffiliateByIdRequest()
            {
                AffiliateId = redistribution.AffiliateId
            });

            if (affiliateResponse.Status != ResponseStatus.Ok || affiliateResponse.Data == null)
            {
                await FailRedistribution(redistribution);
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
                        await ProcessFile(log, redistribution, affiliateResponse.Data);
                        break;
                    case EntityStorage.Database:
                        await ProcessRegistration(log, redistribution, affiliateResponse.Data);
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
                        },
                        CampaignId = redistributionEntity.CampaignId,
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
                    
                    if (redistributionEntity.UseAutologin)
                    {
                        var autologinResult =
                            await AutoLoginClicker.Click(registrationResponse.Data.BrandInfo.LoginUrl);

                        if (autologinResult.Success)
                        {
                            SuccessLog(log, 
                                JsonConvert.SerializeObject(registrationResponse), 
                                autologinResult.StatusCode);
                        }
                        else
                        {
                            _logger.LogError("Cannot autologin. " +
                                             $"RedistributionId = {redistributionEntity.Id}. LogId = {log.Id}. " +
                                             $"AutologinError = {autologinResult.ErrorMessage}");
                            SuccessLog(log, 
                                JsonConvert.SerializeObject(registrationResponse) + JsonConvert.SerializeObject(autologinResult),
                                null);
                        }
                    }
                    else
                    {
                        SuccessLog(log, 
                            JsonConvert.SerializeObject(registrationResponse), 
                            null);
                    }
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

        private static void SuccessLog(RedistributionLog log, string metadata, int? autologinResult)
        {
            log.SendDate = DateTime.UtcNow;
            log.Result = RedistributionResult.Success;
            log.Metadata = metadata;

            if (autologinResult != null)
                log.AutologinResult = autologinResult;
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

            if (string.IsNullOrWhiteSpace(entity.FirstName) ||
                string.IsNullOrWhiteSpace(entity.LastName) ||
                string.IsNullOrWhiteSpace(entity.Email) ||
                string.IsNullOrWhiteSpace(entity.Phone) ||
                string.IsNullOrWhiteSpace(entity.Password) ||
                string.IsNullOrWhiteSpace(entity.Ip) ||
                string.IsNullOrWhiteSpace(entity.CountryAlfa2Code))
            {
                FailLog(log, "Cannot send entity with empty fields.");
                return;
            }

            var registrationResponse = await _registrationService.CreateAsync(new RegistrationCreateRequest()
            {
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
                },
                CampaignId = redistribution.CampaignId
            });
            
            if (redistribution.UseAutologin)
            {
                var autologinResult =
                    await AutoLoginClicker.Click(registrationResponse.Data.BrandInfo.LoginUrl);

                if (autologinResult.Success)
                {
                    SuccessLog(log, 
                        JsonConvert.SerializeObject(registrationResponse), 
                        autologinResult.StatusCode);
                }
                else
                {
                    _logger.LogError("Cannot autologin. " +
                                     $"RedistributionId = {redistribution.Id}. LogId = {log.Id}. " +
                                     $"AutologinError = {autologinResult.ErrorMessage}");
                    SuccessLog(log, 
                        JsonConvert.SerializeObject(registrationResponse) + JsonConvert.SerializeObject(autologinResult),
                        null);
                }
            }
            else
            {
                SuccessLog(log, 
                    JsonConvert.SerializeObject(registrationResponse), 
                    null);
            }
        }

        public void Start()
        {
            _timer.Start();
        }
    }
}