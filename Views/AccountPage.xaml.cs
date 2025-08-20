using TamaGo.Services;

namespace TamaGo.Views;

public partial class AccountPage : ContentPage
{
    private readonly AuthService _authService;
    
    public AccountPage(AuthService authService)
    {
        InitializeComponent();
        _authService = authService;
        LoadUserInfo();
    }

    private void LoadUserInfo()
    {
        var currentUser = _authService.CurrentUser;
        if (currentUser != null)
        {
            UserNameLabel.Text = currentUser.Username ?? "Usuario";
            UserEmailLabel.Text = currentUser.Email ?? "usuario@ejemplo.com";
        }
    }

    async void OnProfileTapped(object sender, TappedEventArgs e)
    {
        try
        {
            var tourService = App.ServiceProvider.GetRequiredService<TourService>();
            var profilePage = new ProfilePage(_authService, tourService);
            await Navigation.PushAsync(profilePage);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error navigating to profile: {ex.Message}");
            await DisplayAlert("Error", "No se pudo abrir el perfil. Intenta nuevamente.", "OK");
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

    async void OnFavoritesTapped(object sender, TappedEventArgs e)
    {
        try
        {
            var destinationService = App.ServiceProvider.GetRequiredService<DestinationService>();
            var tourService = App.ServiceProvider.GetRequiredService<TourService>();
            var favoritesPage = new FavoritesPage(_authService, destinationService, tourService);
            await Navigation.PushAsync(favoritesPage);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error navigating to favorites: {ex.Message}");
            await DisplayAlert("Error", "No se pudo abrir favoritos. Intenta nuevamente.", "OK");
        }
    }

    async void OnSettingsTapped(object sender, TappedEventArgs e)
    {
        await DisplayAlert("Configuración", "Funcionalidad de configuración próximamente disponible.", "OK");
    }

    async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool answer = await DisplayAlert("Cerrar Sesión", "¿Estás seguro que deseas cerrar sesión?", "Sí", "No");
        if (answer)
        {
            _authService.Logout();
            await Navigation.PopToRootAsync();
        }
    }
}