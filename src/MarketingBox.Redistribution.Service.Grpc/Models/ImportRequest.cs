using System.Runtime.Serialization;

namespace MarketingBox.Redistribution.Service.Grpc.Models
{
    [DataContract]
    public class ImportRequest
    {
        [DataMember(Order = 1)] public long UserId { get; set; }
        [DataMember(Order = 2)] public byte[] RegistrationsFile { get; set; }
        [DataMember(Order = 3)] public string TenantId { get; set; }
        [DataMember(Order = 4)] public string FileName { get; set; }
    }
}