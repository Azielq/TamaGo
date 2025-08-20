using TamaGo.Models;
using TamaGo.ViewModels;
using TamaGo.Services;
using Microsoft.Extensions.DependencyInjection;

namespace TamaGo.Views;

public partial class DestinationDetailPage : ContentPage
{
    private readonly DestinationDetailPageViewModel _viewModel;

    public DestinationDetailPage(Destination destination)
    {
        InitializeComponent();
        
        var destinationService = App.ServiceProvider.GetRequiredService<DestinationService>();
        var tourService = App.ServiceProvider.GetRequiredService<TourService>();
        
        _viewModel = new DestinationDetailPageViewModel(destination, destinationService, tourService);
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        try
        {
            System.Diagnostics.Debug.WriteLine($"OnAppearing called for destination: {_viewModel.Destination.Name} (ID: {_viewModel.Destination.IdDestination})");
            await _viewModel.LoadToursAsync();
            System.Diagnostics.Debug.WriteLine($"Tours loaded for destination: {_viewModel.Destination.Name}, Count: {_viewModel.Tours?.Count ?? 0}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading destination tours: {ex}");
            await DisplayAlert("Error", $"Error al cargar los tours del destino: {ex.Message}", "OK");
        }
    }
    
    private async void OnTourDetailsClicked(object sender, EventArgs e)
    {
        try
        {
            if (sender is Button button && button.CommandParameter is Tour tour)
            {
                System.Diagnostics.Debug.WriteLine($"OnTourDetailsClicked: Tour {tour.IdTour} - {tour.Title}");
                var tourDetailPage = new TourDetailPage(tour);
                await Navigation.PushAsync(tourDetailPage);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnTourDetailsClicked: {ex}");
            await DisplayAlert("Error", $"Error navegando al tour: {ex.Message}", "OK");
        }
    }
    
    private async void OnReserveTourClicked(object sender, EventArgs e)
    {
        try
        {
            if (sender is Button button && button.CommandParameter is Tour tour)
            {
                System.Diagnostics.Debug.WriteLine($"OnReserveTourClicked: Tour {tour.IdTour} - {tour.Title}");
                var modal = new BookingModalPage(tour);
                await Navigation.PushModalAsync(modal);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnReserveTourClicked: {ex}");
            await DisplayAlert("Error", $"Error al abrir reserva: {ex.Message}", "OK");
        }
    }
}