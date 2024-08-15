

using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;


using PuppetServiceWorker;
using PuppetServiceWorker.Modules;



namespace UnitTests;

[TestClass]
public class PuppetScraperTests
{
    [TestMethod]
    public void Create_Scrapers_Class_Object()
    {

ILogger<Worker> _logger = new LoggerFactory().CreateLogger<Worker>();

        var scrapers = new PuppetScrapers(_logger);

        var page = scrapers.PuppetPage;
        
        Assert.IsNotNull(scrapers);


        
    }
}
