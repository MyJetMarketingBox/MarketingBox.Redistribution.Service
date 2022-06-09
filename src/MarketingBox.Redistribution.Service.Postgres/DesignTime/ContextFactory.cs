using MyJetWallet.Sdk.Postgres;

namespace MarketingBox.Redistribution.Service.Postgres.DesignTime
{
    public class ContextFactory : MyDesignTimeContextFactory<PgContext>
    {
        public ContextFactory() : base(options => new PgContext(options))
        {
        }
    }
}