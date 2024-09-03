using Microsoft.Extensions.Logging;

using PuppeteerSharp;
using PuppeteerSharp.Dom;

using PuppetServiceWorker.Data;

namespace PuppetServiceWorker.Modules;



public class PuppetScrapers : PuppetPageBase, IDisposable, IAsyncDisposable, IPuppetScrapers
{

    private ILogger _logger;
    public PuppetScrapers()
    {
       
    }


    public IPage PuppetPage { get { return Page; } }



    /// <summary>
    /// Method designed for scraping sharesome.com posts with scrolling. Sharesome.com features an element recycling infinite scroller.
    /// which reduces browser overhead and speeds up transitions between screen redraws. see=cref"vue-recycle-scroller"
    /// </summary>
    /// <param name="address"></param>
    /// <param name="stoppingToken"></param>
    /// <returns>Task</returns>
    public async Task ScrapeShareSome(string address, CancellationToken stoppingToken)
    {
        WaitUntilNavigation[] waitUntil = [WaitUntilNavigation.DOMContentLoaded, WaitUntilNavigation.Load];
        ArgumentNullException.ThrowIfNull(address, nameof(address));

        if (Page == null) { Page = await CreatePage(); }



        await Page.GoToAsync(address, new NavigationOptions { WaitUntil = waitUntil });
        await Page.WaitForSelectorAsync("div.vue-recycle-scroller__item-wrapper");

        await Page.EvaluateExpressionAsync(@"
                {
                    window.onscroll =  function(ev)
                            {   
                            if((window.innerHeight + window.PageYOffset) >= document.body.offsetHeight) 
                                {
                                alert('scrolled to bottom');
                                }
                            }
                }");

        var ageButton = await Page.QuerySelectorAsync<HtmlButtonElement>("button[data-event=age_verification]");
        if (ageButton != null)
        {
            await ageButton.ClickAsync();
        }

        await Task.Delay(1000);



        for (int x = 0; x < 15; x++)
        {
            if (stoppingToken.IsCancellationRequested) break;

            //var docSize = await Page.EvaluateExpressionAsync<string>("document.documentElement.clientHeight");
            //System.Console.WriteLine("Doc Size: " + docSize);

            var vueScroller = await Page.QuerySelectorAsync<HtmlDivElement>("div.vue-recycle-scroller.ready.Page-mode.direction-vertical");
            //  if (vueScroller == null) break;

            await Page.WaitForSelectorAsync("div.vue-recycle-scroller__item-wrapper");
            await Task.Delay(4000);

            // Get the Items in the scroller and check against what we already extracted
            var dataItems = await Page.QuerySelectorAllAsync(".vue-recycle-scroller__item-view");


            // continueScraping is currently set to true only when an extracted post is new and not found in tracker db
            // TODO: Improve the tracker. This should be improved. We continue as long as we have new posts up to outer loop limit
            // Sharesome.com has very limited sorting capabilities, making it difficult to track and skip previously scraped itemsl

            await ExtractPostData(dataItems, Page);



            // Scroll to bottom triggering the infinite scroller
            await Page.EvaluateExpressionAsync("window.scrollTo(0,document.body.scrollHeight)");
            await Task.Delay(2000);




        }
        await Task.Delay(5000);
    }




    public IPage PuppeteerPage => Page;







    private static async Task ExtractPostData(IElementHandle[] items, IPage Page)
    {
        ArgumentNullException.ThrowIfNull(Page, nameof(Page));
        ArgumentNullException.ThrowIfNull(items, nameof(items));

        foreach (var item in items)
        {
            try
            {

                var postElement = await item.QuerySelectorAsync("article");
                if (postElement == null)
                {

                    Console.WriteLine("postElement is null");
                    continue;
                }

                var postId = await Page.EvaluateFunctionAsync<string>("e => e.getAttribute('data-post-id')", postElement);
                if (postId == null || postId == "")
                {
                    Console.WriteLine("postId is null");
                    continue;
                }

                var postType = await Page.EvaluateFunctionAsync<string>("e => e.querySelector('article').getAttribute('data-post-type')", item);

                if (TrackingDb.TrackerContains(postId)) continue;
                TrackingDb.PostTracker.Add(postId, postId);
                if (postType != "video" || postType == null || postType == "") continue;

                var videle = await Page.EvaluateFunctionAsync<string>("e => e.querySelector('video').src", item);

                TrackingDb.InsertRecord(postId, postType, videle);
                Console.WriteLine($"data-index=na  post-data-id={postId} posttype={postType} url={videle}");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"error extracting data...{ex.Message}");
            }
        }

    }





    public async Task StartScrapingAsync(CancellationToken stoppingToken)
    {



        var targetList = System.IO.File.ReadAllLines("targetlist.txt");


        foreach (var webPage in targetList)
        {
            await ScrapeShareSome(webPage, stoppingToken);
        }



    }







    async Task<IPage> CreatePage()
    {

        var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = false,
            Timeout = 0,
            EnqueueTransportMessages = false,
            LogProcess = true,

            ExecutablePath = @"/Storage/Apps/Browser/chrome-linux64/chrome",
            UserDataDir = "/Storage/Data/ChromeData",
            DefaultViewport = new ViewPortOptions { Width = 1280, Height = 4000 },
        });
        var pages = await browser.PagesAsync();
        if (pages.Length == 0)
        {
            return await browser.NewPageAsync();
        }
        return pages[0];
    }



    public async Task ScrapeFetishShrine(string url, CancellationToken stoppingToken)
    {


        using (var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = false,
            ExecutablePath = "/Storage/Apps/Browser/chrome-linux64/chrome",
            DefaultViewport = new ViewPortOptions { Width = 1900, Height = 1080 },
            Args = new string[] { "--start-maximized" }
        }))
        {

            //      browser.DefaultWaitForTimeout = 100000;
            Page.DefaultNavigationTimeout = 100000;
            Page.DefaultTimeout = 100000;

            await Page.GoToAsync("https://www.fetishshrine.com/categories/bizarre/");
            bool lastpageReached = false;

            while (!lastpageReached)
            {

                await ScrapeVids(Page);


                var next = await Page.QuerySelectorAsync<HtmlAnchorElement>("a[title='Next']");
                if (next == null)
                {
                    lastpageReached = true;
                }
                else
                {
                    await next.ClickAsync();
                }
                await Task.Delay(5000);


            }
        }
    }








    private static async Task ScrapeVids(IPage page)
    {
        var vids = await page.QuerySelectorAllAsync<HtmlElement>("span.img");


        foreach (var src in vids)
        {

            var link = await src.GetAttributeAsync("data-src");
            System.Console.WriteLine(link);
            TrackingDb.InsertRecord(Path.GetFileNameWithoutExtension(link)!, "video", link!);
        }
    }


    internal async Task SearchForVids(string address)
    {
        await Page.GoToAsync(address);
        await Task.Delay(3000);

        var vids = await Page.QuerySelectorAllAsync<HtmlElement>("*[href$='.mp4']");
        foreach (var vid in vids)
        {
            var link = await vid.GetAttributeAsync("href");
            System.Console.WriteLine(link);
            TrackingDb.InsertRecord(Path.GetFileNameWithoutExtension(link)!, "video", link!);
        }

        var vid2 = await Page.QuerySelectorAllAsync<HtmlElement>("*[src$='.mp4']");
        foreach (var vidz in vid2)
        {
            var linkz = await vidz.GetAttributeAsync("src");
            System.Console.WriteLine(linkz);
            TrackingDb.InsertRecord(Path.GetFileNameWithoutExtension(linkz)!, "video", linkz!);
        }

        await Page.DisposeAsync();
        await Browser.DisposeAsync();



    }












    public async Task ScrapeBdsmlr(string url, CancellationToken stoppingToken)
    {
        Page.Dialog +=  (sender, e) =>
        {

          // await HandleDialogs(sender, e);
        };
        _logger.LogDebug("Dialog handler attached");

        if (!stoppingToken.IsCancellationRequested)
        {
            //await DoLogin(Page);


            _logger.LogDebug("Starting Navigation");
            _logger.LogDebug("Waiting for network idle");
            try
            {
                
              await  Page.GoToAsync(url);
                await Task.Delay(5000);
                await Page.EvaluateFunctionAsync("() => {window.scrollBy(0, 100)}");
                _logger.LogDebug("starting to search for video elements");
            }
            catch (System.Exception)
            {
                
            
            }
            await SearchVPlayer( stoppingToken);

        }

    }


/// <summary>
/// Method designed for scraping bdsmlr.com posts with scrolling.
/// </summary>
/// <param name="page"></param>
/// <param name="stoppingToken"></param>
/// <returns></returns>
    private async Task SearchVPlayer( CancellationToken stoppingToken)
    {
        // Scroll down slightly to ensure that scrollbars are shown. Some instances
        // the scrollbars are shown briefly before they disappear.
        await Page.EvaluateFunctionAsync("() => {window.scrollBy(0, 100)}");
        int pageCounter = 0;
        while (!stoppingToken.IsCancellationRequested && pageCounter < 15)
        {
            var elements = await Page.QuerySelectorAllAsync<HtmlDivElement>("div.wrap-post.typevideo");

            foreach (var vid in elements)
            {

                _logger.LogDebug("vid elements found count= {0}", elements.Length);

                var vids = await vid.QuerySelectorAsync<HtmlElement>("video.vjs-tech");
                if (vids == null) continue;
                var postid = await vids.GetAttributeAsync("value-id");
                // Get Child element which is a "source" element
                var source = await vids.GetFirstChildAsync();

                if (postid != null && source != null)
                {

                    var link = await source.GetNodeValueAsync();
                    Console.WriteLine(link);
                    TrackingDb.InsertRecord(postid, "video", link!);
                }

            }

            
            await Page.EvaluateExpressionAsync("window.scrollTo(0,document.body.scrollHeight)");
           // await Page.WaitForNavigationAsync(new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Load, WaitUntilNavigation.DOMContentLoaded } });
            await Task.Delay(4000);

            pageCounter++;


        }
        await Task.Delay(3000);
    }





    private async Task DoLogin(IPage page)
    {
        await page.GoToAsync("https://www.bdsmlr.com/login");
        await Task.Delay(5000);

        var element1 = await page.QuerySelectorAsync<HtmlElement>("input#email");
        if (element1 == null) return;
        await element1.TypeAsync("fetishmaster1969@gmail.com");

        var element2 = await page.QuerySelectorAsync<HtmlElement>("input#password");
        if (element2 == null) return;
        await element2.TypeAsync("Angel1031");

        await page.ClickAsync("button[type=submit]");
        await Task.Delay(5000);


    }





    private async Task HandleDialogs(object? sender, DialogEventArgs? e)
    {
        var dialog = e?.Dialog;
        if (dialog == null) return;


        switch (dialog.DialogType)
        {

            case PuppeteerSharp.DialogType.Alert:

                await dialog.Accept();
                break;

            case DialogType.Confirm:
                await dialog.Accept();
                break;

            case DialogType.Prompt:
                await dialog.Accept();
                break;

            case DialogType.BeforeUnload:
                await dialog.Accept();
                break;


        }

    }





}
