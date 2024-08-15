







using PuppeteerSharp;

namespace PuppetServiceWorker;




public static class PuppetSettings
{
    public static int DebuggerAttachedTestTimeout { get; internal set; } = 300_000;
    public static int DefaultPuppeteerTimeout { get; internal set; } = 30_000;
    public static ILoggerFactory LoggerFactory { get; internal set; } = new LoggerFactory();


    internal static LaunchOptions DefaultBrowserOptions()
    {
       var lo= new LaunchOptions
        {
            Headless = false,
            Timeout = 0,
            ExecutablePath = @"/Storage/Apps/Browser/chrome-linux64/chrome",
            Args =  new[]{"--no-incognito", "--disable-features=PreloadMediaEngagementData, MediaEngagementBypassAutoplayPolicies"},
            UserDataDir = "/Storage/Data/ChromeData",
            DefaultViewport = new ViewPortOptions{ Width=1280, Height=8000 },
            
        };
        return lo;
    }

}