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
                    .Where(u => u.Username == username)
                    .Where(u => u.PasswordHash == passwordHash)
                    .Get();
                
                var users = response.Models;
                
                if (users != null && users.Count > 0)
                {
                    _currentUser = users.First();
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
                newUser.CreatedAt = DateTime.UtcNow;
                newUser.UpdatedAt = DateTime.UtcNow;
                
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
    }