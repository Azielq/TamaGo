using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TamaGo.Views;

public partial class BookingsPage : ContentPage
{
    public BookingsPage()
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Aquí cargarás las reservas del usuario desde la base de datos
        // Por ahora muestra el estado vacío
    }

    async void OnExploreToursClicked(object sender, EventArgs e)
    {
        var destPage = App.ServiceProvider.GetRequiredService<DestinationsPage>();
        await Navigation.PushAsync(destPage);
    }
}