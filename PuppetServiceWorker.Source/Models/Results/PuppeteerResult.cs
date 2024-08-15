





namespace PuppeteerSharp.Models.Results
{
    public class PuppeteerResult(bool success) : IResult
    {

        public PuppeteerResult(bool success, string message = "") : this(success)
        {
            Message = message;
        }

        public bool Success { get; } = success;

        public string Message { get; }
    }
}
