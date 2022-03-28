using Autofac;
using MarketingBox.Redistribution.Service.Grpc;

// ReSharper disable UnusedMember.Global

namespace MarketingBox.Redistribution.Service.Client
{
    public static class AutofacHelper
    {
        public static void RegisterRedistributionServiceClient(this ContainerBuilder builder, string grpcServiceUrl)
        {
            var factory = new ServiceClientFactory(grpcServiceUrl);

            builder.RegisterInstance(factory.GetRegistrationImporterService()).As<IRegistrationImporter>().SingleInstance();
        }
    }
}
