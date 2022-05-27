using System.Runtime.Serialization;
using MarketingBox.Sdk.Common.Attributes;

namespace MarketingBox.Redistribution.Service.Grpc.Models;

[DataContract]
public class GetRedistributionsRequest
{
    [DataMember(Order = 1), AdvancedCompare(ComparisonType.GreaterThanOrEqual, 1)]
    public long? Cursor { get; set; }

    [DataMember(Order = 2), AdvancedCompare(ComparisonType.GreaterThanOrEqual, 1)]
    public int? Take { get; set; }

    [DataMember(Order = 3)] public bool Asc { get; set; }
    [DataMember(Order = 4)] public long? CreatedBy { get; set; }
    [DataMember(Order = 5)] public long? AffiliateId { get; set; }
    [DataMember(Order = 6)] public long? CampaignId { get; set; }
    [DataMember(Order = 7)] public string TenantId { get; set; }
    [DataMember(Order = 8)] public string Name { get; set; }
}