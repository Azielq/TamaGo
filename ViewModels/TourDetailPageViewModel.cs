using System.Windows.Input;
using TamaGo.Models;
using TamaGo.Services;
using TamaGo.Views;

namespace TamaGo.ViewModels;

public class TourDetailPageViewModel : BaseViewModel
{
    private readonly TourService _tourService;
    private readonly ContentPage _page;

    public TourDetailPageViewModel(Tour tour, TourService tourService, ContentPage page)
    {
        _tourService = tourService;
        _page = page;
        Tour = tour;
        
        // Inicializar valores por defecto
        SelectedDate = DateTime.Now.Date.AddDays(1);
        PeopleCount = 1;
        
        BackCommand = new Command(async () => await OnBackAsync());
        BookTourCommand = new Command(async () => await BookTourAsync(), () => CanBook);
        IncreasePeopleCommand = new Command(() => IncreasePeople());
        DecreasePeopleCommand = new Command(() => DecreasePeople(), () => PeopleCount > 1);
        CheckAvailabilityCommand = new Command(async () => await CheckAvailabilityAsync());
    }

    #region Properties

    private Tour _tour = new();
    public Tour Tour
    {
        get => _tour;
        set => SetProperty(ref _tour, value);
    }

    private DateTime _selectedDate;
    public DateTime SelectedDate
    {
        get => _selectedDate;
        set
        {
            if (SetProperty(ref _selectedDate, value))
            {
                _ = CheckAvailabilityAsync();
            }
        }
    }

    private int _peopleCount = 1;
    public int PeopleCount
    {
        get => _peopleCount;
        set
        {
            if (SetProperty(ref _peopleCount, value))
            {
                OnPropertyChanged(nameof(TotalPrice));
                OnPropertyChanged(nameof(BookButtonText));
                OnPropertyChanged(nameof(CanBook));
                ((Command)DecreasePeopleCommand).ChangeCanExecute();
                ((Command)BookTourCommand).ChangeCanExecute();
                _ = CheckAvailabilityAsync();
            }
        }
    }

    private int _availableSpaces;
    public int AvailableSpaces
    {
        get => _availableSpaces;
        set
        {
            if (SetProperty(ref _availableSpaces, value))
            {
                OnPropertyChanged(nameof(AvailabilityText));
                OnPropertyChanged(nameof(AvailabilityColor));
                OnPropertyChanged(nameof(ShowAvailability));
                OnPropertyChanged(nameof(CanBook));
                ((Command)BookTourCommand).ChangeCanExecute();
            }
        }
    }

    private bool _availabilityChecked;
    public bool AvailabilityChecked
    {
        get => _availabilityChecked;
        set => SetProperty(ref _availabilityChecked, value);
    }

    public DateTime MinDate => DateTime.Now.Date.AddDays(1);
    public decimal TotalPrice => Tour?.Price * PeopleCount ?? 0;
    public bool HasIncludes => !string.IsNullOrWhiteSpace(Tour?.Includes);
    public bool HasRequirements => !string.IsNullOrWhiteSpace(Tour?.Requirements);
    public bool ShowAvailability => AvailabilityChecked;
    
    public string AvailabilityText => AvailableSpaces > 0 
        ? $"Disponible - {AvailableSpaces} espacios restantes"
        : "No disponible para esta fecha";
        
    public Color AvailabilityColor => AvailableSpaces > 0 
        ? Colors.Green 
        : Colors.Red;

    public bool CanBook => !IsLoading && 
                          AvailabilityChecked && 
                          AvailableSpaces >= PeopleCount && 
                          PeopleCount > 0 && 
                          SelectedDate >= MinDate;

    public string BookButtonText => IsLoading 
        ? "Procesando..." 
        : $"Reservar por ${TotalPrice:F0}";

    #endregion

    #region Commands

    public ICommand BackCommand { get; }
    public ICommand BookTourCommand { get; }
    public ICommand IncreasePeopleCommand { get; }
    public ICommand DecreasePeopleCommand { get; }
    public ICommand CheckAvailabilityCommand { get; }

    #endregion

    #region Methods

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

    private void IncreasePeople()
    {
        System.Diagnostics.Debug.WriteLine($"TourDetail IncreasePeople called. Current: {PeopleCount}, Max: {Tour?.MaxCapacity}");
        if (PeopleCount < Tour.MaxCapacity)
        {
            PeopleCount++;
            System.Diagnostics.Debug.WriteLine($"TourDetail People increased to: {PeopleCount}");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("TourDetail Cannot increase - at max capacity");
        }
    }

    private void DecreasePeople()
    {
        System.Diagnostics.Debug.WriteLine($"TourDetail DecreasePeople called. Current: {PeopleCount}");
        if (PeopleCount > 1)
        {
            PeopleCount--;
            System.Diagnostics.Debug.WriteLine($"TourDetail People decreased to: {PeopleCount}");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("TourDetail Cannot decrease - at minimum");
        }
    }

    private async Task CheckAvailabilityAsync()
    {
        try
        {
            if (Tour == null || SelectedDate < MinDate)
                return;

            var (success, availableSpaces, message) = await _tourService.GetTourAvailabilityAsync(Tour.IdTour, SelectedDate);
            
            if (success)
            {
                AvailableSpaces = availableSpaces;
                AvailabilityChecked = true;
            }
            else
            {
                AvailableSpaces = 0;
                AvailabilityChecked = true;
                System.Diagnostics.Debug.WriteLine($"Error checking availability: {message}");
            }
        }
        catch (Exception ex)
        {
            AvailableSpaces = 0;
            AvailabilityChecked = true;
            System.Diagnostics.Debug.WriteLine($"Error checking availability: {ex.Message}");
        }
    }

    private async Task BookTourAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"TourDetail BookTourAsync called. CanBook: {CanBook}");
            if (!CanBook)
            {
                System.Diagnostics.Debug.WriteLine("TourDetail Cannot book - CanBook is false");
                return;
            }

            // Confirmar la reserva con el usuario usando la página directa
            System.Diagnostics.Debug.WriteLine("TourDetail Using direct page reference for confirmation dialog");
            var confirmed = await _page.DisplayAlert(
                "Confirmar Reserva",
                $"¿Confirmar reserva para {PeopleCount} persona(s) el {SelectedDate:dd/MM/yyyy}?\n\nTotal: ${TotalPrice:F0}",
                "Confirmar",
                "Cancelar");

            if (!confirmed)
            {
                System.Diagnostics.Debug.WriteLine("TourDetail User cancelled booking");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"TourDetail Processing booking for {PeopleCount} people on {SelectedDate:yyyy-MM-dd}");
            IsLoading = true;
            LoadingMessage = "Procesando reserva...";

            var (success, message) = await _tourService.BookTourAsync(Tour.IdTour, SelectedDate, PeopleCount);
            System.Diagnostics.Debug.WriteLine($"TourDetail Booking result: success={success}, message={message}");
            
            if (success)
            {
                await _page.DisplayAlert("¡Reserva Exitosa!", message, "OK");
                
                // Navigation using the page reference instead of Shell to avoid null reference
                try
                {
                    var tourService = App.ServiceProvider.GetRequiredService<TourService>();
                    var authService = App.ServiceProvider.GetRequiredService<AuthService>();
                    var bookingsPage = new BookingsPage(tourService, authService);
                    await _page.Navigation.PushAsync(bookingsPage);
                }
                catch (Exception navEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Navigation error after booking: {navEx.Message}");
                    // Don't show error to user since booking was successful
                }
            }
            else
            {
                await _page.DisplayAlert("Error en la Reserva", message, "OK");
                
                // Actualizar disponibilidad después del error
                await CheckAvailabilityAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"TourDetail Exception in BookTourAsync: {ex}");
            await _page.DisplayAlert("Error", $"Error procesando la reserva: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task InitializeAsync()
    {
        await CheckAvailabilityAsync();
    }

    #endregion
}