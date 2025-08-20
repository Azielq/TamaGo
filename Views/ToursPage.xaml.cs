using TamaGo.Models;
using TamaGo.Services;
using System.Collections.ObjectModel;

namespace TamaGo.Views;

public partial class ToursPage : ContentPage
{
    private readonly TourService _tourService;
    private readonly AuthService _authService;
    public ObservableCollection<Tour> Tours { get; set; } = new();

    public ToursPage(TourService tourService, AuthService authService)
    {
        InitializeComponent();
        _tourService = tourService;
        _authService = authService;
        BindingContext = this;
        NavigationPage.SetHasNavigationBar(this, false);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadToursAsync();
    }

    private async Task LoadToursAsync()
    {
        try
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;
            MainContent.IsVisible = false;

            Tours.Clear();

            // Try to get tours from different destinations
            var destinationIds = new[] { 1, 2, 3, 4, 5 }; // Common destination IDs
            var allTours = new List<Tour>();

            foreach (var destId in destinationIds)
            {
                try
                {
                    var (success, tours, message) = await _tourService.GetToursByDestinationAsync(destId);
                    if (success && tours?.Any() == true)
                    {
                        allTours.AddRange(tours);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading tours for destination {destId}: {ex.Message}");
                }
            }

            // Remove duplicates based on IdTour
            var uniqueTours = allTours.GroupBy(t => t.IdTour).Select(g => g.First()).ToList();

            if (uniqueTours.Any())
            {
                foreach (var tour in uniqueTours.OrderBy(t => t.Title))
                {
                    Tours.Add(tour);
                }
                
                NoToursMessage.IsVisible = false;
                ToursCollectionView.IsVisible = true;
            }
            else
            {
                NoToursMessage.IsVisible = true;
                ToursCollectionView.IsVisible = false;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading tours: {ex.Message}");
            await DisplayAlert("Error", "No se pudieron cargar los tours. Intenta nuevamente.", "OK");
            NoToursMessage.IsVisible = true;
            ToursCollectionView.IsVisible = false;
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
            MainContent.IsVisible = true;
        }
    }

    async void OnTourTapped(object sender, TappedEventArgs e)
    {
        try
        {
            if (e.Parameter is Tour selectedTour)
            {
                var tourDetailPage = new TourDetailPage(selectedTour);
                await Navigation.PushAsync(tourDetailPage);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error navigating to tour detail: {ex.Message}");
            await DisplayAlert("Error", "No se pudo abrir el detalle del tour.", "OK");
        }
    }
}