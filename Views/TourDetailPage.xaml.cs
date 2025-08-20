using TamaGo.Models;
using TamaGo.ViewModels;
using TamaGo.Services;
using Microsoft.Extensions.DependencyInjection;

namespace TamaGo.Views;

public partial class TourDetailPage : ContentPage
{
    private readonly TourDetailPageViewModel _viewModel;

    public TourDetailPage(Tour tour)
    {
        InitializeComponent();
        
        var tourService = App.ServiceProvider.GetRequiredService<TourService>();
        
        _viewModel = new TourDetailPageViewModel(tour, tourService, this);
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
}