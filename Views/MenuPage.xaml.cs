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
        LoadEmbeddedMap();
    }

    private void LoadEmbeddedMap()
    {
        var mapHtml = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body { margin: 0; padding: 0; font-family: Arial, sans-serif; }
        .map-container { width: 100%; height: 100%; }
        iframe { width: 100%; height: 100%; border: none; border-radius: 8px; }
    </style>
</head>
<body>
    <div class='map-container'>
        <iframe src='https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d15702.045940198905!2d-85.83806990000001!3d10.3008886!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x8f9e39409203c30f%3A0xbb189f5e2cc1f893!2sGuanacaste%20Province%2C%20Tamarindo!5e0!3m2!1sen!2scr!4v1755714174246!5m2!1sen!2scr' 
                allowfullscreen='' 
                loading='lazy' 
                referrerpolicy='no-referrer-when-downgrade'>
        </iframe>
    </div>
</body>
</html>";

        MapWebView.Source = new HtmlWebViewSource 
        { 
            Html = mapHtml 
        };
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
            var tourService = App.ServiceProvider.GetRequiredService<TourService>();
            var authService = App.ServiceProvider.GetRequiredService<AuthService>();
            var bookingsPage = new BookingsPage(tourService, authService);
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

    async void OnToursTapped(object sender, TappedEventArgs e)
    {
        try
        {
            var tourService = App.ServiceProvider.GetRequiredService<TourService>();
            var authService = App.ServiceProvider.GetRequiredService<AuthService>();
            var toursPage = new ToursPage(tourService, authService);
            await Navigation.PushAsync(toursPage);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error navigating to tours: {ex.Message}");
            await DisplayAlert("Error", "No se pudo abrir la página de tours. Intenta nuevamente.", "OK");
        }
    }

    async void OnAccountTapped(object sender, TappedEventArgs e)
    {
        try
        {
            var accountPage = new AccountPage(_auth);
            await Navigation.PushAsync(accountPage);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error navigating to account: {ex.Message}");
            await DisplayAlert("Error", "No se pudo abrir la página de cuenta. Intenta nuevamente.", "OK");
        }
    }

    async void OnViewMapClicked(object sender, EventArgs e)
    {
        try
        {
            // Coordenadas de Tamarindo, Costa Rica
            var location = new Location(10.2999, -85.8393);
            var options = new MapLaunchOptions
            { 
                Name = "Tamarindo, Costa Rica",
                NavigationMode = NavigationMode.None
            };
            
            await Map.Default.OpenAsync(location, options);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error opening map: {ex.Message}");
            await DisplayAlert("Error", "No se pudo abrir el mapa. Intenta nuevamente.", "OK");
        }
    }

    async void OnGetDirectionsClicked(object sender, EventArgs e)
    {
        try
        {
            // Coordenadas de Tamarindo, Costa Rica
            var location = new Location(10.2999, -85.8393);
            var options = new MapLaunchOptions
            {
                Name = "Tamarindo, Costa Rica",
                NavigationMode = NavigationMode.Driving
            };
            
            await Map.Default.OpenAsync(location, options);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error opening directions: {ex.Message}");
            await DisplayAlert("Error", "No se pudo abrir las direcciones. Intenta nuevamente.", "OK");
        }
    }
}