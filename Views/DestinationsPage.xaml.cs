using TamaGo.ViewModels;
using TamaGo.Services;
using Microsoft.Extensions.DependencyInjection;

namespace TamaGo.Views;

public partial class DestinationsPage : ContentPage
{
    private readonly DestinationsPageViewModel _viewModel;

    public DestinationsPage(DestinationService destinationService, TourService tourService)
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
        
        _viewModel = new DestinationsPageViewModel(destinationService, tourService);
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        try
        {
            if (_viewModel.FilteredDestinations?.Any() != true)
            {
                await _viewModel.LoadDestinationsAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnAppearing: {ex.Message}");
            await DisplayAlert("Error", "Error al cargar la página de destinos.", "OK");
        }
    }
}