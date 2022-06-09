using System.Runtime.Serialization;
using MarketingBox.Redistribution.Service.Domain.Models;

namespace MarketingBox.Redistribution.Service.Grpc.Models;

[DataContract]
public class UpdateRedistributionStateRequest
{
    [DataMember(Order = 1)] public long RedistributionId { get; set; }
    [DataMember(Order = 2)] public RedistributionState Status { get; set; }
}