using Microsoft.Extensions.Logging;
using MiDeuda.Core; // Importa tu librería en VB
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

        // 1. Definir la ruta multiplataforma de la base de datos
        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "mideuda.db");

        // 2. Registrar el servicio de base de datos de MiDeuda.Core
        builder.Services.AddSingleton<DatabaseService>(s => ActivatorUtilities.CreateInstance<DatabaseService>(s, dbPath));

        // 3. Registrar las vistas que consumirán el servicio
        builder.Services.AddTransient<MainPage>();

        return builder.Build();
    }
}