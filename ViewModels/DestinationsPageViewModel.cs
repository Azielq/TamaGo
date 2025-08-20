using System.Collections.ObjectModel;
using System.Windows.Input;
using TamaGo.Models;
using TamaGo.Services;

namespace TamaGo.ViewModels;

public class DestinationsPageViewModel : BaseViewModel
{
    private readonly DestinationService _destinationService;
    private readonly TourService _tourService;
    

    public DestinationsPageViewModel(DestinationService destinationService, TourService tourService)
    {
        _destinationService = destinationService;
        _tourService = tourService;
        
        SearchCommand = new Command<string>(async (searchText) => await SearchDestinationsAsync(searchText));
        DestinationTappedCommand = new Command<Destination>(async (destination) => await OnDestinationTappedAsync(destination));
        ToggleFavoriteCommand = new Command<Destination>(async (destination) => await ToggleFavoriteAsync(destination));
        ClearFiltersCommand = new Command(async () => await ClearFiltersAsync());
        CategorySelectedCommand = new Command<string>((category) => SelectedCategory = category);
        
        LoadDestinationsCommand = new Command(async () => await LoadDestinationsAsync());
    }

    #region Properties

    private ObservableCollection<Destination> _allDestinations = new();
    private ObservableCollection<Destination> _featuredDestinations = new();
    public ObservableCollection<Destination> FeaturedDestinations
    {
        get => _featuredDestinations;
        set => SetProperty(ref _featuredDestinations, value);
    }

    private ObservableCollection<Destination> _filteredDestinations = new();
    public ObservableCollection<Destination> FilteredDestinations
    {
        get => _filteredDestinations;
        set => SetProperty(ref _filteredDestinations, value);
    }

    private ObservableCollection<string> _categories = new();
    public ObservableCollection<string> Categories
    {
        get => _categories;
        set => SetProperty(ref _categories, value);
    }

    private string _selectedCategory = "Todos";
    public string SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (SetProperty(ref _selectedCategory, value))
            {
                _ = FilterDestinationsAsync();
                OnPropertyChanged(nameof(DestinationsTitle));
            }
        }
    }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    public bool HasFeaturedDestinations => FeaturedDestinations?.Any() == true;
    public bool HasDestinations => FilteredDestinations?.Any() == true;
    public bool IsEmpty => !IsLoading && !HasDestinations;
    
    public string DestinationsTitle => string.IsNullOrWhiteSpace(SearchText) && SelectedCategory == "Todos" 
        ? "Todos los destinos" 
        : "Resultados de búsqueda";

    #endregion

    #region Commands

    public ICommand SearchCommand { get; }
    public ICommand DestinationTappedCommand { get; }
    public ICommand ToggleFavoriteCommand { get; }
    public ICommand ClearFiltersCommand { get; }
    public ICommand CategorySelectedCommand { get; }
    public ICommand LoadDestinationsCommand { get; }

    #endregion

    #region Methods

    public async Task LoadDestinationsAsync()
    {
        try
        {
            IsLoading = true;
            LoadingMessage = "Cargando destinos...";

            // Cargar categorías
            var (categoriesSuccess, categories, _) = await _destinationService.GetCategoriesAsync();
            if (categoriesSuccess)
            {
                Categories = categories;
            }

            // Cargar destinos destacados
            var (featuredSuccess, featuredDestinations, _) = await _destinationService.GetFeaturedDestinationsAsync();
            if (featuredSuccess)
            {
                FeaturedDestinations = featuredDestinations;
            }

            // Cargar todos los destinos
            var (allSuccess, allDestinations, message) = await _destinationService.GetDestinationsAsync();
            if (allSuccess)
            {
                _allDestinations = allDestinations;
                await FilterDestinationsAsync();
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Error", message, "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Error cargando destinos: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(HasFeaturedDestinations));
            OnPropertyChanged(nameof(HasDestinations));
            OnPropertyChanged(nameof(IsEmpty));
        }
    }

    private async Task SearchDestinationsAsync(string searchText)
    {
        SearchText = searchText;
        await FilterDestinationsAsync();
    }

    private async Task FilterDestinationsAsync()
    {
        try
        {
            if (_allDestinations == null || !_allDestinations.Any())
                return;

            var filteredList = _allDestinations.AsEnumerable();

            // Filtrar por categoría
            if (!string.IsNullOrWhiteSpace(SelectedCategory) && SelectedCategory != "Todos")
            {
                filteredList = filteredList.Where(d => d.Category == SelectedCategory);
            }

            // Filtrar por texto de búsqueda
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchLower = SearchText.ToLower();
                filteredList = filteredList.Where(d => 
                    d.Name.ToLower().Contains(searchLower) ||
                    d.Description.ToLower().Contains(searchLower) ||
                    d.Location.ToLower().Contains(searchLower) ||
                    d.Category.ToLower().Contains(searchLower));
            }

            FilteredDestinations = new ObservableCollection<Destination>(filteredList);
            
            OnPropertyChanged(nameof(HasDestinations));
            OnPropertyChanged(nameof(IsEmpty));
            OnPropertyChanged(nameof(DestinationsTitle));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error filtering destinations: {ex.Message}");
        }
    }

    private async Task OnDestinationTappedAsync(Destination destination)
    {
        if (destination == null) return;

        try
        {
            // Navegar a la página de detalle del destino
            var detailPage = new Views.DestinationDetailPage(destination);
            await Application.Current.MainPage.Navigation.PushAsync(detailPage);
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Error navegando al destino: {ex.Message}", "OK");
        }
    }

    private async Task ToggleFavoriteAsync(Destination destination)
    {
        if (destination == null) return;

        try
        {
            var (success, message) = await _destinationService.ToggleFavoriteAsync(destination.IdDestination);
            
            if (success)
            {
                destination.IsFavorite = !destination.IsFavorite;
                
                // Mostrar un toast o mensaje breve
                await Application.Current.MainPage.DisplayAlert("Favoritos", message, "OK");
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Error", message, "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Error al gestionar favorito: {ex.Message}", "OK");
        }
    }

    private async Task ClearFiltersAsync()
    {
        SearchText = string.Empty;
        SelectedCategory = "Todos";
        await FilterDestinationsAsync();
    }

    #endregion
}