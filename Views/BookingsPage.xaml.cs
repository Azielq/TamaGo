using TamaGo.ViewModels;
using TamaGo.Services;

namespace TamaGo.Views;

public partial class BookingsPage : ContentPage
{
    private readonly BookingsPageViewModel _viewModel;

    public BookingsPage(TourService tourService, AuthService authService)
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
        
        _viewModel = new BookingsPageViewModel(tourService, authService);
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        try
        {
            await _viewModel.LoadBookingsAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading bookings on appearing: {ex.Message}");
            await DisplayAlert("Error", "Error al cargar las reservas", "OK");
        }
    }

    async void OnExploreToursClicked(object sender, EventArgs e)
    {
        try
        {
            var destPage = App.ServiceProvider.GetRequiredService<DestinationsPage>();
            await Navigation.PushAsync(destPage);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error navigating to destinations: {ex.Message}");
            await DisplayAlert("Error", "Error al navegar a destinos", "OK");
        }
    }

    private void OnTabSelected(object sender, TappedEventArgs e)
    {
        if (sender is Border border && border.BindingContext is string tabName)
        {
            _viewModel.SelectedTab = tabName;
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

    async void OnDestinationsTapped(object sender, TappedEventArgs e)
    {
        try
        {
            var destinationService = App.ServiceProvider.GetRequiredService<DestinationService>();
            var tourService = App.ServiceProvider.GetRequiredService<TourService>();
            var destPage = new DestinationsPage(destinationService, tourService);
            await Navigation.PushAsync(destPage);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error navigating to destinations: {ex.Message}");
            await DisplayAlert("Error", "No se pudo abrir la página de destinos. Intenta nuevamente.", "OK");
        }
    }

    async void OnHomeTapped(object sender, TappedEventArgs e)
    {
        try
        {
            var authService = App.ServiceProvider.GetRequiredService<AuthService>();
            var menuPage = new MenuPage(authService);
            await Navigation.PushAsync(menuPage);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error navigating to home: {ex.Message}");
            await DisplayAlert("Error", "No se pudo abrir la página de inicio. Intenta nuevamente.", "OK");
        }
    }

    async void OnAccountTapped(object sender, TappedEventArgs e)
    {
        try
        {
            var authService = App.ServiceProvider.GetRequiredService<AuthService>();
            var accountPage = new AccountPage(authService);
            await Navigation.PushAsync(accountPage);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error navigating to account: {ex.Message}");
            await DisplayAlert("Error", "No se pudo abrir la página de cuenta. Intenta nuevamente.", "OK");
        }
    }
}