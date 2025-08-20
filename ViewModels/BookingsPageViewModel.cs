using System.Collections.ObjectModel;
using System.Windows.Input;
using TamaGo.Models;
using TamaGo.Services;

namespace TamaGo.ViewModels;

public class BookingsPageViewModel : BaseViewModel
{
    private readonly TourService _tourService;
    private readonly AuthService _authService;

    public BookingsPageViewModel(TourService tourService, AuthService authService)
    {
        _tourService = tourService;
        _authService = authService;
        
        LoadBookingsCommand = new Command(async () => await LoadBookingsAsync());
        CancelBookingCommand = new Command<Booking>(async (booking) => await CancelBookingAsync(booking));
        RefreshCommand = new Command(async () => await RefreshBookingsAsync());
    }

    #region Properties

    private ObservableCollection<Booking> _allBookings = new();
    public ObservableCollection<Booking> AllBookings
    {
        get => _allBookings;
        set
        {
            if (SetProperty(ref _allBookings, value))
            {
                UpdateFilteredBookings();
            }
        }
    }

    private ObservableCollection<Booking> _activeBookings = new();
    public ObservableCollection<Booking> ActiveBookings
    {
        get => _activeBookings;
        set => SetProperty(ref _activeBookings, value);
    }

    private ObservableCollection<Booking> _pastBookings = new();
    public ObservableCollection<Booking> PastBookings
    {
        get => _pastBookings;
        set => SetProperty(ref _pastBookings, value);
    }

    private ObservableCollection<Booking> _cancelledBookings = new();
    public ObservableCollection<Booking> CancelledBookings
    {
        get => _cancelledBookings;
        set => SetProperty(ref _cancelledBookings, value);
    }

    private string _selectedTab = "active";
    public string SelectedTab
    {
        get => _selectedTab;
        set
        {
            if (SetProperty(ref _selectedTab, value))
            {
                OnPropertyChanged(nameof(IsActiveTabSelected));
                OnPropertyChanged(nameof(IsPastTabSelected));
                OnPropertyChanged(nameof(IsCancelledTabSelected));
                OnPropertyChanged(nameof(CurrentBookings));
            }
        }
    }

    public bool IsActiveTabSelected => SelectedTab == "active";
    public bool IsPastTabSelected => SelectedTab == "past";
    public bool IsCancelledTabSelected => SelectedTab == "cancelled";

    public ObservableCollection<Booking> CurrentBookings =>
        SelectedTab switch
        {
            "active" => ActiveBookings,
            "past" => PastBookings,
            "cancelled" => CancelledBookings,
            _ => ActiveBookings
        };

    public bool HasBookings => AllBookings?.Any() == true;
    public bool NoBookings => !IsLoading && !HasBookings;

    public int ActiveCount => ActiveBookings?.Count ?? 0;
    public int PastCount => PastBookings?.Count ?? 0;
    public int CancelledCount => CancelledBookings?.Count ?? 0;

    #endregion

    #region Commands

    public ICommand LoadBookingsCommand { get; }
    public ICommand CancelBookingCommand { get; }
    public ICommand RefreshCommand { get; }

    #endregion

    #region Methods

    public async Task LoadBookingsAsync()
    {
        try
        {
            IsLoading = true;
            LoadingMessage = "Cargando reservas...";

            System.Diagnostics.Debug.WriteLine("Loading user bookings...");

            var (success, bookings, message) = await _tourService.GetUserBookingsAsync();
            
            if (success)
            {
                AllBookings = bookings;
                System.Diagnostics.Debug.WriteLine($"Loaded {AllBookings.Count} bookings for user");
            }
            else
            {
                AllBookings = new ObservableCollection<Booking>();
                System.Diagnostics.Debug.WriteLine($"Failed to load bookings: {message}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading bookings: {ex.Message}");
            AllBookings = new ObservableCollection<Booking>();
        }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(HasBookings));
            OnPropertyChanged(nameof(NoBookings));
        }
    }

    private async Task RefreshBookingsAsync()
    {
        await LoadBookingsAsync();
    }

    private async Task CancelBookingAsync(Booking booking)
    {
        if (booking == null) return;

        try
        {
            IsLoading = true;
            LoadingMessage = "Cancelando reserva...";

            var (success, message) = await _tourService.CancelBookingAsync(booking.IdBooking);
            
            if (success)
            {
                // Refresh bookings after cancellation
                await LoadBookingsAsync();
                
                var currentPage = Shell.Current?.CurrentPage;
                if (currentPage != null)
                    await currentPage.DisplayAlert("Éxito", message, "OK");
            }
            else
            {
                var currentPage = Shell.Current?.CurrentPage;
                if (currentPage != null)
                    await currentPage.DisplayAlert("Error", message, "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error cancelling booking: {ex.Message}");
            
            var currentPage = Shell.Current?.CurrentPage;
            if (currentPage != null)
                await currentPage.DisplayAlert("Error", $"Error al cancelar la reserva: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void UpdateFilteredBookings()
    {
        if (AllBookings == null) return;

        var now = DateTime.Now.Date;

        ActiveBookings = new ObservableCollection<Booking>(
            AllBookings.Where(b => b.State == "confirmed" && b.TourDate >= now)
        );

        PastBookings = new ObservableCollection<Booking>(
            AllBookings.Where(b => b.State == "confirmed" && b.TourDate < now)
        );

        CancelledBookings = new ObservableCollection<Booking>(
            AllBookings.Where(b => b.State == "cancelled")
        );

        OnPropertyChanged(nameof(ActiveCount));
        OnPropertyChanged(nameof(PastCount));
        OnPropertyChanged(nameof(CancelledCount));
        OnPropertyChanged(nameof(CurrentBookings));
    }

    #endregion
}