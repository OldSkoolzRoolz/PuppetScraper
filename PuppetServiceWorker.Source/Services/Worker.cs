using PuppetServiceWorker.Modules;
using PuppetServiceWorker.Data;



namespace PuppetServiceWorker;






public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IHostApplicationLifetime _appLifetime;

    public string StartingUrl { get; private set; } = string.Empty;

    private static readonly object VideoLinksLock = new object();


    public Worker(ILogger<Worker> logger, IHostApplicationLifetime appLifetime)
    {
        _logger = logger;
        _appLifetime = appLifetime;


    }

    private void OnStopping()
    {
        Console.WriteLine("Application Stopping..");
    }


    private void OnStarted()
    {
            _logger.LogDebug("Application Started -- OnStarted event starting");
   








        //InitiateDownloads(_appLifetime.ApplicationStopping).GetAwaiter().GetResult();
        //ImportData();

        _logger.LogDebug("Creating scraper object...");
        PuppetScrapers scraper = new PuppetScrapers(_logger);

        _logger.LogDebug("Starting scraper -- Scraping site: ");
        scraper.ScrapeBdsmlr(site2, _appLifetime.ApplicationStopping).GetAwaiter().GetResult();


        // scraper.SearchForVids("https://www.pornhub.com/video?c=10").GetAwaiter().GetResult();


_logger.LogDebug("Application Lifetime -- Stop Application triggered");
        _appLifetime.StopApplication();
    }






    private void ImportData()
    {
        var data = System.IO.File.ReadAllLines("FileListAbsolute.txt");

        TrackingDb.InsertRecords(data);
    }







    /*
        private async Task UseBrowserlessAsync()
        {




            var connectOptions = new ConnectOptions()
            {
                BrowserWSEndpoint = "$wss://chrome.browserless.io/"
            };

            using var browser = await Puppeteer.ConnectAsync(connectOptions);

            //  ...



        }
    */
    /*
        private async Task AddEventListenersAsync(IPage page)
        {
            page.Console += (sender, e) => Console.WriteLine($"CONSOLE: {e.Message.Text}");

            await page.ExposeFunctionAsync("onscrollend", () => Console.WriteLine("onscrollend was fired"));


            await page.EvaluateExpressionOnNewDocumentAsync(@"    window.addEventListener('scrollend', (e) => {        // Handle the event here onscrollend()        console.log('Event fired:', e.type, e.detail);    });");
            await page.EvaluateExpressionOnNewDocumentAsync(@"    window.addEventListener('resize', (e) => {        // Handle the event here   onscrollend     console.log('Event fired:', e.type, e.detail);    });");


        }


    */







    //     var wrapper = driver.FindElement(By.ClassName("vue-recycle-scroller__item-wrapper"));
    //   var items = wrapper.FindElements(By.ClassName("vue-recycle-scroller__item-view"));














    // Listen for console messages
    //    page.Console += (sender, e) => Console.WriteLine($"CONSOLE: {e.Message.Text}");

    // Listen for requests
    //   page.Request += (sender, e) => Console.WriteLine($"REQUEST: {e.Request.Url}");

    // Listen for responses
    //  page.Response += (sender, e) => Console.WriteLine($"RESPONSE: {e.Response.Url} - {e.Response.Status}");



    /*

        let scrollHeight = Math.max(
          document.body.scrollHeight, document.documentElement.scrollHeight,
          document.body.offsetHeight, document.documentElement.offsetHeight,
          document.body.clientHeight, document.documentElement.clientHeight
        );
        */


    //resize
    // div.vue-recycle-scroller__item-view

    // scroll
    // div.vue-recycle-scroller.scroller.ready.page-mode.direction-vertical













    /*

        //################################

        //################################
        //################################

        /// <summary>
        /// Parses the given URL and processes the content of the page.
        /// </summary>
        /// <param name="page">The page to navigate to and get content from.</param>
        /// <param name="address">The URL to navigate to.</param>
        /// <param name="stoppingToken">The cancellation token to handle cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        internal async Task ParseAndProcessUrlAsync(string address, CancellationToken stoppingToken)
        {
            var waitUntil = new WaitUntilNavigation[] { WaitUntilNavigation.Networkidle0, WaitUntilNavigation.Networkidle2, WaitUntilNavigation.DOMContentLoaded };
            var tmpObjects = new List<string>();
            var postDictionary = new Dictionary<string, string>();


            using var page = await PuppeteerMethods.OpenChromiumPage(false);

            await page.SetViewportAsync(new ViewPortOptions { Width = 1280, Height = 2500 });
            page.DefaultNavigationTimeout = 0;
            page.DefaultTimeout = 0;

            await AddEventListenersAsync(page);

            // Navigate to the URL and wait until the page has loaded and network is idle
            await page.GoToAsync(address, new NavigationOptions { WaitUntil = waitUntil });


            // Add-Listners to handle infinite scrolling
            // possible events resize, {bool}emitUpdate/update, scroll-start, scroll-end
            try
            {

                ElementHandle[] elementsArray = new ElementHandle[0];
                // TODO: While not scroll end
                for (var i = 0; i < 50; i++)  //hack scroller
                {
                    try
                    {
                        // Recycled scroller item wrapper
                        await page.WaitForSelectorAsync(".vue-recycle-scroller__item-wrapper");

                        var sheight = await page.EvaluateFunctionAsync<string>("() => window.getComputedStyle(document.querySelector('.vue-recycle-scroller__item-wrapper')).getPropertyValue('min-height')");

                        //Extract Item wrappers and grab their ElementHandles
                        var jsItemsSelector = @"Array.from(document.querySelectorAll('.vue-recycle-scroller__item-view'))";
                        var items = await page.EvaluateExpressionHandleAsync(jsItemsSelector);
                        var properties = await items.GetPropertiesAsync().ConfigureAwait(false);
                        elementsArray = properties.Values.OfType<ElementHandle>().ToArray();

                    }
                    catch (System.Exception e)
                    {

                        System.Console.WriteLine("Exception in outer loop");
                        System.Console.WriteLine(e.Message);
                        continue;
                    }
                    for (var x = 0; x < elementsArray.Length; x++)
                    {
                        try
                        {
                            //  Get Post type and filter accordingly
                            var postType = await page.EvaluateFunctionAsync<string>("e => e.querySelector('article').getAttribute('data-post-type')", elementsArray[x]);

                            //Extract the index of the data item -- we will use this to eliminate duplicate data extractions
                            // The number is generated within the context of this page ant the time of generation.
                            var jsidx = $"document.querySelectorAll('.vue-recycle-scroller__item-view > div')[{x}].getAttribute('data-index')";
                            var idx = await page.EvaluateExpressionAsync<string>(jsidx);
                            Console.WriteLine("idx: " + idx + "    postType: " + postType);
                            // If we have already collected this data skip
                            if (postDictionary.ContainsKey(idx)) continue;
                            if (postType != "video") continue;

                            // get video element for this article and the link
                            var videle = await page.EvaluateFunctionAsync<string>("e => e.querySelector('video').src", elementsArray[x]);
                            if (videle == null) continue;

                            tmpObjects.Add(videle);
                            Console.WriteLine("idx: " + idx + "    videle: " + videle);
                        }
                        catch (System.Exception ex)
                        {
                            System.Console.WriteLine(ex.Message);

                        }

                    }

                    await page.EvaluateFunctionAsync("() => window.scrollBy(0, document.body.scrollHeight)");
                    await Task.Delay(5000);
                }

            }
            finally
            {
                AppendToVideoLinks(tmpObjects);
            }
        }


    */




    //https://www.webshare.io/academy-article/puppeteer-scroll






















    /*



        /// <summary>
        /// Retrieves a list of video links from a given page by navigating through pagination.
        /// </summary>
        /// <param name="page">The page to extract the video links from.</param>
        /// <returns>A list of video links.</returns>
        public static async Task<List<string>> VideoLinkList(Page page)
        {
            // Define the navigation events to wait for before continuing
            WaitUntilNavigation[] waitUntil = new[]
            {
                WaitUntilNavigation.Networkidle0,
                WaitUntilNavigation.Networkidle2,
                WaitUntilNavigation.DOMContentLoaded,
                WaitUntilNavigation.Load
            };

            // Select the 'pnnext' element, which is used to navigate to the next page
            var next = @"document.getElementById('pnnext')";
            var nexts = await page.EvaluateExpressionAsync<object>(next);

            // Create a list to store the video links
            var linkList = new List<string>();
            var isLastPage = (nexts == null);

            // Continue navigating through pages until there are no more pages
            while ((nexts != null) || !isLastPage)
            {
                // Select all elements with the class 'g', which are likely the containers for the video links
                var jsSelectAllAnchors = @"Array.from(document.querySelectorAll('.g'))";
                var urls = await page.EvaluateExpressionAsync<object[]>(jsSelectAllAnchors);

                // Iterate through each selected element
                for (int i = 0; i < urls.Length; i++)
                {
                    // Construct a JavaScript query to select the 'href' attribute of the first 'a' element within the selected element
                    var query = $"document.querySelectorAll('.g')[{i}].getElementsByTagName('a')[0].href";
                    // Evaluate the JavaScript query and add the result to the list
                    linkList.Add(await page.EvaluateExpressionAsync<string>(query));
                }

                // Check if there is a 'pnnext' element, indicating that there may be more pages
                nexts = await page.EvaluateExpressionAsync<object>(next);
                if (nexts != null)
                {
                    // If there is a 'pnnext' element, navigate to the next page
                    var nextHref = @"document.getElementById('pnnext').href";
                    var nextHrefUrl = await page.EvaluateExpressionAsync<string>(nextHref);

                    // Check if there is a next page and if the 'href' attribute of the 'pnnext' element is not empty
                    isLastPage = (nexts == null) && !string.IsNullOrEmpty(nextHrefUrl);

                    // Navigate to the next page and wait for the defined navigation events to occur
                    await page.GoToAsync(nextHrefUrl, new NavigationOptions { WaitUntil = waitUntil });
                    nexts = await page.EvaluateExpressionAsync<object>(next);
                }
                else
                {
                    // If there is no 'pnnext' element, set 'isLastPage' to true to exit the loop
                    isLastPage = true;
                }
            }
            // Return the list of video links
            return linkList;
        }











        /*

            var properties = await elements.GetPropertiesAsync().ConfigureAwait(false);
            var elementsArray = properties.Values.OfType<ElementHandle>().ToArray();



        */



    /*







        private HtmlWeb GetHapWeb()
        {

            var hapweb = new HtmlWeb
            {
                AutoDetectEncoding = true,
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.142.86 Safari/537.36",
                AutomaticDecompression = System.Net.DecompressionMethods.All,
                Timeout = 100000,
                UseCookies = true,
            };

            return hapweb;


        }
    */
    /*


        /// <summary>
        /// Returns the launch options for the Puppeteer browser.
        /// </summary>
        /// <returns>The launch options.</returns>
        private PuppeteerSharp.LaunchOptions GetLaunchOptions()
        {
            // Set the executable path for the browser.
            var x = new LaunchOptions
            {
                ExecutablePath = @"/Storage/Apps/Browser/chrome-linux64/chrome",
                // Set the browser to run in headless mode.
                Headless = false,
                // Set the timeout for the browser.
                Timeout = 0,

                // Set the user data directory for the browser.
                UserDataDir = "/Storage/Data/ChromeData",
                Args = new[] { "--disable-gpu", "--disable-dev-shm-usage", "--disable-setuid-sandbox" },
                // Set the arguments for the browser.
                //  Args = new[] { "--disable-gpu", "--disable-dev-shm-usage", "--disable-setuid-sandbox",
                //      "--no-first-run", "--no-sandbox", "--no-zygote", "--single-process", "--disable-dev-shm-usage" },
                // Set the default viewport for the browser.
                DefaultViewport = new ViewPortOptions { Width = 1280, Height = 4024 }
            };

            return x;
        }




    */




    private bool AreBaseHostsTheSame(string url1, string url2)
    {
        Uri uri1 = new Uri(url1);
        Uri uri2 = new Uri(url2);
        return uri1.Host == uri2.Host;
    }





    /*

        /// <summary>
        /// Checks if a given URL is valid.
        /// </summary>
        /// <param name="url">The URL to be checked.</param>
        /// <returns>True if the URL is valid, false otherwise.</returns>
        private static bool IsValidUrl(string url)
        {
            // Try to create a URI from the given URL, specifying that it should be an absolute URI.
            // If successful, check if the URI's scheme is either HTTP or HTTPS.
            // If any of the above checks fail, return false. Otherwise, return true.
            Uri? uriResult;
            bool result = Uri.TryCreate(url, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            return result;
        }




    */



    /*

        /// <summary>
        /// Adds valid and unvisited links to the list of links to visit.
        /// </summary>
        /// <param name="links">The links to add.</param>
        private void AddLinksToVisit(IEnumerable<string> links)
        {
            if (links == null)
            {
                return;
            }

            foreach (var link in links)
            {
                if (AreBaseHostsTheSame(StartingUrl, link) && !_visitedlinks.Contains(link))
                {
                    _linksToVisit.Add(link);
                }
            }
        }



    **/



    /*

        /// <summary>
        /// Retrieves the links from the given HTML node collection.
        /// Only includes links that are valid URLs.
        /// </summary>
        /// <param name="nodes">The HTML node collection to retrieve links from.</param>
        /// <returns>A list of valid URLs.</returns>
        private static List<string> GetLinks(HtmlNodeCollection nodes)
        {
            // Return an empty list if the nodes are null
            if (nodes is null) return new List<string>();

            // Create a new list to store the links
            var alinks = new List<string>();

            // Iterate over each node in the collection
            foreach (var lnk in nodes)
            {
                // Get the href attribute value from the node
                var href = lnk.Attributes["href"].Value;

                // Check if the href is a valid URL
                if (IsValidUrl(href))
                {
                    // Add the href to the list of links
                    alinks.Add(href);
                }
            }

            // Return the list of links
            return alinks;
        }

    */


    /*




        /// <summary>
        /// Retrieves the video links from the given HTML node collection.
        /// </summary>
        /// <param name="nodes">The HTML node collection to retrieve video links from.</param>
        /// <returns>A list of video links.</returns>
        private static List<string> GetVidLinks(HtmlNodeCollection nodes)
        {
            // Return an empty list if the nodes are null
            if (nodes is null) return new List<string>();

            // Create a new list to store the video links
            var vlinks = new List<string>();

            // Iterate over each node in the collection
            foreach (var vid in nodes)
            {
                // Get the src attribute value from the node
                var src = vid.Attributes["src"].Value;

                // Add the src to the list of video links
                vlinks.Add(src);
            }

            // Return the list of video links
            return vlinks;
        }





    */








    /// <summary>
    /// Appends the given list of video links to the "videoLinks.txt" file.
    /// </summary>
    /// <param name="links">The list of video links to append.</param>
    public static void AppendToVideoLinks(List<string> links)
    {
        // Lock the VideoLinksLock object to ensure thread safety
        lock (VideoLinksLock)
        {
            // Open the "videoLinks.txt" file in append mode
            using (StreamWriter writer = System.IO.File.AppendText("videoLinks.txt"))
            {
                // Iterate over each link in the list
                foreach (string link in links)
                {
                    // Write the link to the file on a new line
                    if (string.IsNullOrEmpty(link) == false)
                    {
                        writer.WriteLine(link);
                    }
                }
            }
        }
    }


    protected async Task InitiateDownloads(CancellationToken stoppingToken)
    {


        // using var scraper = new PuppetScrapers();
        // await scraper.StartScrapingAsync(stoppingToken).ConfigureAwait(false);
        Downloader downloader = new Downloader(_appLifetime, "/Extra/Files", 5, stoppingToken);
        await downloader.ActivateDownloader().ConfigureAwait(false);

    }




    protected async Task ActivateScraper(CancellationToken stoppingToken)
    {

        TrackingDb.LoadSeenPosts();
        using var scraper = new PuppetScrapers(_logger);
        await scraper.StartScrapingAsync(stoppingToken).ConfigureAwait(false);


    }




    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(1000, stoppingToken);


        _logger.LogDebug("Worker running-- ExecuteAsync starting");
        _appLifetime.ApplicationStarted.Register(OnStarted);
        _appLifetime.ApplicationStopping.Register(OnStopping);


        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                Console.WriteLine("Worker running");
                await Task.Delay(15000, stoppingToken);
            }
            catch (TaskCanceledException)
            {


            }

        }

    }














}
