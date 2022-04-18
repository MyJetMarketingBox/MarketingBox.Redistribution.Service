using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using MarketingBox.Redistribution.Service.Domain.Models;
using MarketingBox.Redistribution.Service.Grpc.Models;
using MarketingBox.Sdk.Common.Models.Grpc;

namespace MarketingBox.Redistribution.Service.Grpc;

[ServiceContract]
public interface IRedistributionService
{
    [OperationContract]
    Task<Response<RedistributionEntity>> CreateRedistributionAsync(CreateRedistributionRequest request);
    
    [OperationContract]
    Task<Response<RedistributionEntity>> UpdateRedistributionStateAsync(UpdateRedistributionStateRequest request);
    
    [OperationContract]
    Task<Response<List<RedistributionEntity>>> GetRedistributionsAsync(GetRedistributionsRequest request);
}