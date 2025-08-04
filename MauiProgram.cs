// 3. MauiProgram.cs - Configuración de la aplicación
using Microsoft.Extensions.Logging;
using TamaGo.Services;
using TamaGo.Views;

namespace TamaGo;

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

        // Registrar servicios
        builder.Services.AddSingleton<SupabaseService>();
        builder.Services.AddSingleton<AuthService>();
        
        // Registrar App
        builder.Services.AddSingleton<App>();
        
        // Registrar páginas
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<HomePage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}