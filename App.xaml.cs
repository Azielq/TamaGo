using TamaGo.Views;

namespace TamaGo;

public partial class App : Application
{
    public App(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        ServiceProvider = serviceProvider;
    }

    public static IServiceProvider ServiceProvider { get; private set; }

    protected override Window CreateWindow(IActivationState activationState)
    {
        var loginPage = ServiceProvider.GetRequiredService<LoginPage>();
        return new Window(new NavigationPage(loginPage));
    }
}