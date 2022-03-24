using System.Runtime.Serialization;

namespace MarketingBox.Redistribution.Service.Grpc.Models
{
    [DataContract]
    public class ImportRequest
    {
        [DataMember(Order = 1)] public byte[] RegistrationsFile { get; set; }
    }
}