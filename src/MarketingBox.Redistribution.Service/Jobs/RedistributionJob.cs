using System;
using System.Threading.Tasks;
using Autofac;
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

        public RedistributionJob(ILogger<RedistributionJob> logger, 
            RedistributionStorage redistributionStorage)
        {
            _logger = logger;
            _redistributionStorage = redistributionStorage;
            _timer = new MyTaskTimer(nameof(RedistributionJob), 
                TimeSpan.FromSeconds(60),
                logger, DoTime);
        }

        private async Task DoTime()
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            throw new System.NotImplementedException();
        }
    }
}