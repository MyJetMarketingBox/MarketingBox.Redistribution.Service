using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MarketingBox.Redistribution.Service.Domain.Models;
using MarketingBox.Redistribution.Service.Logic;
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
            var bytes = await File.ReadAllBytesAsync(
                $"{Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName}" +
                $"/SvcSamples/RegistrationsSvc.csv");
            
            var registrationsFromSvc =
                await RegistrationsSvcParser.GetRegistrationsFromFile(bytes);
            
            Console.WriteLine(JsonConvert.SerializeObject(registrationsFromSvc));
            Assert.Pass();
        }
    }
}
