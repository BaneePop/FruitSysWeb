using System.Text.Json;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;

namespace FruitSysWeb.Services.Core
{
    public interface ILocalStorageService
    {
        Task<T?> GetItemAsync<T>(string key);
        Task SetItemAsync<T>(string key, T value);
        Task RemoveItemAsync(string key);
        Task ClearAsync();
    }

    public class LocalStorageService : ILocalStorageService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly ILogger<LocalStorageService> _logger;

        public LocalStorageService(IJSRuntime jsRuntime, ILogger<LocalStorageService> logger)
        {
            _jsRuntime = jsRuntime;
            _logger = logger;
        }

        public async Task<T?> GetItemAsync<T>(string key)
        {
            try
            {
                var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
                
                if (string.IsNullOrEmpty(json))
                    return default;

                return JsonSerializer.Deserialize<T>(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri učitavanju iz LocalStorage");
                return default;
            }
        }

        public async Task SetItemAsync<T>(string key, T value)
        {
            try
            {
                var json = JsonSerializer.Serialize(value);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri čuvanju u LocalStorage");
            }
        }

        public async Task RemoveItemAsync(string key)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri brisanju iz LocalStorage");
            }
        }

        public async Task ClearAsync()
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.clear");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri čišćenju LocalStorage");
            }
        }
    }
}