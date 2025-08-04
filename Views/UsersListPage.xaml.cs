using TamaGo.Models;
using TamaGo.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TamaGo.Views;

public partial class UsersListPage : ContentPage, INotifyPropertyChanged
{
    private readonly UserService _userService;
    private ObservableCollection<User> _allUsers;
    private ObservableCollection<User> _filteredUsers;
    private bool _isRefreshing;

    public ObservableCollection<User> FilteredUsers
    {
        get => _filteredUsers;
        set
        {
            _filteredUsers = value;
            OnPropertyChanged();
        }
    }

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set
        {
            _isRefreshing = value;
            OnPropertyChanged();
        }
    }

    public UsersListPage(UserService userService)
    {
        InitializeComponent();
        _userService = userService;
        _allUsers = new ObservableCollection<User>();
        _filteredUsers = new ObservableCollection<User>();
        BindingContext = this;
        NavigationPage.SetHasNavigationBar(this, false);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadUsers();
    }

    private async Task LoadUsers()
    {
        try
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;
            UsersCollectionView.IsVisible = false;

            var users = await _userService.GetAllUsersAsync();
            _allUsers = new ObservableCollection<User>(users);
            FilteredUsers = new ObservableCollection<User>(_allUsers);

            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
            UsersCollectionView.IsVisible = true;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudieron cargar los usuarios: {ex.Message}", "OK");
        }
    }

    private async void OnRefreshing(object sender, EventArgs e)
    {
        IsRefreshing = true;
        await LoadUsers();
        IsRefreshing = false;
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        var searchTerm = e.NewTextValue?.ToLower() ?? "";

        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            FilteredUsers = new ObservableCollection<User>(_allUsers);
        }
        else
        {
            var filtered = _allUsers.Where(u =>
                u.FirstName?.ToLower().Contains(searchTerm) == true ||
                u.LastName?.ToLower().Contains(searchTerm) == true ||
                u.Username?.ToLower().Contains(searchTerm) == true ||
                u.Email?.ToLower().Contains(searchTerm) == true
            ).ToList();

            FilteredUsers = new ObservableCollection<User>(filtered);
        }
    }

    private async void OnAddUserClicked(object sender, EventArgs e)
    {
        var editPage = App.ServiceProvider.GetRequiredService<UserEditPage>();
        await Navigation.PushAsync(editPage);
    }

    private async void OnUserTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is User user)
        {
            var detailPage = App.ServiceProvider.GetRequiredService<UserDetailPage>();
            detailPage.SetUser(user);
            await Navigation.PushAsync(detailPage);
        }
    }

    private async void OnEditClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is User user)
        {
            var editPage = App.ServiceProvider.GetRequiredService<UserEditPage>();
            editPage.SetUser(user);
            await Navigation.PushAsync(editPage);
        }
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is User user)
        {
            bool answer = await DisplayAlert(
                "Confirmar eliminación", 
                $"¿Estás seguro de eliminar al usuario {user.FirstName} {user.LastName}?", 
                "Eliminar", 
                "Cancelar");

            if (answer)
            {
                var result = await _userService.DeleteUserAsync(user.IdUser);
                
                if (result.success)
                {
                    await DisplayAlert("Éxito", result.message, "OK");
                    await LoadUsers();
                }
                else
                {
                    await DisplayAlert("Error", result.message, "OK");
                }
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}