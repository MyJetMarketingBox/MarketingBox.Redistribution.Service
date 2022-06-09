using System.Runtime.Serialization;

namespace MarketingBox.Redistribution.Service.Domain.Models
{
    [DataContract]
    public class RegistrationFromFile
    {
        [DataMember(Order = 1)] public long FileId { get; set; }
        [DataMember(Order = 2)] public string FirstName { get; set; }
        [DataMember(Order = 3)] public string LastName { get; set; }
        [DataMember(Order = 4)] public string Email { get; set; }
        [DataMember(Order = 5)] public string Phone { get; set; }
        [DataMember(Order = 6)] public string Password { get; set; }
        [DataMember(Order = 7)] public string Ip { get; set; }
        [DataMember(Order = 8)] public string CountryAlfa2Code { get; set; }
        [DataMember(Order = 9)] public string Sub1 { get; set; }

        [DataMember(Order = 10)] public string Sub2 { get; set; }

        [DataMember(Order = 11)] public string Sub3 { get; set; }

        [DataMember(Order = 12)] public string Sub4 { get; set; }

        [DataMember(Order = 13)] public string Sub5 { get; set; }

        [DataMember(Order = 14)] public string Sub6 { get; set; }

        [DataMember(Order = 15)] public string Sub7 { get; set; }

        [DataMember(Order = 16)] public string Sub8 { get; set; }

        [DataMember(Order = 17)] public string Sub9 { get; set; }

        [DataMember(Order = 18)] public string Sub10 { get; set; }

        [DataMember(Order = 19)] public string Funnel { get; set; }

        [DataMember(Order = 20)] public string AffCode { get; set; }
        
        [DataMember(Order = 21)] public int Index { get; set; }
    }
}