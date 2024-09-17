


namespace PuppetScraper.Sample.Services;






public interface IClipboardService
{
    void SetText(string text);
}

public class ClipboardService : IClipboardService
{
    // TODO: Implement Clipboard monitoring to add support for pasting
    public void SetText(string text)
    {
        throw new System.NotImplementedException();
    }

}
