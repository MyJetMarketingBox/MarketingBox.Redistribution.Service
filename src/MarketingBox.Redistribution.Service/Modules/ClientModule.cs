using Autofac;
using MarketingBox.Affiliate.Service.Client;
using MarketingBox.Registration.Service.Client;
using MarketingBox.Reporting.Service.Client;

namespace MarketingBox.Redistribution.Service.Modules
{
    public class ClientModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterReportingServiceClient(Program.Settings.ReportingServiceUrl);
            builder.RegisterRegistrationServiceClient(Program.Settings.RegistrationServiceUrl);
            builder.RegisterAffiliateServiceClient(Program.Settings.AffiliateServiceUrl);
        }
    }
}