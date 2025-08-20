using System.Windows.Input;
using TamaGo.Models;
using TamaGo.Services;

namespace TamaGo.ViewModels;

public class BookingModalViewModel : BaseViewModel
{
    private readonly TourService _tourService;
    private readonly Func<bool, Task> _closeCallback;
    private readonly ContentPage _page;

    public BookingModalViewModel(Tour tour, TourService tourService, Func<bool, Task> closeCallback, ContentPage page)
    {
        _tourService = tourService;
        _closeCallback = closeCallback;
        _page = page;
        Tour = tour;

        SelectedDate = DateTime.Now.Date.AddDays(1);
        PeopleCount = 1;

        ConfirmCommand = new Command(async () => await ConfirmAsync(), () => CanConfirm);
        CancelCommand = new Command(async () => await _closeCallback(false));
        IncreasePeopleCommand = new Command(() => IncreasePeople());
        DecreasePeopleCommand = new Command(() => DecreasePeople(), () => PeopleCount > 1);
        CheckAvailabilityCommand = new Command(async () => await CheckAvailabilityAsync());
        
        System.Diagnostics.Debug.WriteLine($"BookingModalViewModel created for tour {tour.IdTour} - {tour.Title}");
        
        // Initialize availability check
        _ = Task.Run(async () => await CheckAvailabilityAsync());
    }

    public Tour Tour { get; }

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

    public DateTime MinDate => DateTime.Now.Date.AddDays(1);

    private int _peopleCount = 1;
    public int PeopleCount
    {
        get => _peopleCount;
        set
        {
            if (SetProperty(ref _peopleCount, value))
            {
                OnPropertyChanged(nameof(TotalPrice));
                OnPropertyChanged(nameof(CanConfirm));
                ((Command)DecreasePeopleCommand).ChangeCanExecute();
                ((Command)ConfirmCommand).ChangeCanExecute();
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
                OnPropertyChanged(nameof(CanConfirm));
                ((Command)ConfirmCommand).ChangeCanExecute();
            }
        }
    }

    private bool _availabilityChecked;
    public bool AvailabilityChecked
    {
        get => _availabilityChecked;
        set => SetProperty(ref _availabilityChecked, value);
    }

    public bool ShowAvailability => AvailabilityChecked;
    public decimal TotalPrice => (Tour?.Price ?? 0) * PeopleCount;
    public string AvailabilityText => AvailableSpaces > 0
        ? $"Disponible - {AvailableSpaces} espacios restantes"
        : "No disponible para esta fecha";
    public Color AvailabilityColor => AvailableSpaces > 0 ? Colors.Green : Colors.Red;
    public bool CanConfirm => !IsLoading && AvailabilityChecked && AvailableSpaces >= PeopleCount && SelectedDate >= MinDate && PeopleCount > 0;

    public ICommand ConfirmCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand IncreasePeopleCommand { get; }
    public ICommand DecreasePeopleCommand { get; }
    public ICommand CheckAvailabilityCommand { get; }

    private void IncreasePeople()
    {
        System.Diagnostics.Debug.WriteLine($"IncreasePeople called. Current: {PeopleCount}, Max: {Tour?.MaxCapacity}");
        if (PeopleCount < (Tour?.MaxCapacity ?? int.MaxValue))
        {
            PeopleCount++;
            System.Diagnostics.Debug.WriteLine($"People increased to: {PeopleCount}");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("Cannot increase - at max capacity");
        }
    }

    private void DecreasePeople()
    {
        System.Diagnostics.Debug.WriteLine($"DecreasePeople called. Current: {PeopleCount}");
        if (PeopleCount > 1)
        {
            PeopleCount--;
            System.Diagnostics.Debug.WriteLine($"People decreased to: {PeopleCount}");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("Cannot decrease - at minimum");
        }
    }

    private async Task CheckAvailabilityAsync()
    {
        try
        {
            if (Tour == null || SelectedDate < MinDate)
                return;

            var (success, availableSpaces, message) = await _tourService.GetTourAvailabilityAsync(Tour.IdTour, SelectedDate);

            AvailabilityChecked = true;
            AvailableSpaces = success ? availableSpaces : 0;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error checking availability: {ex}");
            System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException}");
            
            AvailabilityChecked = true;
            // Set fallback values when availability check fails
            AvailableSpaces = Tour.MaxCapacity; // Assume full capacity available
            System.Diagnostics.Debug.WriteLine($"Using fallback values: {AvailableSpaces} spots, ${TotalPrice} total");
        }
    }

    private async Task ConfirmAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"ConfirmAsync called. CanConfirm: {CanConfirm}");
            if (!CanConfirm) 
            {
                System.Diagnostics.Debug.WriteLine("Cannot confirm - CanConfirm is false");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"Processing booking for {PeopleCount} people on {SelectedDate:yyyy-MM-dd}");
            IsLoading = true;
            LoadingMessage = "Procesando reserva...";

            var (success, message) = await _tourService.BookTourAsync(Tour.IdTour, SelectedDate, PeopleCount);
            System.Diagnostics.Debug.WriteLine($"Booking result: success={success}, message={message}");

            if (success)
            {
                await _page.DisplayAlert("¡Reserva Exitosa!", message, "OK");
                await _closeCallback(true);
            }
            else
            {
                await _page.DisplayAlert("Error en la Reserva", message, "OK");
                await CheckAvailabilityAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Exception in ConfirmAsync: {ex}");
            await _page.DisplayAlert("Error", $"Error procesando la reserva: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task InitializeAsync()
    {
        System.Diagnostics.Debug.WriteLine("InitializeAsync called");
        await CheckAvailabilityAsync();
    }
}


