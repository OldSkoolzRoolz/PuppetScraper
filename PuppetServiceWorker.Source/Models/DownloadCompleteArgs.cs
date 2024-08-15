namespace PuppetServiceWorker.Models;

public class DownloadCompleteArgs : EventArgs
{
/// <summary>
/// The ID of the downloaded object. For internal tracking only
/// </summary>
    public string Id { get; set; }



/// <summary>
/// The URL of the downloaded object
/// </summary>
public string Url { get; set; }



/// <summary>
/// The number of bytes transferred during the download
/// </summary>
public long BytesTransferred { get; set; }

/// <summary>
/// The exception that caused the download to fail. Null if the download succeeded
/// </summary>
public Exception? Exception { get; set; }
    public long? BytesExpected { get; internal set; }

    public DownloadCompleteArgs(string id, string url) => (Id, Url) = (id, url);


}
