using System;
using System.Diagnostics.CodeAnalysis;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using Microsoft.Extensions.DependencyInjection;

using PuppetScraper.Sample.Services;
using PuppetScraper.Sample.Views;

namespace PuppetScraper.Sample;

public partial class App : Application
{
    
    [MemberNotNull(nameof(Services))]
    public override void Initialize()
    {

        Services = ConfigureServices();

        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    /// Gets the current <see cref="App"/> instance in use
    /// </summary>
    public new static App Current => Application.Current as App ?? throw new InvalidOperationException("The application is not running");

    /// <summary>
    /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
    /// </summary>
    public ServiceProvider? Services { get; set; }

    /// <summary>
    /// Configures the services for the application.
    /// </summary>
    private static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IFilesService, FilesService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IClipboardService, ClipboardService>();
        services.AddSingleton<IDataLayerService, DataLayerService>();
       

        return services.BuildServiceProvider();
    }











    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}