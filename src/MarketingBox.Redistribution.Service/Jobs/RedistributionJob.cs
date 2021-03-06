using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using MarketingBox.Affiliate.Service.Client.Interfaces;
using MarketingBox.Affiliate.Service.Domain.Models.Affiliates;
using MarketingBox.Redistribution.Service.Domain.Models;
using MarketingBox.Redistribution.Service.Grpc.Models;
using MarketingBox.Redistribution.Service.Logic;
using MarketingBox.Redistribution.Service.Storage;
using MarketingBox.Registration.Service.Domain.Models.Affiliate;
using MarketingBox.Registration.Service.Domain.Models.Registrations;
using MarketingBox.Registration.Service.Grpc;
using MarketingBox.Registration.Service.Grpc.Requests.Registration;
using MarketingBox.Reporting.Service.Grpc.Requests.Registrations;
using MarketingBox.Sdk.Common.Enums;
using MarketingBox.Sdk.Common.Exceptions;
using MarketingBox.Sdk.Common.Extensions;
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
        private readonly IAffiliateClient _affiliateClient;
        private readonly ICountryClient _countryClient;

        private static bool _activeProcessing;
        private DateTime _now = DateTime.Now;

        public RedistributionJob(ILogger<RedistributionJob> logger,
            RedistributionStorage redistributionStorage,
            FileStorage fileStorage,
            IRegistrationService registrationService,
            Reporting.Service.Grpc.IRegistrationService reportingService,
            IAffiliateClient affiliateClient,
            ICountryClient countryClient)
        {
            _logger = logger;
            _redistributionStorage = redistributionStorage;
            _fileStorage = fileStorage;
            _registrationService = registrationService;
            _reportingService = reportingService;
            _affiliateClient = affiliateClient;
            _countryClient = countryClient;
            _timer = new MyTaskTimer(
                nameof(RedistributionJob),
                TimeSpan.FromSeconds(60),
                logger,
                DoTime);
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

            _now = DateTime.UtcNow;

            foreach (var redistribution in collection)
            {
                try
                {
                    var logs = await _redistributionStorage.GetLogs(redistribution.Id, redistribution.TenantId);

                    if (!logs.Any())
                    {
                        await FailRedistribution(redistribution, "Cant process Redistribution without logs.");
                        continue;
                    }

                    if (logs.All(e => e.Result != RedistributionResult.InQueue))
                    {
                        await FinishRedistribution(redistribution);
                        continue;
                    }

                    var todaySent = logs.Count(e => e.RegistrationStatus == RegistrationStatus.Registered
                                                    && e.SendDate?.Date == _now.Date);

                    if (todaySent >= redistribution.DayLimit)
                        continue;

                    var canSendToday = redistribution.DayLimit - todaySent;

                    var nextPortion = canSendToday > redistribution.PortionLimit
                        ? redistribution.PortionLimit
                        : canSendToday;

                    if (nextPortion == 0)
                        continue;

                    switch (redistribution.Frequency)
                    {
                        case RedistributionFrequency.Minute:
                            await ProcessRedistribution(redistribution, logs, nextPortion);
                            break;
                        case RedistributionFrequency.Hour:
                            if (logs.All(e => e.SendDate == null) ||
                                logs.Max(e => e.SendDate) <= _now.AddHours(-1))
                                await ProcessRedistribution(redistribution, logs, nextPortion);
                            break;
                        case RedistributionFrequency.Day:
                            if (logs.All(e => e.SendDate == null) ||
                                logs.Max(e => e.SendDate) <= _now.AddDays(-1))
                                await ProcessRedistribution(redistribution, logs, nextPortion);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    await _redistributionStorage.SaveLogs(logs);
                }
                catch (Exception ex)
                {
                    await FailRedistribution(redistribution, ex.Message);
                }
            }
        }

        private async Task ProcessRedistribution(
            RedistributionEntity redistribution,
            IEnumerable<RedistributionLog> logs,
            int portionSize)
        {
            try
            {
                AffiliateMessage affiliate;
                try
                {
                    affiliate = await _affiliateClient.GetAffiliateById(
                            redistribution.AffiliateId,
                            redistribution.TenantId,
                            true);
                }
                catch (NotFoundException)
                {
                    await FailRedistribution(redistribution, "Cannot find affiliate.");
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
                            await ProcessFile(log, redistribution, affiliate);
                            break;
                        case EntityStorage.Database:
                            await ProcessRegistration(log, redistribution, affiliate);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        private async Task FailRedistribution(RedistributionEntity entity, string metadata)
        {
            await _redistributionStorage.UpdateState(entity.Id, RedistributionState.Error, metadata);
        }

        private async Task FinishRedistribution(RedistributionEntity entity)
        {
            await _redistributionStorage.UpdateState(entity.Id, RedistributionState.Finished);
        }

        private async Task ProcessRegistration(
            RedistributionLog log,
            RedistributionEntity redistributionEntity,
            AffiliateMessage affiliate)
        {
            try
            {
                var reportingResponse = await _reportingService.SearchAsync(new RegistrationSearchRequest()
                {
                    RegistrationIds = new List<long> {long.Parse(log.EntityId)},
                    Type = RegistrationsReportType.All
                });

                try
                {
                    var entity = reportingResponse.Process().First();

                    var countries = await _countryClient.GetCountries();
                    var country = countries.FirstOrDefault(x => x.Id == entity.CountryId);
                    if (country is null)
                    {
                        throw new NotFoundException("Country with id", entity.CountryId);
                    }

                    var registrationResponse = await _registrationService.CreateAsync(new RegistrationCreateRequest
                    {
                        GeneralInfo = new RegistrationGeneralInfo()
                        {
                            FirstName = entity.FirstName,
                            LastName = entity.LastName,
                            Password = entity.Password,
                            Email = entity.Email,
                            Phone = entity.Phone,
                            Ip = entity.Ip,
                            CountryCode = country.Alfa2Code,
                            CountryCodeType = CountryCodeType.Alfa2Code
                        },
                        AuthInfo = new AffiliateAuthInfo()
                        {
                            AffiliateId = redistributionEntity.AffiliateId,
                            ApiKey = affiliate.GeneralInfo.ApiKey,
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

                    log.RegistrationStatus = registrationResponse?.Data?.Status;

                    if (redistributionEntity.UseAutologin)
                    {
                        try
                        {
                            var result = registrationResponse.Process();
                            var autologinResult =
                                await AutoLoginClicker.Click(result.CustomerLoginUrl);
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
                                    JsonConvert.SerializeObject(registrationResponse) +
                                    JsonConvert.SerializeObject(autologinResult),
                                    null);
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.LogError("Cannot autologin. " +
                                             $"RedistributionId = {redistributionEntity.Id}. LogId = {log.Id}. " +
                                             $"AutologinError = CustomerLoginUrl is empty.");
                            SuccessLog(log,
                                JsonConvert.SerializeObject(registrationResponse),
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
                catch (Exception e)
                {
                    FailLog(log, JsonConvert.SerializeObject(e));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                FailLog(log, JsonConvert.SerializeObject(ex));
            }
        }

        private void FailLog(RedistributionLog log, string metadata)
        {
            log.SendDate = _now;
            log.Result = RedistributionResult.Error;
            log.Metadata = metadata;
        }

        private void SuccessLog(RedistributionLog log, string metadata, int? autologinResult)
        {
            log.SendDate = _now;
            log.Result = RedistributionResult.Success;
            log.Metadata = metadata;

            if (autologinResult != null)
                log.AutologinResult = autologinResult;
        }

        private async Task ProcessFile(
            RedistributionLog log,
            RedistributionEntity redistribution,
            AffiliateMessage affiliate)
        {
            try
            {
                var (registrations, total) = await _fileStorage
                    .ParseFile(new GetRegistrationsFromFileRequest
                        {
                            FileId = FileEntityUniqGenerator.GetFileId(log.EntityId),
                            Asc = true
                        }
                    );

                if (!registrations.Any())
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
                        ApiKey = affiliate.GeneralInfo.ApiKey,
                    },
                    CampaignId = redistribution.CampaignId,
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
                        AffCode = entity.AffCode
                    }
                });

                log.RegistrationStatus = registrationResponse?.Data?.Status;

                if (redistribution.UseAutologin)
                {
                    if (registrationResponse?.Data == null ||
                        string.IsNullOrWhiteSpace(registrationResponse.Data.CustomerLoginUrl))
                    {
                        _logger.LogError("Cannot autologin. " +
                                         $"RedistributionId = {redistribution.Id}. LogId = {log.Id}. " +
                                         $"AutologinError = CustomerLoginUrl is empty.");
                        SuccessLog(log,
                            JsonConvert.SerializeObject(registrationResponse),
                            null);
                        return;
                    }

                    var autologinResult =
                        await AutoLoginClicker.Click(registrationResponse.Data.CustomerLoginUrl);

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
                            JsonConvert.SerializeObject(registrationResponse) +
                            JsonConvert.SerializeObject(autologinResult),
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
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                FailLog(log, JsonConvert.SerializeObject(ex));
            }
        }

        public void Start()
        {
            _timer.Start();
        }
    }
}