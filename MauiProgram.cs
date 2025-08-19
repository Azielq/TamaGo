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
        builder.Services.AddSingleton<DestinationService>();
        builder.Services.AddSingleton<TourService>();
        
        // Registrar App
        builder.Services.AddSingleton<App>();
        
        // Registrar páginas
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<MenuPage>();
        builder.Services.AddTransient<DestinationsPage>();
        builder.Services.AddTransient<BookingsPage>();
        
        // Registrar ViewModels
        builder.Services.AddTransient<ViewModels.DestinationsPageViewModel>();
        builder.Services.AddTransient<ViewModels.DestinationDetailPageViewModel>();
        builder.Services.AddTransient<ViewModels.TourDetailPageViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}