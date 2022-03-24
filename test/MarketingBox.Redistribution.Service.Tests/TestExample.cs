using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MarketingBox.Redistribution.Service.Domain.Models;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using NUnit.Framework;

namespace MarketingBox.Redistribution.Service.Tests
{
    public class TestExample
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task Test1()
        {
            var registrationsSvc = new List<RegistrationFromFile>();
            
            var bytes = await File.ReadAllBytesAsync("/Users/geomatika/Downloads/testSvc.csv");
            
            await using var stream = new MemoryStream(bytes);
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
            Console.WriteLine(JsonConvert.SerializeObject(registrationsSvc));
            Assert.Pass();
        }
    }
}
