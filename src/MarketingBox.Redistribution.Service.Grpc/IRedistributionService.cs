using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using MarketingBox.Redistribution.Service.Grpc.Models;
using MarketingBox.Sdk.Common.Models.Grpc;

namespace MarketingBox.Redistribution.Service.Grpc;

[ServiceContract]
public interface IRedistributionService
{
    [OperationContract]
    Task CreateRedistributionAsync(Domain.Models.Redistribution entity);
    
    [OperationContract]
    Task<Response<Domain.Models.Redistribution>> UpdateRedistributionStateAsync(UpdateRedistributionStateRequest request);
    
    [OperationContract]
    Task<Response<List<Domain.Models.Redistribution>>> GetRedistributionsAsync(GetRedistributionsRequest request);
}