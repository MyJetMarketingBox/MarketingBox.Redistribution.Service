using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using MarketingBox.Redistribution.Service.Domain.Models;
using MarketingBox.Reporting.Service.Grpc.Requests.Registrations;
using MarketingBox.Sdk.Common.Attributes;
using MarketingBox.Sdk.Common.Models;

namespace MarketingBox.Redistribution.Service.Grpc.Models;

[DataContract]
public class CreateRedistributionRequest : ValidatableEntity
{
    [DataMember(Order = 1), Required] public long? CreatedBy { get; set; }
    [DataMember(Order = 2), Required] public long? AffiliateId { get; set; }
    [DataMember(Order = 3), Required] public long? CampaignId { get; set; }
    [DataMember(Order = 4), Required, IsEnum] public RedistributionFrequency? Frequency { get; set; }
    [DataMember(Order = 5), IsEnum] public RedistributionState Status { get; set; }
    [DataMember(Order = 6), Required] public int? PortionLimit { get; set; }
    [DataMember(Order = 7), Required] public int? DayLimit { get; set; }
    [DataMember(Order = 8)] public bool UseAutologin { get; set; }
    [DataMember(Order = 9)] public List<long>? RegistrationsIds { get; set; }
    [DataMember(Order = 10)] public List<long>? FilesIds { get; set; }
    [DataMember(Order = 11)] public RegistrationSearchRequest? RegistrationSearchRequest { get; set; }
}