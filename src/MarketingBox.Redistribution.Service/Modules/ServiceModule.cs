using Autofac;
using MarketingBox.Redistribution.Service.Logic;
using MarketingBox.Redistribution.Service.Postgres;

namespace MarketingBox.Redistribution.Service.Modules
{
    public class ServiceModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DatabaseContextFactory>().AsSelf().SingleInstance();
        }
    }
}