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
    // 1. Validaciones
    if (string.IsNullOrWhiteSpace(FirstNameEntry.Text) ||
        string.IsNullOrWhiteSpace(LastNameEntry.Text)  ||
        string.IsNullOrWhiteSpace(UsernameEntry.Text)  ||
        string.IsNullOrWhiteSpace(PasswordEntry.Text))
    {
        await DisplayAlert("Error", "Por favor completa todos los campos obligatorios", "OK");
        return;
    }

    if (PasswordEntry.Text != ConfirmPasswordEntry.Text)
    {
        await DisplayAlert("Error", "Las contraseñas no coinciden", "OK");
        return;
    }

    // 2. Feedback de carga
    LoadingOverlay.IsVisible = true;
    RegisterButton.IsEnabled = false;

    try
    {
        var newUser = new User
        {
            Username   = UsernameEntry.Text.Trim().ToLower(),
            Email      = $"{UsernameEntry.Text.Trim().ToLower()}@tamago.com", // o usa un Entry para email
            FirstName  = FirstNameEntry.Text.Trim(),
            LastName   = LastNameEntry.Text.Trim(),
            Country    = CountryPicker.SelectedItem?.ToString(),
            Phone      = PhoneEntry.Text?.Trim(),
            BirthDate  = BirthDatePicker.Date
            // CreatedAt / UpdatedAt los pone el trigger del server
        };

        var (ok, msg) = await _authService.RegisterAsync(newUser, PasswordEntry.Text);

        if (ok)
        {
            await DisplayAlert("Éxito", msg, "OK");
            await Navigation.PopAsync(); // vuelve al login
        }
        else
        {
            await DisplayAlert("Error", msg, "OK");
        }
    }
    finally
    {
        LoadingOverlay.IsVisible = false;
        RegisterButton.IsEnabled = true;
    }
}

}