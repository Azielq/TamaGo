using TamaGo.Models;
using Supabase;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TamaGo.Services;

public class UserService
{
    private readonly SupabaseService _supabaseService;
    private readonly AuthService _authService;

    public UserService(SupabaseService supabaseService, AuthService authService)
    {
        _supabaseService = supabaseService;
        _authService = authService;
    }

    // Obtener todos los usuarios
    public async Task<List<User>> GetAllUsersAsync()
    {
        try
        {
            var client = await _supabaseService.GetClientAsync();
            var response = await client
                .From<User>()
                .Order(u => u.CreatedAt, Supabase.Postgrest.Constants.Ordering.Descending)
                .Get();

            return response.Models ?? new List<User>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al obtener usuarios: {ex.Message}");
            return new List<User>();
        }
    }

    // Obtener usuario por ID
    public async Task<User> GetUserByIdAsync(int userId)
    {
        try
        {
            var client = await _supabaseService.GetClientAsync();
            var response = await client
                .From<User>()
                .Where(u => u.IdUser == userId)
                .Single();

            return response;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al obtener usuario: {ex.Message}");
            return null;
        }
    }

    // Buscar usuarios
    public async Task<List<User>> SearchUsersAsync(string searchTerm)
    {
        try
        {
            var client = await _supabaseService.GetClientAsync();
            searchTerm = searchTerm.ToLower();

            var response = await client
                .From<User>()
                .Filter("firstname", Supabase.Postgrest.Constants.Operator.ILike, $"%{searchTerm}%")
                .Get();

            var users = response.Models ?? new List<User>();

            // También buscar por apellido, username y email
            var responseLastname = await client
                .From<User>()
                .Filter("lastname", Supabase.Postgrest.Constants.Operator.ILike, $"%{searchTerm}%")
                .Get();

            var responseUsername = await client
                .From<User>()
                .Filter("username", Supabase.Postgrest.Constants.Operator.ILike, $"%{searchTerm}%")
                .Get();

            var responseEmail = await client
                .From<User>()
                .Filter("email", Supabase.Postgrest.Constants.Operator.ILike, $"%{searchTerm}%")
                .Get();

            // Combinar resultados y eliminar duplicados
            users.AddRange(responseLastname.Models ?? new List<User>());
            users.AddRange(responseUsername.Models ?? new List<User>());
            users.AddRange(responseEmail.Models ?? new List<User>());

            return users.GroupBy(u => u.IdUser).Select(g => g.First()).ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al buscar usuarios: {ex.Message}");
            return new List<User>();
        }
    }

    // Crear usuario (sin contraseña, usa el AuthService para registro completo)
    public async Task<(bool success, string message, User user)> CreateUserAsync(User newUser)
    {
        try
        {
            var client = await _supabaseService.GetClientAsync();

            // Verificar duplicados
            var existingUsername = await client
                .From<User>()
                .Where(u => u.Username == newUser.Username)
                .Get();

            if (existingUsername.Models?.Count > 0)
            {
                return (false, "El nombre de usuario ya existe", null);
            }

            var existingEmail = await client
                .From<User>()
                .Where(u => u.Email == newUser.Email)
                .Get();

            if (existingEmail.Models?.Count > 0)
            {
                return (false, "El email ya está registrado", null);
            }

            // Crear usuario
            newUser.CreatedAt = DateTime.UtcNow;
            newUser.UpdatedAt = DateTime.UtcNow;

            var response = await client
                .From<User>()
                .Insert(newUser);

            if (response.Models?.Count > 0)
            {
                return (true, "Usuario creado exitosamente", response.Models.First());
            }

            return (false, "Error al crear usuario", null);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al crear usuario: {ex.Message}");
            return (false, $"Error: {ex.Message}", null);
        }
    }

    // Actualizar usuario
    public async Task<(bool success, string message)> UpdateUserAsync(User user)
    {
        try
        {
            var client = await _supabaseService.GetClientAsync();

            // No permitir cambiar username si ya existe
            var existingUsername = await client
                .From<User>()
                .Where(u => u.Username == user.Username && u.IdUser != user.IdUser)
                .Get();

            if (existingUsername.Models?.Count > 0)
            {
                return (false, "El nombre de usuario ya existe");
            }

            // No permitir cambiar email si ya existe
            var existingEmail = await client
                .From<User>()
                .Where(u => u.Email == user.Email && u.IdUser != user.IdUser)
                .Get();

            if (existingEmail.Models?.Count > 0)
            {
                return (false, "El email ya está registrado");
            }

            user.UpdatedAt = DateTime.UtcNow;

            await client
                .From<User>()
                .Where(u => u.IdUser == user.IdUser)
                .Update(user);

            return (true, "Usuario actualizado exitosamente");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al actualizar usuario: {ex.Message}");
            return (false, $"Error: {ex.Message}");
        }
    }

    // Eliminar usuario
    public async Task<(bool success, string message)> DeleteUserAsync(int userId)
    {
        try
        {
            // No permitir eliminar el usuario actual
            if (_authService.CurrentUser?.IdUser == userId)
            {
                return (false, "No puedes eliminar tu propio usuario");
            }

            var client = await _supabaseService.GetClientAsync();

            await client
                .From<User>()
                .Where(u => u.IdUser == userId)
                .Delete();

            return (true, "Usuario eliminado exitosamente");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al eliminar usuario: {ex.Message}");
            return (false, $"Error: {ex.Message}");
        }
    }

    // Cambiar contraseña
    public async Task<(bool success, string message)> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        try
        {
            var client = await _supabaseService.GetClientAsync();
            
            // Verificar contraseña actual
            var user = await GetUserByIdAsync(userId);
            if (user == null)
            {
                return (false, "Usuario no encontrado");
            }

            var currentHash = HashPassword(currentPassword);
            if (user.PasswordHash != currentHash)
            {
                return (false, "La contraseña actual es incorrecta");
            }

            // Actualizar contraseña
            user.PasswordHash = HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await client
                .From<User>()
                .Where(u => u.IdUser == userId)
                .Update(user);

            return (true, "Contraseña actualizada exitosamente");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al cambiar contraseña: {ex.Message}");
            return (false, $"Error: {ex.Message}");
        }
    }

    // Método auxiliar para hash de contraseña (mismo que en AuthService)
    private string HashPassword(string password)
    {
        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password + "TamaGoSalt2025"));
            var builder = new System.Text.StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }
}