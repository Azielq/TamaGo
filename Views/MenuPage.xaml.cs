using TamaGo.Services;

namespace TamaGo.Views;

public partial class MenuPage : ContentPage
{
    private readonly AuthService _auth;
    
    public MenuPage(AuthService auth)
    {
        InitializeComponent();
        _auth = auth;
        NavigationPage.SetHasNavigationBar(this, false);
    }

    async void OnExploreTapped(object sender, TappedEventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("Starting navigation to destinations...");
            
            // Resolver servicios individualmente
            var destinationService = App.ServiceProvider.GetRequiredService<DestinationService>();
            var tourService = App.ServiceProvider.GetRequiredService<TourService>();
            
            // Crear la página directamente
            var destPage = new DestinationsPage(destinationService, tourService);
            
            System.Diagnostics.Debug.WriteLine("DestinationsPage created successfully");
            await Navigation.PushAsync(destPage);
            System.Diagnostics.Debug.WriteLine("Navigation completed successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error navigating to destinations: {ex}");
            await DisplayAlert("Error", $"No se pudo abrir la página de destinos: {ex.Message}", "OK");
        }
    }

    async void OnBookingsTapped(object sender, TappedEventArgs e)
    {
        try
        {
            var bookingsPage = App.ServiceProvider.GetRequiredService<BookingsPage>();
            await Navigation.PushAsync(bookingsPage);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error navigating to bookings: {ex.Message}");
            await DisplayAlert("Error", "No se pudo abrir la página de reservas. Intenta nuevamente.", "OK");
        }
    }

    async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool answer = await DisplayAlert("Cerrar Sesión", "¿Estás seguro que deseas cerrar sesión?", "Sí", "No");
        if (answer)
        {
            _auth.Logout();
            await Navigation.PopToRootAsync();
        }
    }
}