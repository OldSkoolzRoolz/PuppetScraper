



using PuppeteerSharp;


namespace PuppetScraper;

public class PuppetBrowserContextBase : PuppetBrowserBase
{
    protected IBrowserContext Context { get; set; }






    public PuppetBrowserContextBase()
    {
        Context = Browser.DefaultContext;
        // Context.Incognito = false;
    }
}
