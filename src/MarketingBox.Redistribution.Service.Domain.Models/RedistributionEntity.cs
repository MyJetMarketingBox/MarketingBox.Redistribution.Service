using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MarketingBox.Redistribution.Service.Domain.Models;

[DataContract]
public class RedistributionEntity
{
    [DataMember(Order = 1)] public long Id { get; set; }
    [DataMember(Order = 2)] public long CreatedBy { get; set; }
    [DataMember(Order = 3)] public DateTime CreatedAt { get; set; }
    [DataMember(Order = 4)] public long AffiliateId { get; set; }
    [DataMember(Order = 5)] public long CampaignId { get; set; }
    [DataMember(Order = 6)] public RedistributionFrequency Frequency { get; set; }
    [DataMember(Order = 7)] public RedistributionState Status { get; set; }
    [DataMember(Order = 8)] public int PortionLimit { get; set; }
    [DataMember(Order = 9)] public int DayLimit { get; set; }
    [DataMember(Order = 10)] public bool UseAutologin { get; set; }
    [DataMember(Order = 11)] public List<long>? RegistrationsIds { get; set; }
    [DataMember(Order = 12)] public List<long>? FilesIds { get; set; }
    [DataMember(Order = 13)] public string? Metadata { get; set; }
    [DataMember(Order = 14)] public string TenantId { get; set; }
}