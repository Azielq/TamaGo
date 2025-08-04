using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TamaGo.Views;

public partial class DestinationsPage : ContentPage
{
    public DestinationsPage()
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Aquí puedes cargar los destinos desde la base de datos
        // Por ahora los datos están hardcodeados en el XAML
    }
}