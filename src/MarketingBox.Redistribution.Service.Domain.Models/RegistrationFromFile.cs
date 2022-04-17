using System.Runtime.Serialization;

namespace MarketingBox.Redistribution.Service.Domain.Models
{
    [DataContract]
    public class RegistrationFromFile
    {
        [DataMember(Order = 1)] public string FirstName { get; set; }
        [DataMember(Order = 2)] public string LastName { get; set; }
        [DataMember(Order = 3)] public string Email { get; set; }
        [DataMember(Order = 4)] public string Phone { get; set; }
        [DataMember(Order = 5)] public string Password { get; set; }
        [DataMember(Order = 6)] public string Ip { get; set; }
    }
}