using TamaGo.Services;
using TamaGo.Models;
using System.Collections.ObjectModel;

namespace TamaGo.Views;

public partial class FavoritesPage : ContentPage
{
    private readonly AuthService _authService;
    private readonly DestinationService _destinationService;
    private readonly TourService _tourService;
    
    public ObservableCollection<Destination> FavoriteDestinations { get; set; } = new();

    public FavoritesPage(AuthService authService, DestinationService destinationService, TourService tourService)
    {
        InitializeComponent();
        _authService = authService;
        _destinationService = destinationService;
        _tourService = tourService;
        NavigationPage.SetHasNavigationBar(this, false);
        
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadFavoritesAsync();
    }

    private async Task LoadFavoritesAsync()
    {
        try
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;
            MainContent.IsVisible = false;

            FavoriteDestinations.Clear();

            // For now, we'll show a placeholder since we don't have the FavoriteService implemented yet
            // This would normally load from a FavoriteService that queries the favorite table
            
            // Placeholder: Load some sample destinations as favorites
            // In a real implementation, this would be:
            // var (success, favorites, message) = await _favoriteService.GetUserFavoritesAsync();
            
            var (success, destinations, message) = await _destinationService.GetDestinationsAsync();
            if (success && destinations?.Any() == true)
            {
                // For demo purposes, show first 2 destinations as "favorites"
                var sampleFavorites = destinations.Take(2);
                foreach (var dest in sampleFavorites)
                {
                    FavoriteDestinations.Add(dest);
                }
            }

            // Update UI based on whether we have favorites
            if (FavoriteDestinations.Any())
            {
                EmptyState.IsVisible = false;
                FavoritesContent.IsVisible = true;
            }
            else
            {
                EmptyState.IsVisible = true;
                FavoritesContent.IsVisible = false;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading favorites: {ex.Message}");
            await DisplayAlert("Error", "No se pudieron cargar los favoritos", "OK");
            EmptyState.IsVisible = true;
            FavoritesContent.IsVisible = false;
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
            MainContent.IsVisible = true;
        }
    }

    async void OnDestinationTapped(object sender, TappedEventArgs e)
    {
        try
        {
            if (e.Parameter is Destination destination)
            {
                var destDetailPage = new DestinationDetailPage(destination);
                await Navigation.PushAsync(destDetailPage);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error navigating to destination: {ex.Message}");
            await DisplayAlert("Error", "No se pudo abrir el destino", "OK");
        }
    }

    async void OnRemoveFavoriteClicked(object sender, EventArgs e)
    {
        try
        {
            if (sender is Button button && button.CommandParameter is Destination destination)
            {
                var confirmed = await DisplayAlert(
                    "Remover Favorito", 
                    $"¿Deseas remover '{destination.Name}' de tus favoritos?", 
                    "Sí", 
                    "No");

                if (confirmed)
                {
                    // TODO: Implement actual favorite removal
                    // await _favoriteService.RemoveFavoriteAsync(destination.IdDestination);
                    
                    FavoriteDestinations.Remove(destination);
                    
                    // Update UI if no favorites left
                    if (!FavoriteDestinations.Any())
                    {
                        EmptyState.IsVisible = true;
                        FavoritesContent.IsVisible = false;
                    }
                    
                    await DisplayAlert("Éxito", "Favorito removido", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error removing favorite: {ex.Message}");
            await DisplayAlert("Error", "No se pudo remover el favorito", "OK");
        }
    }

    async void OnExploreDestinationsClicked(object sender, EventArgs e)
    {
        try
        {
            var destPage = new DestinationsPage(_destinationService, _tourService);
            await Navigation.PushAsync(destPage);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error navigating to destinations: {ex.Message}");
            await DisplayAlert("Error", "No se pudo abrir destinos", "OK");
        }
    }

    // Bottom navigation handlers
    async void OnToursTapped(object sender, TappedEventArgs e)
    {
        try
        {
            var toursPage = new ToursPage(_tourService, _authService);
            await Navigation.PushAsync(toursPage);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error navigating to tours: {ex.Message}");
            await DisplayAlert("Error", "No se pudo abrir tours", "OK");
        }
    }

    async void OnDestinationsTapped(object sender, TappedEventArgs e)
    {
        try
        {
            var destPage = new DestinationsPage(_destinationService, _tourService);
            await Navigation.PushAsync(destPage);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error navigating to destinations: {ex.Message}");
            await DisplayAlert("Error", "No se pudo abrir destinos", "OK");
        }
    }

    async void OnHomeTapped(object sender, TappedEventArgs e)
    {
        try
        {
            var menuPage = new MenuPage(_authService);
            await Navigation.PushAsync(menuPage);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error navigating to home: {ex.Message}");
            await DisplayAlert("Error", "No se pudo abrir inicio", "OK");
        }
    }

    async void OnAccountTapped(object sender, TappedEventArgs e)
    {
        try
        {
            var accountPage = new AccountPage(_authService);
            await Navigation.PushAsync(accountPage);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error navigating to account: {ex.Message}");
            await DisplayAlert("Error", "No se pudo abrir cuenta", "OK");
        }
    }
}