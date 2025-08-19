using TamaGo.Views;

namespace TamaGo;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        
        // Registrar rutas para navegación
        Routing.RegisterRoute("destinationdetail", typeof(DestinationDetailPage));
        Routing.RegisterRoute("tourdetail", typeof(TourDetailPage));
    }
}