using System;
using System.Runtime.Serialization;

namespace MarketingBox.Redistribution.Service.Domain.Models;

[DataContract]
public class RedistributionLog
{
    [DataMember(Order = 1)] public long Id { get; set; }
    [DataMember(Order = 2)] public long RedistributionId { get; set; }
    [DataMember(Order = 3)] public DateTime? SendDate { get; set; }
    [DataMember(Order = 4)] public EntityStorage Storage { get; set; }
    [DataMember(Order = 5)] public string EntityId { get; set; }
    [DataMember(Order = 6)]  public RedistributionResult Result { get; set; }
    [DataMember(Order = 7)] public string? Metadata { get; set; }
}

public enum RedistributionResult
{
    InQueue,
    Success,
    Error
}

public enum EntityStorage
{
    Database,
    File
}