using System.Threading.Tasks;
using MarketingBox.Redistribution.Service.Grpc;
using MarketingBox.Redistribution.Service.Grpc.Models;
using MarketingBox.Sdk.Common.Models.Grpc;

namespace MarketingBox.Redistribution.Service.Services
{
    public class RegistrationImporter: IRegistrationImporter
    {
        public Task<Response<ImportResponse>> ImportAsync(ImportRequest request)
        {
            throw new System.NotImplementedException();
        }
    }
}
