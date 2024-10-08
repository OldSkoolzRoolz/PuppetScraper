using PuppeteerSharp;

namespace PuppetScraper;

public class PuppetBrowserBase : IDisposable, IAsyncDisposable
{
    public IBrowser Browser { get; set; }
    protected LaunchOptions? DefaultOptions { get; set; } = PuppetSettings.DefaultBrowserOptions();


    public PuppetBrowserBase()
    {

        Browser = Puppeteer.LaunchAsync( DefaultOptions).GetAwaiter().GetResult();

    }



  

    public virtual async Task DisposeAsync()
        {
            if (Browser is not null)
            {
                await Browser.CloseAsync();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    protected virtual void Dispose(bool disposing)
    {
        _ = DisposeAsync();
    }

    ValueTask IAsyncDisposable.DisposeAsync()
        {
            GC.SuppressFinalize(this);
            return Browser.DisposeAsync();
        }





















}