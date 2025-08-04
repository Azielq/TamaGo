using TamaGo.Models;
using TamaGo.Services;

namespace TamaGo.Views;

public partial class RegisterPage : ContentPage
{
    private readonly AuthService _authService;

    public RegisterPage(AuthService authService)
    {
        InitializeComponent();
        _authService = authService;
        CountryPicker.SelectedIndex = 0;
    }

    async void OnRegisterClicked(object sender, EventArgs e)
    {
        // Validaciones básicas
        if (string.IsNullOrWhiteSpace(UsernameEntry.Text) ||
            string.IsNullOrWhiteSpace(PasswordEntry.Text) ||
            string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            await DisplayAlert("Error", "Por favor complete todos los campos obligatorios", "OK");
            return;
        }

        if (PasswordEntry.Text != ConfirmPasswordEntry.Text)
        {
            await DisplayAlert("Error", "Las contraseñas no coinciden", "OK");
            return;
        }

        if (PasswordEntry.Text.Length < 6)
        {
            await DisplayAlert("Error", "La contraseña debe tener al menos 6 caracteres", "OK");
            return;
        }

        // Mostrar loading
        LoadingOverlay.IsVisible = true;
        RegisterButton.IsEnabled = false;

        try
        {
            var newUser = new User
            {
                Username = UsernameEntry.Text.ToLower().Trim(),
                Email = $"{UsernameEntry.Text.ToLower().Trim()}@tamago.com",
                FullName = NameEntry.Text.Trim(),
                PhoneNumber = PhoneEntry.Text?.Trim(),
                DateOfBirth = BirthDatePicker.Date,
                Country = CountryPicker.SelectedItem?.ToString() ?? "Costa Rica"
            };

            var (success, message) = await _authService.RegisterAsync(newUser, PasswordEntry.Text);

            if (success)
            {
                await DisplayAlert("Éxito", message, "OK");
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Error", message, "OK");
            }
        }
        finally
        {
            // Ocultar loading
            LoadingOverlay.IsVisible = false;
            RegisterButton.IsEnabled = true;
        }
    }
}