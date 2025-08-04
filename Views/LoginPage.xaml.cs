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

        // Mostrar loading
        LoadingOverlay.IsVisible = true;
        LoadingLabel.Text = "Verificando credenciales...";
        LoginButton.IsEnabled = false;
        
        try
        {
            var (success, message) = await _authService.LoginAsync(
                UsernameEntry.Text.Trim(), 
                PasswordEntry.Text);
            
            if (success)
            {
                var homePage = App.ServiceProvider.GetRequiredService<HomePage>();
                await Navigation.PushAsync(homePage);
                
                // Limpiar campos
                UsernameEntry.Text = "";
                PasswordEntry.Text = "";
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
            LoginButton.IsEnabled = true;
        }
    }

    async void OnSignUpTapped(object sender, EventArgs e)
    {
        var registerPage = App.ServiceProvider.GetRequiredService<RegisterPage>();
        await Navigation.PushAsync(registerPage);
    }
}