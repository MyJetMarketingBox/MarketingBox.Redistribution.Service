using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MarketingBox.Redistribution.Service.Domain.Models;
using Microsoft.VisualBasic.FileIO;

namespace MarketingBox.Redistribution.Service.Logic
{
    public class RegistrationsSvcParser
    {
        public static async Task<List<RegistrationFromFile>> GetRegistrationsFromFile(byte[] file)
        {
            var registrationsSvc = new List<RegistrationFromFile>();
            
            await using var stream = new MemoryStream(file);
            using var reader = new StreamReader(stream);
            using var parser = new TextFieldParser(reader);
            
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(",");
            while (!parser.EndOfData)
            {
                var fields = parser.ReadFields();
                
                if (fields == null || parser.LineNumber == 2)
                    continue;

                registrationsSvc.Add(new RegistrationFromFile()
                {
                    FirstName = fields[0],
                    LastName = fields[1],
                    Email = fields[2],
                    Phone = fields[3],
                    Password = fields[4],
                    Ip = fields[5]
                });
            }
            return registrationsSvc;
        }
    }
}