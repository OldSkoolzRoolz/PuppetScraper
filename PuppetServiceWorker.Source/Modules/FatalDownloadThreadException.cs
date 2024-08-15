





namespace PuppetServiceWorker.Modules;

[Serializable]
internal class FatalDownloadThreadException : Exception
{
    public FatalDownloadThreadException()
    {
    }

    public FatalDownloadThreadException(string? message) : base(message)
    {
    }

    public FatalDownloadThreadException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}