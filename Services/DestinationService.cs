using TamaGo.Models;
using Supabase;
using System.Collections.ObjectModel;

namespace TamaGo.Services;

public class DestinationService
{
    private readonly SupabaseService _supabaseService;
    private readonly AuthService _authService;

    public DestinationService(SupabaseService supabaseService, AuthService authService)
    {
        _supabaseService = supabaseService;
        _authService = authService;
    }

    public async Task<(bool success, ObservableCollection<Destination> destinations, string message)> GetDestinationsAsync()
    {
        try
        {
            var client = await _supabaseService.GetClientAsync();
            
            var response = await client
                .From<Destination>()
                .Get();

            if (response?.Models != null)
            {
                var destinations = new ObservableCollection<Destination>(response.Models);
                
                foreach (var destination in destinations)
                {
                    await LoadDestinationDetailsAsync(destination);
                }

                return (true, destinations, "Destinos cargados exitosamente");
            }

            return (false, new ObservableCollection<Destination>(), "No se encontraron destinos");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading destinations: {ex.Message}");
            return (false, new ObservableCollection<Destination>(), $"Error: {ex.Message}");
        }
    }

    public async Task<(bool success, ObservableCollection<Destination> destinations, string message)> GetFeaturedDestinationsAsync()
    {
        try
        {
            var client = await _supabaseService.GetClientAsync();
            
            // Algunas bases pueden no tener la columna is_featured.
            // Intentar filtrar por columna real; si falla, traer todos y filtrar en memoria por propiedad.
            try
            {
                var featuredResponse = await client
                    .From<Destination>()
                    .Filter("is_featured", Supabase.Postgrest.Constants.Operator.Equals, true)
                    .Get();

                if (featuredResponse?.Models != null)
                {
                    var destinations = new ObservableCollection<Destination>(featuredResponse.Models);
                    foreach (var destination in destinations)
                    {
                        await LoadDestinationDetailsAsync(destination);
                    }
                    return (true, destinations, "Destinos destacados cargados");
                }
            }
            catch
            {
                // fallback abajo
            }

            var allResponse = await client
                .From<Destination>()
                .Get();

            if (allResponse?.Models != null)
            {
                var destinations = new ObservableCollection<Destination>(allResponse.Models.Where(d => d.IsFeatured));
                foreach (var destination in destinations)
                {
                    await LoadDestinationDetailsAsync(destination);
                }
                return (true, destinations, destinations.Count > 0 ? "Destinos destacados cargados" : "Sin destacados configurados");
            }

            return (false, new ObservableCollection<Destination>(), "No se encontraron destinos destacados");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading featured destinations: {ex.Message}");
            return (false, new ObservableCollection<Destination>(), $"Error: {ex.Message}");
        }
    }

    public async Task<(bool success, ObservableCollection<Destination> destinations, string message)> SearchDestinationsAsync(string searchTerm, string? category = null)
    {
        try
        {
            var client = await _supabaseService.GetClientAsync();
            
            var response = await client
                .From<Destination>()
                .Get();
            
            if (response?.Models != null)
            {
                var filteredDestinations = response.Models.AsEnumerable();
                
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchLower = searchTerm.ToLower();
                    filteredDestinations = filteredDestinations.Where(d => 
                        d.Name?.ToLower().Contains(searchLower) == true ||
                        d.Description?.ToLower().Contains(searchLower) == true ||
                        d.Location?.ToLower().Contains(searchLower) == true);
                }
                
                if (!string.IsNullOrWhiteSpace(category) && category != "Todos")
                {
                    filteredDestinations = filteredDestinations.Where(d => d.Category == category);
                }
                
                var destinations = new ObservableCollection<Destination>(filteredDestinations);
                
                foreach (var destination in destinations)
                {
                    await LoadDestinationDetailsAsync(destination);
                }

                return (true, destinations, $"Se encontraron {destinations.Count} destinos");
            }

            return (false, new ObservableCollection<Destination>(), "No se encontraron destinos");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error searching destinations: {ex.Message}");
            return (false, new ObservableCollection<Destination>(), $"Error: {ex.Message}");
        }
    }

    public async Task<(bool success, Destination? destination, string message)> GetDestinationByIdAsync(int destinationId)
    {
        try
        {
            var client = await _supabaseService.GetClientAsync();
            
            var response = await client
                .From<Destination>()
                .Where(d => d.IdDestination == destinationId)
                .Get();

            if (response?.Models != null && response.Models.Count > 0)
            {
                var destination = response.Models.First();
                await LoadDestinationDetailsAsync(destination);
                
                return (true, destination, "Destino cargado exitosamente");
            }

            return (false, null, "Destino no encontrado");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading destination: {ex.Message}");
            return (false, null, $"Error: {ex.Message}");
        }
    }

    private async Task LoadDestinationDetailsAsync(Destination destination)
    {
        try
        {
            var client = await _supabaseService.GetClientAsync();

            var reviewsResponse = await client
                .From<Review>()
                .Where(r => r.IdDestination == destination.IdDestination)
                .Get();

            if (reviewsResponse?.Models != null)
            {
                destination.Reviews = new ObservableCollection<Review>(reviewsResponse.Models);
                destination.ReviewCount = destination.Reviews.Count;
                destination.AverageRating = destination.Reviews.Any() 
                    ? Math.Round(destination.Reviews.Average(r => r.Rating), 1) 
                    : 0;
            }

            if (_authService.CurrentUser != null)
            {
                var favoriteResponse = await client
                    .From<Favorite>()
                    .Where(f => f.IdUser == _authService.CurrentUser.IdUser && f.IdDestination == destination.IdDestination)
                    .Get();

                destination.IsFavorite = favoriteResponse?.Models?.Any() == true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading destination details: {ex.Message}");
        }
    }

    public async Task<(bool success, string message)> ToggleFavoriteAsync(int destinationId)
    {
        try
        {
            if (_authService.CurrentUser == null)
            {
                return (false, "Debe iniciar sesión para guardar favoritos");
            }

            var client = await _supabaseService.GetClientAsync();
            
            var existingFavorite = await client
                .From<Favorite>()
                .Where(f => f.IdUser == _authService.CurrentUser.IdUser && f.IdDestination == destinationId)
                .Get();

            if (existingFavorite?.Models?.Any() == true)
            {
                await client
                    .From<Favorite>()
                    .Where(f => f.IdUser == _authService.CurrentUser.IdUser && f.IdDestination == destinationId)
                    .Delete();
                    
                return (true, "Eliminado de favoritos");
            }
            else
            {
                var newFavorite = new Favorite
                {
                    IdUser = _authService.CurrentUser.IdUser,
                    IdDestination = destinationId,
                    AddedDate = DateTime.UtcNow
                };

                await client
                    .From<Favorite>()
                    .Insert(newFavorite);
                    
                return (true, "Agregado a favoritos");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error toggling favorite: {ex.Message}");
            return (false, $"Error: {ex.Message}");
        }
    }

    public async Task<(bool success, ObservableCollection<string> categories, string message)> GetCategoriesAsync()
    {
        try
        {
            var client = await _supabaseService.GetClientAsync();
            
            var response = await client
                .From<Destination>()
                .Select("category")
                .Get();

            if (response?.Models != null)
            {
                var categories = new ObservableCollection<string>(
                    response.Models
                        .Select(d => d.Category)
                        .Where(c => !string.IsNullOrWhiteSpace(c))
                        .Distinct()
                        .OrderBy(c => c)
                );

                categories.Insert(0, "Todos");
                
                return (true, categories, "Categorías cargadas");
            }

            return (false, new ObservableCollection<string>(), "No se encontraron categorías");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading categories: {ex.Message}");
            return (false, new ObservableCollection<string>(), $"Error: {ex.Message}");
        }
    }
}