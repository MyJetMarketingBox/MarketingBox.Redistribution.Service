using JetBrains.Annotations;
using MarketingBox.Redistribution.Service.Grpc;
using MyJetWallet.Sdk.Grpc;

namespace MarketingBox.Redistribution.Service.Client
{
    [UsedImplicitly]
    public class ServiceClientFactory : MyGrpcClientFactory
    {
        public ServiceClientFactory(string grpcServiceUrl) : base(grpcServiceUrl)
        {
        }

        public IRegistrationImporter GetRegistrationImporterService() => CreateGrpcService<IRegistrationImporter>();
        public IRedistributionService GetRedistributionServiceService() => CreateGrpcService<IRedistributionService>();
    }
}
