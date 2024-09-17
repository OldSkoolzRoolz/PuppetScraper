using System.Collections.Concurrent;
using System.Diagnostics;

using PuppetScraper.Data;
using PuppetScraper.Models;

namespace PuppetScraper.Modules;



public interface IDownloader
{

    Task StartDownloadingAsync(CancellationToken stoppingToken);
    Task DownloadFile(string id, string url, CancellationToken token);

}




public class Downloader:IDownloader
{


    private readonly CancellationToken _token;
    private readonly string _downloadPath;
    private readonly int _concurrentTasks;
    private readonly ConcurrentDictionary<string, string> _targetList = new();

   private readonly HttpClient _client = new HttpClient()
    {
        Timeout = TimeSpan.FromSeconds(360),
        DefaultRequestHeaders = { { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.142.86 Safari/537.36" } }
    };


   public CancellationToken Token => _token;
public string DownloadPath => _downloadPath;
public int ConcurrentTasks => _concurrentTasks;

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
    /// <param name="concurrentTasks"></param>
    /// <param name="stoppingToken">
    /// The <see cref="CancellationToken"/> used to stop the download tasks gracefully.
    /// </param>
    /// <param name="downloadPath">
    /// The directory where the downloaded files will be saved.
    /// </param>
    public Downloader(string downloadPath = "/Extra/Files", int concurrentTasks = 5, CancellationToken stoppingToken = default)
    {
       
        // Stores the cancellation token to stop the download tasks gracefully.
        _token = stoppingToken;

        // Stores the directory where the downloaded files will be saved.
        _downloadPath = downloadPath;

        _concurrentTasks = concurrentTasks;
        DownloadComplete += OnDownloadComplete;
        DownloadTaskCompleted += OnDownloadTaskCompleted;
        

        // Loads a number of unseen posts from the "Tracker" table into a ConcurrentDictionary.
        _targetList = TrackingDb.LoadUnSeenPostsAsync(50).Result;
    }

    private void OnDownloadTaskCompleted(object? sender, EventArgs e)
    {
        System.Console.WriteLine("Download Task Completed");
      
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
    /// <param name="sender"></param>
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














    public async Task StartDownloadsAsync()
    {
        // Check if the number of concurrent tasks is valid
        if (_concurrentTasks <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(_concurrentTasks), "Number of concurrent tasks must be greater than 0.");
        }

        // Create a linked list to store the tasks
        var tasks = new LinkedList<Task>();

        // Iterate over the number of concurrent tasks
        for (int i = 0; i < _concurrentTasks; i++)
        {
            // Create a new task and add it to the linked list
            var task = StartDownloadingAsync(Token);
            tasks.AddLast(task);
        }

        try
        {
            // Keep running until all tasks are completed
            while (tasks.Count > 0)
            {
                // Await the completion of the first task in the linked list
                var completedTask = await Task.WhenAny(tasks);

                // Remove the completed task from the linked list
                tasks.Remove(completedTask);

                // If the task threw an exception, print an error message to the console
                if (completedTask.Exception != null)
                {
                    foreach (var ex in completedTask.Exception.InnerExceptions)
                    {
                        Console.WriteLine($"Error occurred during download task: {ex.Message}");
                    }
                }

                // If there are still tasks to be completed, create a new concurrent task and add it to the linked list
                if (tasks.Count < _concurrentTasks)
                {
                    var newTask = StartDownloadingAsync(Token);
                    tasks.AddLast(newTask);
                }
            }
        }
        finally
        {
            // Raise the DownloadTaskCompleted event to notify subscribers that all download tasks have completed
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

        Stream? stream = null;
        FileStream? fileStream = null;

        try
        {
            using var response = await _client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, stoppingToken);

            if (response.IsSuccessStatusCode)
            {
                stream = await response.Content.ReadAsStreamAsync(stoppingToken);

                if (stream == null)
                {
                    throw new InvalidOperationException($"Failed to read stream from {url}");
                }

                var path = Path.Combine(_downloadPath, Path.GetFileName(url));

                fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

                if (fileStream == null)
                {
                    throw new InvalidOperationException($"Failed to open file stream for {path}");
                }

                await stream.CopyToAsync(fileStream, 8192, stoppingToken);

                args.BytesTransferred = new FileInfo(path).Length;
                args.BytesExpected = response.Content.Headers.ContentLength;
            }
            else
            {
                args.Exception = new FatalDownloadThreadException(response.ReasonPhrase);
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
            stream?.Dispose();
            fileStream?.Dispose();
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
        while(!_targetList.IsEmpty)
        {
        

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
        
        Debug.WriteLine("Download Thread Complete");

    }
}
