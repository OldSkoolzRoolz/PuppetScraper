using PuppeteerSharp;


using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;


namespace PuppetScraper;

public class PuppetPageBase : PuppetBrowserContextBase
{


    protected IPage Page { get; set; }


    public PuppetPageBase()
    {

        CreatePage();
    }

    [MemberNotNull(nameof(Page))]
    private void CreatePage()
    {
        IPage[] tmp = Context.PagesAsync().GetAwaiter().GetResult();

        // if tmp is null or empty, create a new page
        if (tmp.Length == 0)
        {
            Page = Context.NewPageAsync().GetAwaiter().GetResult() ?? throw new Exception("Failed to create new page");
        }
        else
        {
            // TODO: If tmp contains more than one page, set the Page property to index 0 and close any other pages.
            Page = tmp[0];

            for (var i = 1; i < tmp.Length; i++)
            {
                tmp[i].CloseAsync().GetAwaiter().GetResult();
            }
        }

        Page.DefaultTimeout = Debugger.IsAttached ? PuppetSettings.DebuggerAttachedTestTimeout : PuppetSettings.DefaultPuppeteerTimeout;
    }






    public async Task ClosePageAsync()
    {
        if (Page is not null)
        {
            await Page.CloseAsync();
        }
    }

    protected Task WaitForError()
    {
        var wrapper = new TaskCompletionSource<bool>();


            void ErrorEvent(object? sender, PuppeteerSharp.ErrorEventArgs e)
            {
                wrapper.SetResult(true);
                Page.Error -= ErrorEvent;
            }

             Page!.Error += ErrorEvent;

        return wrapper.Task;
    }






}
