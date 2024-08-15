







namespace PuppetServiceWorker;

public class Program
{
    public static async Task Main(string[] args)
    {


         IHostBuilder builder = Host.CreateDefaultBuilder(args);
        
        builder.ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<Worker>();
                services.AddLogging().Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Debug);    
            })
            .UseContentRoot(Directory.GetCurrentDirectory())
            .UseEnvironment("Development");

            

       using IHost host = builder.Build();
        await host.RunAsync();
            


       
    }







}