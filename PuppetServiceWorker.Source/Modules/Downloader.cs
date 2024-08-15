using System.Diagnostics;



using System.Collections.Concurrent;


using PuppetServiceWorker.Data;
using PuppetServiceWorker.Models;

namespace PuppetServiceWorker.Modules;

public class Downloader
{
    private readonly IHostApplicationLifetime _appLifetime;

    private readonly CancellationToken _token;
    private readonly string _downloadPath;
    private readonly int _concurrentTasks;
    private readonly ConcurrentDictionary<string, string> _targetList = new ConcurrentDictionary<string, string>();

    readonly HttpClient _client = new HttpClient()
    {
        Timeout = TimeSpan.FromSeconds(360),
        DefaultRequestHeaders = { { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.142.86 Safari/537.36" } }
    };


    /// <summary>
    /// Event raised when a single download task is completed.
    /// </summary>
    public static event EventHandler<DownloadCompleteArgs>? DownloadComplete;

    /// <summary>
    /// Event raised when all the download tasks are completed.
    /// </summary>
    public static event EventHandler? DownloadTaskCompleted;






    /// <summary>
    /// Initializes a new instance of the <see cref="Downloader"/> class.
    /// </summary>
    /// <param name="stoppingToken">
    /// The <see cref="CancellationToken"/> used to stop the download tasks gracefully.
    /// </param>
    /// <param name="downloadPath">
    /// The directory where the downloaded files will be saved.
    /// </param>
    public Downloader(IHostApplicationLifetime hostApplicationLifetime,string downloadPath = "/Extra/Files", int concurrentTasks = 5, CancellationToken stoppingToken = default)
    {
        _appLifetime = hostApplicationLifetime;
        // Stores the cancellation token to stop the download tasks gracefully.
        _token = stoppingToken;

        // Stores the directory where the downloaded files will be saved.
        _downloadPath = downloadPath;

        _concurrentTasks = concurrentTasks;
        DownloadComplete += OnDownloadComplete;
        DownloadTaskCompleted += OnDownloadTaskCompleted;
        

        // Loads a number of unseen posts from the "Tracker" table into a ConcurrentDictionary.
        _targetList = TrackingDb.LoadUnSeenPostsAsync(200).Result;
    }

    private void OnDownloadTaskCompleted(object? sender, EventArgs e)
    {
        System.Console.WriteLine("Download Task Completed");
        _appLifetime.StopApplication();
    }





    /// <summary>
    /// Raises the <see cref="DownloadComplete"/> event.
    /// </summary>
    /// <param name="args">The <see cref="DownloadCompleteArgs"/> containing the information about the completed download.</param>
    private void RaiseDownloadComplete(DownloadCompleteArgs args)
    {
        // Raise the event to notify the subscribers that a download task is completed.
        // The event handler receives a reference to the current instance of the Downloader class 
        // and the DownloadCompleteArgs object that contains the information about the completed download.

        DownloadComplete?.Invoke(this, args);
    }









    /// <summary>
    /// This method is called when a download task is completed. 
    /// It updates the seen status of the downloaded item in the database.
    /// </summary>
    /// <param name="args">The <see cref="DownloadCompleteArgs"/> containing the information about the completed download.</param>
    public virtual void OnDownloadComplete(object? sender, DownloadCompleteArgs args)
    {
        // Update the seen status of the downloaded item in the database.
        // The item is identified by its ID.
        if (args.Exception == null) { 
            if(args.BytesExpected == args.BytesTransferred)
            {
            TrackingDb.UpdateSeenStatus(args.Id); }
            }

        System.Console.WriteLine(args.Exception?.Message);
        System.Console.WriteLine("Bytes Expected: " + args.BytesExpected + " Bytes Downloaded: " + args.BytesTransferred);


    }





    /// <summary>
    /// Activates the downloader by starting multiple download tasks concurrently.
    /// </summary>
    /// <returns>A task that represents the asynchronous activation of the downloader.</returns>
    public async Task ActivateDownloader()
    {
        if (_concurrentTasks <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(_concurrentTasks), "Number of concurrent tasks must be greater than 0.");
        }

        var tasks = new Task[_concurrentTasks];
        for (int i = 0; i < _concurrentTasks; i++)
        {
            tasks[i] = StartDownloadingAsync(_token);
        }

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error occurred during download tasks: {ex.Message}");
            throw;
        }
        finally
        {
            DownloadTaskCompleted?.Invoke(this, EventArgs.Empty);
        }
    }











    public async Task DownloadFile(string id, string url, CancellationToken stoppingToken)
    {
        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(url))
            return;

        var args = new DownloadCompleteArgs(id, url);

        if (stoppingToken.IsCancellationRequested)
            return;

        try
        {
            using var response = await _client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, stoppingToken);

            if (response.IsSuccessStatusCode)
            {
                using var stream = await response.Content.ReadAsStreamAsync(stoppingToken);
                var path = Path.Combine(_downloadPath, Path.GetFileName(url));

                using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write,FileShare.None,8192,true))
                {
                    await stream.CopyToAsync(fileStream, 8192, stoppingToken);
                }

                args.BytesTransferred = new FileInfo(path).Length;
                args.BytesExpected = response.Content.Headers.ContentLength;
            }
        }
        catch (TaskCanceledException tce)
        {
            args.Exception = tce;
        }
        catch (Exception ex)
        {
            args.Exception = ex;
        }
        finally
        {
            RaiseDownloadComplete(args);
        }
    }






    /// <summary>
    /// Downloads files from the targetList concurrently until the cancellation token is cancelled or the targetList is empty.
    /// </summary>
    /// <param name="stoppingToken">The cancellation token to stop the download tasks gracefully.</param>
    /// <returns></returns>
    public async Task StartDownloadingAsync(CancellationToken stoppingToken)
    {
        do
        {
            if (_targetList.IsEmpty)
            {
                Console.WriteLine("Download List Empty. Exiting.");
                return;
            } // works around bug in ConcurrentDictionary

            var target = _targetList.First();
            _targetList.TryRemove(target.Key, out _);
            try
            {

                await DownloadFile(target.Key, target.Value, stoppingToken);
            }
            catch (Exception)
            {
                // We do not want to stop download threads if an exception occurs.
                // So exception is handled internally and 
                //we should never see this exception.
                throw new FatalDownloadThreadException();
            }

        }
        while (!stoppingToken.IsCancellationRequested || !_targetList.IsEmpty);
        // Possible bug in ConcurrentDictionary
        // while does not exit if targetList is empty.        
        Debug.WriteLine("Download Thread Complete");

    }
}
