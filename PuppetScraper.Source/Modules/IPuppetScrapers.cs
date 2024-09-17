// "@    @      @    @

using PuppeteerSharp;

namespace PuppetScraper.Modules;

public interface IPuppetScrapers
{
    IPage PuppetPage { get; }
    Task StartScrapingAsync(CancellationToken stoppingToken);
    Task DisposeAsync();
    Task ClosePageAsync();
}