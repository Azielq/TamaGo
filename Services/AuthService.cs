using TamaGo.Models;
using System.Security.Cryptography;
using System.Text;
using Supabase;

namespace TamaGo.Services;

public class AuthService
    {
        private readonly SupabaseService _supabaseService;
        private User _currentUser;
        
        public AuthService(SupabaseService supabaseService)
        {
            _supabaseService = supabaseService;
        }
        
        public User CurrentUser => _currentUser;
        
        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password + "TamaGoSalt2025"));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
        
        public async Task<(bool success, string message)> LoginAsync(string username, string password)
        {
            try
            {
                var client = await _supabaseService.GetClientAsync();
                var passwordHash = HashPassword(password);
                
                var response = await client
                    .From<User>()
                    .Where(u => u.Username == username || u.Email == username)
                    .Where(u => u.PasswordHash == passwordHash)
                    .Get();
                
                var users = response.Models;
                
                if (users != null && users.Count > 0)
                {
                    _currentUser = users.First();
                    System.Diagnostics.Debug.WriteLine($"Login successful - User ID: {_currentUser.IdUser}");
                    System.Diagnostics.Debug.WriteLine($"Login successful - Username: {_currentUser.Username}");
                    System.Diagnostics.Debug.WriteLine($"Login successful - Stored password hash: '{_currentUser.PasswordHash}'");
                    return (true, "Login exitoso");
                }
                
                return (false, "Usuario o contraseña incorrectos");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en login: {ex.Message}");
                return (false, $"Error de conexión: {ex.Message}");
            }
        }
        
        public async Task<(bool success, string message)> RegisterAsync(User newUser, string password)
        {
            try
            {
                var client = await _supabaseService.GetClientAsync();
                
                // Verificar si el usuario ya existe
                var existingUserResponse = await client
                    .From<User>()
                    .Where(u => u.Username == newUser.Username)
                    .Get();
                
                if (existingUserResponse.Models != null && existingUserResponse.Models.Count > 0)
                {
                    return (false, "El nombre de usuario ya existe");
                }
                
                // Verificar si el email ya existe
                var existingEmailResponse = await client
                    .From<User>()
                    .Where(u => u.Email == newUser.Email)
                    .Get();
                
                if (existingEmailResponse.Models != null && existingEmailResponse.Models.Count > 0)
                {
                    return (false, "El email ya está registrado");
                }
                
                // Crear nuevo usuario
                newUser.PasswordHash = HashPassword(password);
                
                var response = await client
                    .From<User>()
                    .Insert(newUser);
                
                if (response.Models != null && response.Models.Count > 0)
                {
                    return (true, "Usuario registrado exitosamente");
                }
                
                return (false, "Error al registrar usuario");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en registro: {ex.Message}");
                return (false, $"Error: {ex.Message}");
            }
        }
        
        public void Logout()
        {
            _currentUser = null;
        }

        public async Task<(bool success, string message)> ChangePasswordAsync(string currentPassword, string newPassword)
        {
            try
            {
                if (_currentUser == null)
                {
                    return (false, "No hay usuario logueado");
                }

                var currentPasswordHash = HashPassword(currentPassword);
                
                // Debug logging
                System.Diagnostics.Debug.WriteLine($"Current password entered: '{currentPassword}'");
                System.Diagnostics.Debug.WriteLine($"Current password hash calculated: '{currentPasswordHash}'");
                System.Diagnostics.Debug.WriteLine($"Stored password hash: '{_currentUser.PasswordHash}'");
                System.Diagnostics.Debug.WriteLine($"Hashes match: {currentPasswordHash == _currentUser.PasswordHash}");
                
                if (currentPasswordHash != _currentUser.PasswordHash)
                {
                    return (false, "La contraseña actual es incorrecta");
                }

                var newPasswordHash = HashPassword(newPassword);
                var client = await _supabaseService.GetClientAsync();

                System.Diagnostics.Debug.WriteLine($"Attempting to update user ID: {_currentUser.IdUser}");
                System.Diagnostics.Debug.WriteLine($"New password hash: {newPasswordHash}");

                var response = await client
                    .From<User>()
                    .Where(u => u.IdUser == _currentUser.IdUser)
                    .Set(u => u.PasswordHash, newPasswordHash)
                    .Set(u => u.UpdatedAt, DateTime.UtcNow)
                    .Update();

                if (response?.Models != null && response.Models.Count > 0)
                {
                    // Solo actualizar el objeto en memoria DESPUÉS de que la BD se actualice exitosamente
                    _currentUser.PasswordHash = newPasswordHash;
                    _currentUser.UpdatedAt = DateTime.UtcNow;
                    
                    System.Diagnostics.Debug.WriteLine("Password successfully updated in database and memory");
                    return (true, "Contraseña cambiada exitosamente");
                }

                System.Diagnostics.Debug.WriteLine($"Database update failed. Response models count: {response?.Models?.Count ?? 0}");
                return (false, "Error al cambiar la contraseña en la base de datos");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error changing password: {ex.Message}");
                return (false, $"Error: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> UpdateUserProfileAsync(string firstName, string lastName, string country, string phone)
        {
            try
            {
                if (_currentUser == null)
                {
                    return (false, "No hay usuario logueado");
                }

                var client = await _supabaseService.GetClientAsync();

                // Create a user object with the updated values for the database update
                var userUpdate = new User
                {
                    IdUser = _currentUser.IdUser,
                    FirstName = firstName,
                    LastName = lastName,
                    Country = country,
                    Phone = phone,
                    UpdatedAt = DateTime.UtcNow
                };

                var response = await client
                    .From<User>()
                    .Where(u => u.IdUser == _currentUser.IdUser)
                    .Update(userUpdate);

                if (response?.Models != null && response.Models.Count > 0)
                {
                    // Only update in-memory object after successful database update
                    _currentUser.FirstName = firstName;
                    _currentUser.LastName = lastName;
                    _currentUser.Country = country;
                    _currentUser.Phone = phone;
                    _currentUser.UpdatedAt = DateTime.UtcNow;
                    
                    return (true, "Perfil actualizado exitosamente");
                }

                return (false, "Error al actualizar el perfil");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating profile: {ex.Message}");
                return (false, $"Error: {ex.Message}");
            }
        }
    }