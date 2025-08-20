using TamaGo.Models;
using Supabase;
using System.Collections.ObjectModel;

namespace TamaGo.Services;

public class TourService
{
    private readonly SupabaseService _supabaseService;
    private readonly AuthService _authService;

    public TourService(SupabaseService supabaseService, AuthService authService)
    {
        _supabaseService = supabaseService;
        _authService = authService;
    }

    public async Task<(bool success, ObservableCollection<Tour> tours, string message)> GetToursByDestinationAsync(int destinationId)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"=== DEBUGGING TOUR LOADING FOR DESTINATION {destinationId} ===");
            
            var client = await _supabaseService.GetClientAsync();
            System.Diagnostics.Debug.WriteLine("Supabase client obtained successfully");
            
            // First, let's try to get ALL tours to see if there are any in the database
            System.Diagnostics.Debug.WriteLine("=== STEP 1: Checking for ANY tours in database ===");
            var allToursResponse = await client
                .From<Tour>()
                .Get();
                
            System.Diagnostics.Debug.WriteLine($"Response object null: {allToursResponse == null}");
            System.Diagnostics.Debug.WriteLine($"Models null: {allToursResponse?.Models == null}");
            System.Diagnostics.Debug.WriteLine($"Total tours in database: {allToursResponse?.Models?.Count ?? 0}");
            
            if (allToursResponse?.Models != null && allToursResponse.Models.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine("=== TOURS FOUND IN DATABASE ===");
                foreach (var tour in allToursResponse.Models)
                {
                    System.Diagnostics.Debug.WriteLine($"Tour: ID={tour.IdTour}, Title='{tour.Title}', DestinationID={tour.IdDestination}, Available={tour.IsAvailable}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("=== NO TOURS IN DATABASE - CREATING SAMPLE DATA ===");
                var (createSuccess, createMessage) = await CreateSampleToursAsync();
                System.Diagnostics.Debug.WriteLine($"Sample creation result: {createSuccess}, Message: {createMessage}");
                
                if (createSuccess)
                {
                    // Retry getting all tours after creating samples
                    allToursResponse = await client.From<Tour>().Get();
                    System.Diagnostics.Debug.WriteLine($"After creating samples, total tours: {allToursResponse?.Models?.Count ?? 0}");
                }
            }
            
            // Now get tours for the specific destination
            System.Diagnostics.Debug.WriteLine($"=== STEP 2: Filtering tours for destination {destinationId} ===");
            var response = await client
                .From<Tour>()
                .Where(t => t.IdDestination == destinationId)
                .Get();
                
            System.Diagnostics.Debug.WriteLine($"Filtered response null: {response == null}");
            System.Diagnostics.Debug.WriteLine($"Filtered models null: {response?.Models == null}");
            System.Diagnostics.Debug.WriteLine($"Filtered tours count: {response?.Models?.Count ?? 0}");

            if (response?.Models != null && response.Models.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine("=== TOURS FOUND FOR DESTINATION ===");
                var availableTours = response.Models.Where(t => t.IsAvailable).ToList();
                System.Diagnostics.Debug.WriteLine($"Available tours count: {availableTours.Count}");
                
                foreach (var tour in response.Models)
                {
                    System.Diagnostics.Debug.WriteLine($"Destination Tour: ID={tour.IdTour}, Title='{tour.Title}', Available={tour.IsAvailable}");
                }
                
                var tours = new ObservableCollection<Tour>(availableTours);
                return (true, tours, tours.Count > 0 ? $"Found {tours.Count} tours" : "Tours found but none available");
            }

            System.Diagnostics.Debug.WriteLine($"=== NO TOURS FOUND FOR DESTINATION {destinationId} ===");
            return (false, new ObservableCollection<Tour>(), "No se encontraron tours para este destino");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"=== ERROR IN GetToursByDestinationAsync ===");
            System.Diagnostics.Debug.WriteLine($"Exception: {ex}");
            return (false, new ObservableCollection<Tour>(), $"Error: {ex.Message}");
        }
    }

    public async Task<(bool success, Tour tour, string message)> GetTourByIdAsync(int tourId)
    {
        try
        {
            var client = await _supabaseService.GetClientAsync();
            
            var response = await client
                .From<Tour>()
                .Where(t => t.IdTour == tourId)
                .Get();

            if (response?.Models != null && response.Models.Count > 0)
            {
                var tour = response.Models.First();
                
                var destinationResponse = await client
                    .From<Destination>()
                    .Where(d => d.IdDestination == tour.IdDestination)
                    .Get();

                if (destinationResponse?.Models != null && destinationResponse.Models.Count > 0)
                {
                    tour.Destination = destinationResponse.Models.First();
                }

                return (true, tour, "Tour cargado exitosamente");
            }

            return (false, null, "Tour no encontrado");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading tour: {ex.Message}");
            return (false, null, $"Error: {ex.Message}");
        }
    }

    public async Task<(bool success, ObservableCollection<Tour> tours, string message)> GetAllToursAsync()
    {
        try
        {
            var client = await _supabaseService.GetClientAsync();
            
            var response = await client
                .From<Tour>()
                .Where(t => t.IsAvailable == true)
                .Get();

            if (response?.Models != null)
            {
                var tours = new ObservableCollection<Tour>(response.Models);
                
                foreach (var tour in tours)
                {
                    var destinationResponse = await client
                        .From<Destination>()
                        .Where(d => d.IdDestination == tour.IdDestination)
                        .Get();

                    if (destinationResponse?.Models != null && destinationResponse.Models.Count > 0)
                    {
                        tour.Destination = destinationResponse.Models.First();
                    }
                }

                return (true, tours, "Tours cargados exitosamente");
            }

            return (false, new ObservableCollection<Tour>(), "No se encontraron tours disponibles");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading tours: {ex.Message}");
            return (false, new ObservableCollection<Tour>(), $"Error: {ex.Message}");
        }
    }

    public async Task<(bool success, ObservableCollection<Tour> tours, string message)> SearchToursAsync(string searchTerm)
    {
        try
        {
            var client = await _supabaseService.GetClientAsync();
            
            var query = client
                .From<Tour>()
                .Where(t => t.IsAvailable == true);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(t => t.Title.Contains(searchTerm) || t.Description.Contains(searchTerm));
            }

            var response = await query.Get();

            if (response?.Models != null)
            {
                var tours = new ObservableCollection<Tour>(response.Models);
                
                foreach (var tour in tours)
                {
                    var destinationResponse = await client
                        .From<Destination>()
                        .Where(d => d.IdDestination == tour.IdDestination)
                        .Get();

                    if (destinationResponse?.Models != null && destinationResponse.Models.Count > 0)
                    {
                        tour.Destination = destinationResponse.Models.First();
                    }
                }

                return (true, tours, $"Se encontraron {tours.Count} tours");
            }

            return (false, new ObservableCollection<Tour>(), "No se encontraron tours");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error searching tours: {ex.Message}");
            return (false, new ObservableCollection<Tour>(), $"Error: {ex.Message}");
        }
    }

    public async Task<(bool success, string message)> BookTourAsync(int tourId, DateTime tourDate, int peopleQuantity)
    {
        try
        {
            if (_authService.CurrentUser == null)
            {
                return (false, "Debe iniciar sesión para realizar una reserva");
            }

            var client = await _supabaseService.GetClientAsync();
            
            var tourResponse = await client
                .From<Tour>()
                .Where(t => t.IdTour == tourId)
                .Get();

            if (tourResponse?.Models == null || !tourResponse.Models.Any())
            {
                return (false, "Tour no encontrado");
            }

            var tour = tourResponse.Models.First();

            if (!tour.IsAvailable)
            {
                return (false, "Este tour no está disponible actualmente");
            }

            if (peopleQuantity > tour.MaxCapacity)
            {
                return (false, $"La cantidad de personas excede la capacidad máxima ({tour.MaxCapacity})");
            }

            if (tourDate < DateTime.Now.Date)
            {
                return (false, "No se pueden hacer reservas para fechas pasadas");
            }

            var startDate = tourDate.Date;
            var endDateExclusive = startDate.AddDays(1);

            var existingBookingsResponse = await client
                .From<Booking>()
                .Filter("id_tour", Supabase.Postgrest.Constants.Operator.Equals, tourId)
                .Filter("state", Supabase.Postgrest.Constants.Operator.NotEqual, "cancelled")
                .Get();

            int currentBookings = 0;
            if (existingBookingsResponse?.Models != null)
            {
                currentBookings = existingBookingsResponse.Models
                    .Where(b => b.TourDate >= startDate && b.TourDate < endDateExclusive)
                    .Sum(b => b.PplQuantity);
            }

            if (currentBookings + peopleQuantity > tour.MaxCapacity)
            {
                return (false, $"No hay suficiente capacidad disponible. Quedan {tour.MaxCapacity - currentBookings} espacios");
            }

            var totalPrice = tour.Price * peopleQuantity;

            var newBooking = new Booking
            {
                IdUser = _authService.CurrentUser.IdUser,
                IdTour = tourId,
                ReserveDate = DateTime.UtcNow,
                TourDate = tourDate,
                State = "confirmed",
                PplQuantity = peopleQuantity,
                TotalPrice = totalPrice
            };

            var response = await client
                .From<Booking>()
                .Insert(newBooking);

            if (response?.Models != null && response.Models.Count > 0)
            {
                return (true, $"Reserva confirmada por ${totalPrice:F0} para {peopleQuantity} persona(s)");
            }

            return (false, "Error al procesar la reserva");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error booking tour: {ex.Message}");
            return (false, $"Error: {ex.Message}");
        }
    }

    public async Task<(bool success, ObservableCollection<Booking> bookings, string message)> GetUserBookingsAsync()
    {
        try
        {
            if (_authService.CurrentUser == null)
            {
                return (false, new ObservableCollection<Booking>(), "Debe iniciar sesión");
            }

            System.Diagnostics.Debug.WriteLine($"Loading bookings for user ID: {_authService.CurrentUser.IdUser}");

            var client = await _supabaseService.GetClientAsync();
            
            var response = await client
                .From<Booking>()
                .Where(b => b.IdUser == _authService.CurrentUser.IdUser)
                .Order(b => b.ReserveDate, Supabase.Postgrest.Constants.Ordering.Descending)
                .Get();

            if (response?.Models != null)
            {
                System.Diagnostics.Debug.WriteLine($"Found {response.Models.Count} bookings for user");

                var bookings = new ObservableCollection<Booking>();

                // Load tour information for each booking
                foreach (var booking in response.Models)
                {
                    try
                    {
                        // Get tour details for this booking
                        var tourResponse = await client
                            .From<Tour>()
                            .Where(t => t.IdTour == booking.IdTour)
                            .Get();

                        if (tourResponse?.Models != null && tourResponse.Models.Count > 0)
                        {
                            booking.Tour = tourResponse.Models.First();
                            
                            // Also get destination info for the tour
                            if (booking.Tour.IdDestination > 0)
                            {
                                var destinationResponse = await client
                                    .From<Destination>()
                                    .Where(d => d.IdDestination == booking.Tour.IdDestination)
                                    .Get();

                                if (destinationResponse?.Models != null && destinationResponse.Models.Count > 0)
                                {
                                    booking.Tour.Destination = destinationResponse.Models.First();
                                }
                            }
                            
                            System.Diagnostics.Debug.WriteLine($"Loaded tour info for booking: {booking.Tour.Title}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"Tour not found for booking ID {booking.IdBooking}");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error loading tour for booking {booking.IdBooking}: {ex.Message}");
                    }

                    bookings.Add(booking);
                }

                System.Diagnostics.Debug.WriteLine($"Successfully loaded {bookings.Count} bookings with tour details");
                return (true, bookings, bookings.Count > 0 ? "Reservas cargadas exitosamente" : "No tiene reservas");
            }

            return (true, new ObservableCollection<Booking>(), "No tiene reservas");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading user bookings: {ex}");
            return (false, new ObservableCollection<Booking>(), $"Error: {ex.Message}");
        }
    }

    public async Task<(bool success, string message)> CancelBookingAsync(int bookingId)
    {
        try
        {
            if (_authService.CurrentUser == null)
            {
                return (false, "Debe iniciar sesión");
            }

            var client = await _supabaseService.GetClientAsync();
            
            var bookingResponse = await client
                .From<Booking>()
                .Where(b => b.IdBooking == bookingId && b.IdUser == _authService.CurrentUser.IdUser)
                .Get();

            if (bookingResponse?.Models == null || !bookingResponse.Models.Any())
            {
                return (false, "Reserva no encontrada");
            }

            var booking = bookingResponse.Models.First();

            if (booking.TourDate < DateTime.Now.Date)
            {
                return (false, "No se pueden cancelar reservas de fechas pasadas");
            }

            if (booking.State == "cancelled")
            {
                return (false, "Esta reserva ya está cancelada");
            }

            booking.State = "cancelled";

            await client
                .From<Booking>()
                .Where(b => b.IdBooking == bookingId)
                .Set(b => b.State, "cancelled")
                .Update();

            return (true, "Reserva cancelada exitosamente");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error cancelling booking: {ex.Message}");
            return (false, $"Error: {ex.Message}");
        }
    }

    public async Task<(bool success, int availableSpaces, string message)> GetTourAvailabilityAsync(int tourId, DateTime tourDate)
    {
        try
        {
            var client = await _supabaseService.GetClientAsync();
            
            var tourResponse = await client
                .From<Tour>()
                .Where(t => t.IdTour == tourId)
                .Get();

            if (tourResponse?.Models == null || !tourResponse.Models.Any())
            {
                return (false, 0, "Tour no encontrado");
            }

            var tour = tourResponse.Models.First();

            var startDate = tourDate.Date;
            var endDateExclusive = startDate.AddDays(1);

            var bookingsResponse = await client
                .From<Booking>()
                .Filter("id_tour", Supabase.Postgrest.Constants.Operator.Equals, tourId)
                .Filter("state", Supabase.Postgrest.Constants.Operator.NotEqual, "cancelled")
                .Get();

            int bookedSpaces = 0;
            if (bookingsResponse?.Models != null)
            {
                bookedSpaces = bookingsResponse.Models
                    .Where(b => b.TourDate >= startDate && b.TourDate < endDateExclusive)
                    .Sum(b => b.PplQuantity);
            }

            int availableSpaces = tour.MaxCapacity - bookedSpaces;
            
            return (true, availableSpaces, $"Espacios disponibles: {availableSpaces}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error checking availability: {ex.Message}");
            return (false, 0, $"Error: {ex.Message}");
        }
    }

    public async Task<(bool success, string message)> CreateSampleToursAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("=== CREATING SAMPLE TOURS ===");
            var client = await _supabaseService.GetClientAsync();
            
            // Check if tours already exist
            var existingTours = await client.From<Tour>().Get();
            System.Diagnostics.Debug.WriteLine($"Existing tours check: {existingTours?.Models?.Count ?? 0} tours found");
            
            if (existingTours?.Models?.Count > 0)
            {
                return (true, "Tours ya existen en la base de datos");
            }

            // Create sample tours for the first few destinations (assuming they exist)
            var sampleTours = new List<Tour>
            {
                new Tour
                {
                    IdDestination = 1,
                    Title = "Tour de Snorkeling en Bahía Flamingo",
                    Description = "Explora los arrecifes de coral y la vida marina de Bahía Flamingo en esta aventura de snorkeling de medio día.",
                    Price = 75.00m,
                    Duration = 180, // 3 horas
                    DifficultyLevel = "Facil",
                    Includes = "Equipo de snorkeling, instructor, refrigerios, transporte",
                    Requirements = "Saber nadar básico, edad mínima 8 años",
                    TourPicture = "snorkeling_flamingo.jpg",
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Tour
                {
                    IdDestination = 1,
                    Title = "Sunset Sailing en Bahía Flamingo",
                    Description = "Disfruta de un relajante paseo en velero al atardecer con vista panorámica de la bahía.",
                    Price = 95.00m,
                    Duration = 150, // 2.5 horas
                    DifficultyLevel = "Facil",
                    Includes = "Bebidas, aperitivos, capitán experimentado",
                    Requirements = "No se requiere experiencia previa",
                    TourPicture = "sunset_sailing.jpg",
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Tour
                {
                    IdDestination = 2,
                    Title = "Clases de Surf en Playa Grande",
                    Description = "Aprende a surfear en una de las mejores playas de surf de Costa Rica con instructores certificados.",
                    Price = 60.00m,
                    Duration = 120, // 2 horas
                    DifficultyLevel = "Medio",
                    Includes = "Tabla de surf, traje de neopreno, instructor",
                    Requirements = "Buena condición física, saber nadar",
                    TourPicture = "surf_lessons.jpg",
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            System.Diagnostics.Debug.WriteLine($"Attempting to create {sampleTours.Count} sample tours");
            int successCount = 0;

            foreach (var tour in sampleTours)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"Creating tour: {tour.Title} for destination {tour.IdDestination}");
                    var insertResponse = await client.From<Tour>().Insert(tour);
                    
                    if (insertResponse?.Models != null && insertResponse.Models.Count > 0)
                    {
                        var createdTour = insertResponse.Models.First();
                        System.Diagnostics.Debug.WriteLine($"✅ Successfully created tour: {createdTour.Title} with ID {createdTour.IdTour}");
                        successCount++;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ Failed to create tour: {tour.Title} - No response models");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Exception creating tour {tour.Title}: {ex.Message}");
                }
            }

            System.Diagnostics.Debug.WriteLine($"=== SAMPLE TOUR CREATION COMPLETE: {successCount}/{sampleTours.Count} successful ===");
            return (successCount > 0, $"Se crearon {successCount} tours de ejemplo de {sampleTours.Count} intentados");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"=== ERROR IN CreateSampleToursAsync ===");
            System.Diagnostics.Debug.WriteLine($"Exception: {ex}");
            return (false, $"Error creando tours de ejemplo: {ex.Message}");
        }
    }


}