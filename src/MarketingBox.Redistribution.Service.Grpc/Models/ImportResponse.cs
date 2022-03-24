using System.Runtime.Serialization;

namespace MarketingBox.Redistribution.Service.Grpc.Models
{
    [DataContract]
    public class ImportResponse
    {
        [DataMember(Order = 1)] public long FileId { get; set; }
    }
}