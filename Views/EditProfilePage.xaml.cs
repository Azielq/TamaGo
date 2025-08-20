using TamaGo.Services;
using TamaGo.Models;

namespace TamaGo.Views;

public partial class EditProfilePage : ContentPage
{
    private readonly AuthService _authService;
    private User _currentUser;

    public EditProfilePage(AuthService authService)
    {
        InitializeComponent();
        _authService = authService;
        NavigationPage.SetHasNavigationBar(this, false);
        LoadUserData();
    }

    private void LoadUserData()
    {
        _currentUser = _authService.CurrentUser;
        if (_currentUser != null)
        {
            FirstNameEntry.Text = _currentUser.FirstName ?? "";
            LastNameEntry.Text = _currentUser.LastName ?? "";
            CountryEntry.Text = _currentUser.Country ?? "";
            PhoneEntry.Text = _currentUser.Phone ?? "";
        }
    }

    async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(FirstNameEntry.Text))
            {
                await DisplayAlert("Error", "El nombre es requerido", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(LastNameEntry.Text))
            {
                await DisplayAlert("Error", "El apellido es requerido", "OK");
                return;
            }

            // Disable button to prevent multiple submissions
            SaveButton.IsEnabled = false;
            SaveButton.Text = "Guardando...";

            // Update user data
            var (success, message) = await _authService.UpdateUserProfileAsync(
                FirstNameEntry.Text.Trim(),
                LastNameEntry.Text.Trim(),
                CountryEntry.Text?.Trim(),
                PhoneEntry.Text?.Trim()
            );

            if (success)
            {
                await DisplayAlert("Éxito", "Perfil actualizado exitosamente", "OK");
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Error", message, "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving profile: {ex.Message}");
            await DisplayAlert("Error", "Ocurrió un error al guardar los cambios", "OK");
        }
        finally
        {
            SaveButton.IsEnabled = true;
            SaveButton.Text = "Guardar Cambios";
        }
    }
}