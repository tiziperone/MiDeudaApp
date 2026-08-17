using Microsoft.Extensions.Logging;
using MiDeuda.Core;
using System.IO;

namespace MiDeuda.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "mideuda.db");

        builder.Services.AddSingleton<DatabaseService>(s => ActivatorUtilities.CreateInstance<DatabaseService>(s, dbPath));

        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<FacturasPage>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<FacturasPage>();
        builder.Services.AddTransient<ResumenPage>();
        return builder.Build();
    }
}