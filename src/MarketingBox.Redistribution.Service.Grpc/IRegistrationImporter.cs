using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using MarketingBox.Redistribution.Service.Domain.Models;
using MarketingBox.Redistribution.Service.Grpc.Models;
using MarketingBox.Sdk.Common.Models.Grpc;

namespace MarketingBox.Redistribution.Service.Grpc
{
    [ServiceContract]
    public interface IRegistrationImporter
    {
        [OperationContract]
        Task<Response<ImportResponse>> ImportAsync(ImportRequest request);
        
        [OperationContract]
        Task<Response<GetRegistrationFilesResponse>> GetRegistrationFilesAsync();
        
        [OperationContract]
        Task<Response<List<RegistrationFromFile>>> GetRegistrationsFromFileAsync(GetRegistrationsFromFileRequest request);
    }
}