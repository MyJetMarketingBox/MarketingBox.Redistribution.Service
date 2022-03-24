using System;

namespace MarketingBox.Redistribution.Service.Domain.Models
{
    public class RegistrationsFile
    {
        public long Id { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public byte[] File { get; set; }
    }
}