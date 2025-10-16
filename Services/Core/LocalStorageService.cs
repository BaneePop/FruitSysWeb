using System.Text.Json;
using Microsoft.JSInterop;

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

        public LocalStorageService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
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
                Console.WriteLine($"Greška pri učitavanju iz LocalStorage: {ex.Message}");
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
                Console.WriteLine($"Greška pri čuvanju u LocalStorage: {ex.Message}");
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
                Console.WriteLine($"Greška pri brisanju iz LocalStorage: {ex.Message}");
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
                Console.WriteLine($"Greška pri čišćenju LocalStorage: {ex.Message}");
            }
        }
    }
}