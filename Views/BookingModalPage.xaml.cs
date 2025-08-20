using TamaGo.Models;
using TamaGo.ViewModels;
using TamaGo.Services;
using Microsoft.Extensions.DependencyInjection;

namespace TamaGo.Views;

public partial class BookingModalPage : ContentPage
{
    private readonly BookingModalViewModel _viewModel;

    public BookingModalPage(Tour tour)
    {
        InitializeComponent();

        var tourService = App.ServiceProvider.GetRequiredService<TourService>();
        _viewModel = new BookingModalViewModel(tour, tourService, CloseAsync, this);
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        System.Diagnostics.Debug.WriteLine("BookingModalPage OnAppearing");
        await _viewModel.InitializeAsync();
    }

    private async Task CloseAsync(bool booked)
    {
        await Navigation.PopModalAsync();

        if (booked)
        {
            await Shell.Current.GoToAsync("//BookingsPage");
        }
    }
}

