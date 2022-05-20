using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MarketingBox.Redistribution.Service.Domain.Models;
using Microsoft.VisualBasic.FileIO;

namespace MarketingBox.Redistribution.Service.Logic
{
    public static class RegistrationsSvcParser
    {
        public static async Task<List<RegistrationFromFile>> GetRegistrationsFromFile(long fileId, byte[] file)
        {
            var registrationsSvc = new List<RegistrationFromFile>();
            
            await using var stream = new MemoryStream(file);
            using var reader = new StreamReader(stream);
            using var parser = new TextFieldParser(reader);
            
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(",", ";", "\t");
            int i = 0;
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
                    Ip = fields[5],
                    CountryAlfa2Code = fields[6],
                    Sub1 = fields[7],
                    Sub2 = fields[8],
                    Sub3 = fields[9],
                    Sub4 = fields[10],
                    Sub5 = fields[11],
                    Sub6 = fields[12],
                    Sub7 = fields[13],
                    Sub8 = fields[14],
                    Sub9 = fields[15],
                    Sub10 = fields[16],
                    Funnel = fields[17],
                    AffCode = fields[18],
                    FileId = fileId,
                    Index = ++i
                });
            }
            return registrationsSvc;
        }
    }
}