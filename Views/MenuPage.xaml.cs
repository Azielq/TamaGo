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
        var destPage = App.ServiceProvider.GetRequiredService<DestinationsPage>();
        await Navigation.PushAsync(destPage);
    }

    async void OnBookingsTapped(object sender, TappedEventArgs e)
    {
        var bookingsPage = App.ServiceProvider.GetRequiredService<BookingsPage>();
        await Navigation.PushAsync(bookingsPage);
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