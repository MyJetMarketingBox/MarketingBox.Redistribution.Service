using System.Threading.Tasks;
using MarketingBox.Redistribution.Service.Logic;
using NUnit.Framework;

namespace MarketingBox.Redistribution.Service.Tests;

public class TestAutoLoginClicker
{
    [SetUp]
    public void Setup()
    {
    }
    
    [Test]
    public async Task Test1()
    {
        var result = await AutoLoginClicker.Click("https://gengine.ru");
        Assert.AreEqual(200, result);
    }
}