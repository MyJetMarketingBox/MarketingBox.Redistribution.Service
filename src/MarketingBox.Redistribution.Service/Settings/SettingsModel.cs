using MyJetWallet.Sdk.Service;
using MyYamlParser;

namespace MarketingBox.Redistribution.Service.Settings
{
    public class SettingsModel
    {
        [YamlProperty("RedistributionService.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }

        [YamlProperty("RedistributionService.ZipkinUrl")]
        public string ZipkinUrl { get; set; }

        [YamlProperty("RedistributionService.ElkLogs")]
        public LogElkSettings ElkLogs { get; set; }

        [YamlProperty("RedistributionService.PostgresConnectionString")]
        public string PostgresConnectionString { get; set; }

        [YamlProperty("RedistributionService.ReportingServiceUrl")]
        public string ReportingServiceUrl { get; set; }
        
        [YamlProperty("RedistributionService.RegistrationServiceUrl")]
        public string RegistrationServiceUrl { get; set; }

        [YamlProperty("RedistributionService.AffiliateServiceUrl")]
        public string AffiliateServiceUrl { get; set; }

        [YamlProperty("RedistributionService.MyNoSqlReaderHostPort")]
        public string MyNoSqlReaderHostPort { get; set; }
    }
}
