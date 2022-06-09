using System.Runtime.Serialization;
using MarketingBox.Sdk.Common.Attributes;

namespace MarketingBox.Redistribution.Service.Grpc.Models;

[DataContract]
public class GetRegistrationsFromFileRequest
{    
    [DataMember(Order = 1), AdvancedCompare(ComparisonType.GreaterThanOrEqual, 1)]
    public long? Cursor { get; set; }

    [DataMember(Order = 2), AdvancedCompare(ComparisonType.GreaterThanOrEqual, 1)]
    public int? Take { get; set; }

    [DataMember(Order = 3)] public bool Asc { get; set; }
    [DataMember(Order = 4)] public long FileId { get; set; }
}