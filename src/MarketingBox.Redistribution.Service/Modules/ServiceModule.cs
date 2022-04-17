using Autofac;
using MarketingBox.Redistribution.Service.Jobs;
using MarketingBox.Redistribution.Service.Logic;
using MarketingBox.Redistribution.Service.Postgres;
using MarketingBox.Redistribution.Service.Storage;

namespace MarketingBox.Redistribution.Service.Modules
{
    public class ServiceModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DatabaseContextFactory>().AsSelf().SingleInstance();
            builder.RegisterType<FileStorage>().AsSelf().SingleInstance();
            builder.RegisterType<RedistributionStorage>().AsSelf().SingleInstance();
            builder.RegisterType<RedistributionJob>().As<IStartable>().SingleInstance();
        }
    }
}