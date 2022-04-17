using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using MarketingBox.Redistribution.Service.Domain.Models;
using MarketingBox.Redistribution.Service.Storage;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service.Tools;

namespace MarketingBox.Redistribution.Service.Jobs
{
    public class RedistributionJob : IStartable
    {
        private readonly MyTaskTimer _timer;
        private readonly ILogger<RedistributionJob> _logger;
        private readonly RedistributionStorage _redistributionStorage;
        private readonly FileStorage _fileStorage;

        public RedistributionJob(ILogger<RedistributionJob> logger, 
            RedistributionStorage redistributionStorage, 
            FileStorage fileStorage)
        {
            _logger = logger;
            _redistributionStorage = redistributionStorage;
            _fileStorage = fileStorage;
            _timer = new MyTaskTimer(nameof(RedistributionJob), 
                TimeSpan.FromSeconds(60),
                logger, DoTime);
        }

        private async Task DoTime()
        {
            Console.WriteLine("RedistributionJob is OFFLINE");
            return;
            await ProcessRedistribution();
        }

        private async Task ProcessRedistribution()
        {
            var collection = await _redistributionStorage.Get();

            foreach (var entity in collection)
            {
                if (entity.Status == RedistributionState.Disable)
                    continue;

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
            List<RedistributionLog> logs,
            int portionSize)
        {
            foreach (var log in logs.Where(e => e.Result == RedistributionResult.InQueue))
            {
                switch (log.Type)
                {
                    case RedistributionEntityType.File:
                        await ProcessFile(log);
                        break;
                    case RedistributionEntityType.Registration:
                        await ProcessRegistration(log);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private async Task ProcessRegistration(RedistributionLog log)
        {
            throw new NotImplementedException();
        }

        private async Task ProcessFile(RedistributionLog log)
        {
            var registrationsFromFile = await _fileStorage.ParseFile(log.EntityId);

            if (registrationsFromFile == null || !registrationsFromFile.Any())
            {
                log.Result = RedistributionResult.Error;
                log.Metadata = "File is empty.";
                return;
            }
            
            
        }

        public void Start()
        {
            _timer.Start();
        }
    }
}