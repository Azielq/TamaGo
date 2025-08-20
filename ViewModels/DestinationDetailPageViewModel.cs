using System.Collections.ObjectModel;
using System.Windows.Input;
using TamaGo.Models;
using TamaGo.Services;

namespace TamaGo.ViewModels;

public class DestinationDetailPageViewModel : BaseViewModel
{
    private readonly DestinationService _destinationService;
    private readonly TourService _tourService;

    public DestinationDetailPageViewModel(Destination destination, DestinationService destinationService, TourService tourService)
    {
        _destinationService = destinationService;
        _tourService = tourService;
        Destination = destination;
        
        BackCommand = new Command(async () => await OnBackAsync());
        ToggleFavoriteCommand = new Command(async () => await ToggleFavoriteAsync());
        TourTappedCommand = new Command<Tour>(async (tour) => await OnTourTappedAsync(tour));
        
        LoadToursCommand = new Command(async () => await LoadToursAsync());
        ReserveTourCommand = new Command<Tour>(async (tour) => await OnReserveTourAsync(tour));
    }

    #region Properties

    private Destination _destination = new();
    public Destination Destination
    {
        get => _destination;
        set => SetProperty(ref _destination, value);
    }

    private ObservableCollection<Tour> _tours = new();
    public ObservableCollection<Tour> Tours
    {
        get => _tours;
        set
        {
            if (SetProperty(ref _tours, value))
            {
                OnPropertyChanged(nameof(HasTours));
                OnPropertyChanged(nameof(NoTours));
            }
        }
    }

    public bool HasTours => Tours?.Any() == true && !IsLoading;
    public bool NoTours => !IsLoading && !HasTours;
    public bool HasReviews => Destination?.Reviews?.Any() == true;
    
    public ObservableCollection<Review> TopReviews 
    {
        get
        {
            if (Destination?.Reviews == null) return new ObservableCollection<Review>();
            
            return new ObservableCollection<Review>(
                Destination.Reviews
                    .OrderByDescending(r => r.ReviewDate)
                    .Take(3)
            );
        }
    }

    #endregion

    #region Commands

    public ICommand BackCommand { get; }
    public ICommand ToggleFavoriteCommand { get; }
    public ICommand TourTappedCommand { get; }
    public ICommand LoadToursCommand { get; }
    public ICommand ReserveTourCommand { get; }

    #endregion

    #region Methods

    public async Task LoadToursAsync()
    {
        try
        {
            IsLoading = true;
            LoadingMessage = "Cargando tours...";
            System.Diagnostics.Debug.WriteLine($"Starting LoadToursAsync for destination ID: {Destination.IdDestination}");

            // Ensure UI updates immediately when loading starts
            OnPropertyChanged(nameof(HasTours));
            OnPropertyChanged(nameof(NoTours));

            var (success, tours, message) = await _tourService.GetToursByDestinationAsync(Destination.IdDestination);
            
            System.Diagnostics.Debug.WriteLine($"LoadToursAsync result: success={success}, tours count={tours?.Count ?? 0}, message={message}");
            
            if (success && tours?.Count > 0)
            {
                Tours = new ObservableCollection<Tour>(tours);
                System.Diagnostics.Debug.WriteLine($"Tours collection updated with {Tours.Count} tours");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load tours: {message}");
                
                // If no tours found, try to create sample tours
                System.Diagnostics.Debug.WriteLine("Attempting to create sample tours...");
                var (createSuccess, createMessage) = await _tourService.CreateSampleToursAsync();
                
                if (createSuccess)
                {
                    System.Diagnostics.Debug.WriteLine($"Sample tours created: {createMessage}");
                    // Try loading tours again after creating samples
                    var (retrySuccess, retryTours, retryMessage) = await _tourService.GetToursByDestinationAsync(Destination.IdDestination);
                    
                    if (retrySuccess && retryTours?.Count > 0)
                    {
                        Tours = new ObservableCollection<Tour>(retryTours);
                        System.Diagnostics.Debug.WriteLine($"After creating samples, loaded {Tours.Count} tours");
                    }
                    else
                    {
                        Tours = new ObservableCollection<Tour>();
                        System.Diagnostics.Debug.WriteLine("No tours found even after creating samples");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to create sample tours: {createMessage}");
                    Tours = new ObservableCollection<Tour>();
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Exception in LoadToursAsync: {ex}");
            Tours = new ObservableCollection<Tour>();
        }
        finally
        {
            IsLoading = false;
            // Force update of computed properties
            OnPropertyChanged(nameof(HasTours));
            OnPropertyChanged(nameof(NoTours));
            OnPropertyChanged(nameof(IsLoading));
            System.Diagnostics.Debug.WriteLine($"LoadToursAsync completed. IsLoading: {IsLoading}, HasTours: {HasTours}, NoTours: {NoTours}, Tours Count: {Tours?.Count ?? 0}");
        }
    }

    private async Task OnBackAsync()
    {
        try
        {
            var nav = Shell.Current?.Navigation;
            if (nav is not null)
                await nav.PopAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error navigating back: {ex.Message}");
        }
    }

    private async Task ToggleFavoriteAsync()
    {
        try
        {
            var (success, message) = await _destinationService.ToggleFavoriteAsync(Destination.IdDestination);
            
            if (success)
            {
                Destination.IsFavorite = !Destination.IsFavorite;
                OnPropertyChanged(nameof(Destination));
                
                // Mostrar mensaje breve
                var currentPage = Shell.Current?.CurrentPage;
                if (currentPage is not null)
                    await currentPage.DisplayAlert("Favoritos", message, "OK");
            }
            else
            {
                var currentPage = Shell.Current?.CurrentPage;
                if (currentPage is not null)
                    await currentPage.DisplayAlert("Error", message, "OK");
            }
        }
        catch (Exception ex)
        {
            var currentPage = Shell.Current?.CurrentPage;
            if (currentPage is not null)
                await currentPage.DisplayAlert("Error", $"Error al gestionar favorito: {ex.Message}", "OK");
        }
    }

    private async Task OnTourTappedAsync(Tour tour)
    {
        if (tour == null) return;

        try
        {
            System.Diagnostics.Debug.WriteLine($"TourTappedCommand invoked for tour {tour.IdTour} - {tour.Title}");
            
            // Debug navigation state
            System.Diagnostics.Debug.WriteLine($"Shell.Current: {Shell.Current}");
            System.Diagnostics.Debug.WriteLine($"Shell.Current?.Navigation: {Shell.Current?.Navigation}");
            System.Diagnostics.Debug.WriteLine($"Shell.Current?.CurrentPage: {Shell.Current?.CurrentPage}");
            
            // Navegar a la página de detalle del tour
            var tourDetailPage = new Views.TourDetailPage(tour);
            var nav = Shell.Current?.Navigation;
            if (nav is not null)
            {
                System.Diagnostics.Debug.WriteLine("Attempting navigation to TourDetailPage...");
                await nav.PushAsync(tourDetailPage);
                System.Diagnostics.Debug.WriteLine("Navigation to TourDetailPage completed");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("ERROR: Navigation is null!");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Exception in OnTourTappedAsync: {ex}");
            var currentPage = Shell.Current?.CurrentPage;
            if (currentPage is not null)
                await currentPage.DisplayAlert("Error", $"Error navegando al tour: {ex.Message}", "OK");
        }
    }

    private async Task OnReserveTourAsync(Tour tour)
    {
        if (tour == null) return;

        try
        {
            System.Diagnostics.Debug.WriteLine($"ReserveTourCommand invoked for tour {tour.IdTour} - {tour.Title}");
            
            // Debug navigation state
            System.Diagnostics.Debug.WriteLine($"Shell.Current: {Shell.Current}");
            System.Diagnostics.Debug.WriteLine($"Shell.Current?.Navigation: {Shell.Current?.Navigation}");
            System.Diagnostics.Debug.WriteLine($"Shell.Current?.CurrentPage: {Shell.Current?.CurrentPage}");
            
            var modal = new Views.BookingModalPage(tour);
            var nav = Shell.Current?.Navigation;
            if (nav is not null)
            {
                System.Diagnostics.Debug.WriteLine("Attempting PushModalAsync for BookingModalPage...");
                await nav.PushModalAsync(modal);
                System.Diagnostics.Debug.WriteLine("PushModalAsync completed");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("ERROR: Navigation is null!");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Exception in OnReserveTourAsync: {ex}");
            var currentPage = Shell.Current?.CurrentPage;
            if (currentPage is not null)
                await currentPage.DisplayAlert("Error", $"Error al abrir reserva: {ex.Message}", "OK");
        }
    }

    protected override void OnIsLoadingChanged()
    {
        base.OnIsLoadingChanged();
        OnPropertyChanged(nameof(HasTours));
        OnPropertyChanged(nameof(NoTours));
    }

    #endregion
}