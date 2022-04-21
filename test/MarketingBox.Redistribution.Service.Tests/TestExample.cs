using System;
using System.IO;
using System.Threading.Tasks;
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
                await RegistrationsSvcParser.GetRegistrationsFromFile(1, bytes);
            
            Console.WriteLine(JsonConvert.SerializeObject(registrationsFromSvc));
            
            
            Assert.Pass();
        }
        
        [Test]
        public async Task Test2()
        {
            var bytes = await File.ReadAllBytesAsync(
                $"{Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName}" +
                $"/SvcSamples/RegistrationsSvc.csv");

            var fileId = 777;
            
            var registrationsFromSvc =
                await RegistrationsSvcParser.GetRegistrationsFromFile(fileId, bytes);

            foreach (var registration in registrationsFromSvc)
            {
                var uniq = FileEntityUniqGenerator.GenerateUniq(registration);

                var fileIdFromUniq = FileEntityUniqGenerator.GetFileId(uniq);
                
                Assert.AreEqual(fileId, fileIdFromUniq);
            }
        }
    }
}
