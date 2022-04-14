using System.Runtime.Serialization;

namespace MarketingBox.Redistribution.Service.Grpc.Models;

[DataContract]
public class GetRedistributionsRequest
{
    [DataMember(Order = 1)] public long? CreatedBy { get; set; }
    [DataMember(Order = 2)] public long? AffiliateId { get; set; }
    [DataMember(Order = 3)] public long? CampaignId { get; set; }
}