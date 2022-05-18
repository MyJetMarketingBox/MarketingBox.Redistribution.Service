using System;
using System.Runtime.Serialization;

namespace MarketingBox.Redistribution.Service.Domain.Models
{
    [DataContract]
    public class RegistrationsFile
    {
        [DataMember(Order = 1)] public long Id { get; set; }
        [DataMember(Order = 2)] public long CreatedBy { get; set; }
        [DataMember(Order = 3)] public DateTime CreatedAt { get; set; }
        public byte[] File { get; set; }
        [DataMember(Order = 4)] public string TenantId { get; set; }
    }
}