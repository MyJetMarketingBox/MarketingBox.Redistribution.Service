using System.Collections.Generic;
using System.Runtime.Serialization;
using MarketingBox.Redistribution.Service.Domain.Models;
using MarketingBox.Reporting.Service.Grpc.Requests.Registrations;

namespace MarketingBox.Redistribution.Service.Grpc.Models;

[DataContract]
public class CreateRedistributionRequest
{
    [DataMember(Order = 1)] public long CreatedBy { get; set; }
    [DataMember(Order = 2)] public long AffiliateId { get; set; }
    [DataMember(Order = 3)] public long CampaignId { get; set; }
    [DataMember(Order = 4)] public RedistributionFrequency Frequency { get; set; }
    [DataMember(Order = 5)] public RedistributionState Status { get; set; }
    [DataMember(Order = 6)] public int PortionLimit { get; set; }
    [DataMember(Order = 7)] public int DayLimit { get; set; }
    [DataMember(Order = 8)] public bool UseAutologin { get; set; }
    [DataMember(Order = 9)] public List<long>? RegistrationsIds { get; set; }
    [DataMember(Order = 10)] public List<long>? FilesIds { get; set; }
    [DataMember(Order = 11)] public RegistrationSearchRequest? RegistrationSearchRequest { get; set; }
}