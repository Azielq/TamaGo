using Supabase;
using TamaGo.Configuration;

namespace TamaGo.Services;

public class SupabaseService
{
    private Client _supabaseClient;
    private bool _isInitialized = false;
        
    public async Task<Client> GetClientAsync()
    {
        if (!_isInitialized)
        {
            try
            {
                var options = new SupabaseOptions
                {
                    AutoConnectRealtime = false,
                    AutoRefreshToken = true,
                };
                    
                _supabaseClient = new Client(
                    SupabaseConfig.SUPABASE_URL, 
                    SupabaseConfig.SUPABASE_ANON_KEY, 
                    options);
                    
                await _supabaseClient.InitializeAsync();
                _isInitialized = true;
                System.Diagnostics.Debug.WriteLine("Supabase client initialized successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing Supabase client: {ex}");
                throw;
            }
        }
            
        return _supabaseClient;
    }
}