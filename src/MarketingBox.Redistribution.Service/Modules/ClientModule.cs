using Autofac;
using MarketingBox.Affiliate.Service.Client;
using MarketingBox.Registration.Service.Client;
using MarketingBox.Reporting.Service.Client;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.NoSql;

namespace MarketingBox.Redistribution.Service.Modules
{
    public class ClientModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterReportingServiceClient(Program.Settings.ReportingServiceUrl);
            builder.RegisterRegistrationServiceClient(Program.Settings.RegistrationServiceUrl);
            builder.RegisterAffiliateServiceClient(Program.Settings.AffiliateServiceUrl);
            
            var noSqlClient = builder.CreateNoSqlClient(
                Program.ReloadedSettings(e => e.MyNoSqlReaderHostPort).Invoke(),
                new LoggerFactory());
            builder.RegisterAffiliateClient(Program.Settings.AffiliateServiceUrl, noSqlClient);
            builder.RegisterCountryClient(Program.Settings.AffiliateServiceUrl, noSqlClient);
        }
    }
}