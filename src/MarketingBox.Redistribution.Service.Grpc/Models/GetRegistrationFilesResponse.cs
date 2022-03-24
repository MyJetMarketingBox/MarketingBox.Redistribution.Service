using System.Collections.Generic;
using System.Runtime.Serialization;
using MarketingBox.Redistribution.Service.Domain.Models;

namespace MarketingBox.Redistribution.Service.Grpc.Models;

[DataContract]
public class GetRegistrationFilesResponse
{
    [DataMember(Order = 1)] public List<RegistrationsFile> Files { get; set; }
}