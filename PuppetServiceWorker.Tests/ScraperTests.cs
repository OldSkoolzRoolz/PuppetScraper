using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PuppetServiceWorker.Modules;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTests
{
    [TestClass]
    public class PuppetScrapersTests
    {
        [TestMethod]
        public void Constructor_NullLogger_ThrowsArgumentNullException()
        {
            // Arrange
            ILogger<Worker> logger = null;

            // Act and Assert
            Assert.ThrowsException<ArgumentNullException>(() => new PuppetScrapers(logger));
        }

        [TestMethod]
        public void Constructor_NonNullLogger_SetsLoggerProperty()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Worker>>();
            ILogger<Worker> logger = loggerMock.Object;

            // Act
            var puppetScrapers = new PuppetScrapers(logger);

            // Assert
            Assert.AreEqual(logger, puppetScrapers._logger);
        }

        [TestMethod]
        public async Task StartScrapingAsync_ReadsTargetListFromFile()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Worker>>();
            ILogger<Worker> logger = loggerMock.Object;
            var puppetScrapers = new PuppetScrapers(logger);
            var targetList = new[] { "http://example.com" };
            System.IO.File.WriteAllLines("targetlist.txt", targetList);

            // Act
            await puppetScrapers.StartScrapingAsync(CancellationToken.None);

            // Assert
            Assert.IsTrue(System.IO.File.Exists("targetlist.txt"));
            var readTargetList = System.IO.File.ReadAllLines("targetlist.txt");
            Assert.AreEqual(targetList.Length, readTargetList.Length);
        }

        [TestMethod]
        public async Task ScrapeVids_QuerySelectorAllAsync_ReturnsHtmlElements()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Worker>>();
            ILogger<Worker> logger = loggerMock.Object;
            var puppetScrapers = new PuppetScrapers(logger);
            var pageMock = new Mock<IPage>();
            pageMock.Setup(p => p.QuerySelectorAllAsync<HtmlElement>("span.img")).ReturnsAsync(new[] { new HtmlElement() });

            // Act
            await puppetScrapers.ScrapeVids(pageMock.Object);

            // Assert
            pageMock.Verify(p => p.QuerySelectorAllAsync<HtmlElement>("span.img"), Times.Once);
        }

        [TestMethod]
        public async Task ScrapeBdsmlr_PageGoToAsync_NavigatesToUrl()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Worker>>();
            ILogger<Worker> logger = loggerMock.Object;
            var puppetScrapers = new PuppetScrapers(logger);
            var pageMock = new Mock<IPage>();
            var url = "http://example.com";

            // Act
            await puppetScrapers.ScrapeBdsmlr(url, CancellationToken.None);

            // Assert
            pageMock.Verify(p => p.GoToAsync(url), Times.Once);
        }
    }
}