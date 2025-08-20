using TamaGo.Services;

namespace TamaGo.Views;

public partial class ChangePasswordPage : ContentPage
{
    private readonly AuthService _authService;

    public ChangePasswordPage(AuthService authService)
    {
        InitializeComponent();
        _authService = authService;
        NavigationPage.SetHasNavigationBar(this, false);
    }

    async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    async void OnChangePasswordClicked(object sender, EventArgs e)
    {
        try
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(CurrentPasswordEntry.Text))
            {
                await DisplayAlert("Error", "Por favor ingresa tu contraseña actual", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(NewPasswordEntry.Text))
            {
                await DisplayAlert("Error", "Por favor ingresa tu nueva contraseña", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(ConfirmPasswordEntry.Text))
            {
                await DisplayAlert("Error", "Por favor confirma tu nueva contraseña", "OK");
                return;
            }

            if (NewPasswordEntry.Text != ConfirmPasswordEntry.Text)
            {
                await DisplayAlert("Error", "Las contraseñas no coinciden", "OK");
                return;
            }

            if (NewPasswordEntry.Text.Length < 6)
            {
                await DisplayAlert("Error", "La nueva contraseña debe tener al menos 6 caracteres", "OK");
                return;
            }

            // Disable button to prevent multiple submissions
            ChangePasswordButton.IsEnabled = false;
            ChangePasswordButton.Text = "Cambiando...";

            // Change password (AuthService will verify the current password)
            System.Diagnostics.Debug.WriteLine($"ChangePasswordPage: About to call ChangePasswordAsync");
            System.Diagnostics.Debug.WriteLine($"ChangePasswordPage: Current password length: {CurrentPasswordEntry.Text?.Length}");
            System.Diagnostics.Debug.WriteLine($"ChangePasswordPage: New password length: {NewPasswordEntry.Text?.Length}");
            
            var (success, message) = await _authService.ChangePasswordAsync(CurrentPasswordEntry.Text, NewPasswordEntry.Text);
            
            System.Diagnostics.Debug.WriteLine($"ChangePasswordPage: ChangePasswordAsync returned - Success: {success}, Message: '{message}'");
            
            if (success)
            {
                await DisplayAlert("Éxito", "Contraseña cambiada exitosamente", "OK");
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Error", message, "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error changing password: {ex.Message}");
            await DisplayAlert("Error", "Ocurrió un error al cambiar la contraseña", "OK");
        }
        finally
        {
            ChangePasswordButton.IsEnabled = true;
            ChangePasswordButton.Text = "Cambiar Contraseña";
        }
    }

}