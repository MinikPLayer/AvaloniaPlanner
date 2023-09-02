using Avalonia;
using Avalonia.Browser;
using Avalonia.ReactiveUI;
using AvaloniaPlanner;
using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;

[assembly: SupportedOSPlatform("browser")]

internal partial class Program
{
    private static Task Main(string[] args)
    {
        try
        {
            return BuildAvaloniaApp()
                .UseReactiveUI()
                .StartBrowserAppAsync("out");
        }
        catch(Exception e)
        {
            throw e;
        }

    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}