using System;
using MarketingBox.Redistribution.Service.Domain.Models;

namespace MarketingBox.Redistribution.Service.Logic
{
    public class FileEntityUniqGenerator
    {
        public static string GenerateUniq(RegistrationFromFile entity)
        {
            return $"{entity.FileId}:{entity.Email}-{entity.Password}";
        }

        public static long GetFileId(string uniq)
        {
            if (string.IsNullOrWhiteSpace(uniq))
                throw new Exception("Cannot process empty uniq.");
            
            var determinator = uniq.IndexOf(":", StringComparison.Ordinal);

            return long.Parse(uniq[..determinator]);
        }
    }
}