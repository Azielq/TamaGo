using TamaGo.Services;
using Microsoft.Extensions.DependencyInjection;

namespace TamaGo.Views;
public partial class LoginPage : ContentPage
{
    private readonly AuthService _authService;

    public LoginPage(AuthService authService)
    {
        InitializeComponent();
        _authService = authService;
    }

    async void OnLoginClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(UsernameEntry.Text) ||
            string.IsNullOrWhiteSpace(PasswordEntry.Text))
        {
            await DisplayAlert("Error", "Por favor complete todos los campos", "OK");
            return;
        }

        // ⬇️ Mostrar loading
        LoadingOverlay.IsVisible = true;
        LoadingLabel.Text = "Verificando credenciales…";
        LoginButton.IsEnabled = false;

        try
        {
            var (success, message) = await _authService.LoginAsync(
                UsernameEntry.Text.Trim(),
                PasswordEntry.Text);

            if (success)
            {
                // ✅ Oculta el overlay ANTES de navegar
                LoadingOverlay.IsVisible = false;

                var homePage = App.ServiceProvider.GetRequiredService<MenuPage>();
                await Navigation.PushAsync(homePage);

                UsernameEntry.Text = string.Empty;
                PasswordEntry.Text = string.Empty;
            }
            else
            {
                await DisplayAlert("Error", message, "OK");
            }
        }
        finally
        {
            // 🔄 Siempre re-habilita y oculta, ocurra lo que ocurra
            LoginButton.IsEnabled = true;
            LoadingOverlay.IsVisible = false;
        }
    }
    
    
    
    
    async void OnSignUpTapped(object sender, EventArgs e)
    {
        var registerPage = App.ServiceProvider.GetRequiredService<RegisterPage>();
        await Navigation.PushAsync(registerPage);
    }
}