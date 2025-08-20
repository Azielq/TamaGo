using TamaGo.Services;
using TamaGo.Models;

namespace TamaGo.Views;

public partial class ProfilePage : ContentPage
{
    private readonly AuthService _authService;
    private readonly TourService _tourService;
    private User _currentUser;

    public ProfilePage(AuthService authService, TourService tourService)
    {
        InitializeComponent();
        _authService = authService;
        _tourService = tourService;
        NavigationPage.SetHasNavigationBar(this, false);
        LoadUserProfile();
    }

    private async void LoadUserProfile()
    {
        try
        {
            _currentUser = _authService.CurrentUser;
            if (_currentUser != null)
            {
                // Bind user data
                FirstNameLabel.Text = _currentUser.FirstName ?? "No especificado";
                LastNameLabel.Text = _currentUser.LastName ?? "No especificado";
                UsernameLabel.Text = _currentUser.Username ?? "No especificado";
                EmailLabel.Text = _currentUser.Email ?? "No especificado";
                CountryLabel.Text = _currentUser.Country ?? "No especificado";
                PhoneLabel.Text = _currentUser.Phone ?? "No especificado";

                // Load statistics
                await LoadUserStatistics();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading user profile: {ex.Message}");
            await DisplayAlert("Error", "No se pudo cargar el perfil", "OK");
        }
    }

    private async Task LoadUserStatistics()
    {
        try
        {
            // Load bookings count
            var (bookingsSuccess, bookings, _) = await _tourService.GetUserBookingsAsync();
            if (bookingsSuccess && bookings != null)
            {
                BookingsCountLabel.Text = bookings.Count.ToString();
            }

            // Load favorites count (we'll implement this in FavoriteService)
            // For now, set to 0
            FavoritesCountLabel.Text = "0";
            
            // Load reviews count (placeholder for future implementation)
            ReviewsCountLabel.Text = "0";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading user statistics: {ex.Message}");
        }
    }

    async void OnChangePhotoClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Cambiar Foto", "Esta funcionalidad estará disponible próximamente.", "OK");
    }

    async void OnEditProfileClicked(object sender, EventArgs e)
    {
        try
        {
            var editProfilePage = new EditProfilePage(_authService);
            await Navigation.PushAsync(editProfilePage);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error navigating to edit profile: {ex.Message}");
            await DisplayAlert("Error", "No se pudo abrir la página de edición", "OK");
        }
    }

    async void OnChangePasswordClicked(object sender, EventArgs e)
    {
        try
        {
            var changePasswordPage = new ChangePasswordPage(_authService);
            await Navigation.PushAsync(changePasswordPage);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error navigating to change password: {ex.Message}");
            await DisplayAlert("Error", "No se pudo abrir la página de cambio de contraseña", "OK");
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Refresh data when returning to page
        LoadUserProfile();
    }
}