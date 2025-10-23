// ConfigurationReader.AdminPanel/Services/ConfigurationApiService.cs
using System.Net.Http.Json;
using ConfigurationReader.AdminPanel.Models;

namespace ConfigurationReader.AdminPanel.Services;

public class ConfigurationApiService
{
    private readonly HttpClient _httpClient;

    public ConfigurationApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        Console.WriteLine($"[ConfigurationApiService] Initialized with BaseAddress: {_httpClient.BaseAddress}");
    }

    public async Task<List<ConfigurationItemDto>> GetAllAsync(string? applicationName = null)
    {
        try
        {
            var url = string.IsNullOrWhiteSpace(applicationName)
                ? "api/configurations"
                : $"api/configurations?applicationName={applicationName}";

            Console.WriteLine($"[GetAllAsync] Fetching: {_httpClient.BaseAddress}{url}");

            var response = await _httpClient.GetAsync(url);

            Console.WriteLine($"[GetAllAsync] Status: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[GetAllAsync] Error: {errorContent}");
                return new List<ConfigurationItemDto>();
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResult<List<ConfigurationItemDto>>>();

            Console.WriteLine($"[GetAllAsync] Success: {result?.Data?.Count ?? 0} items");

            return result?.Data ?? new List<ConfigurationItemDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GetAllAsync] Exception: {ex.Message}");
            Console.WriteLine($"[GetAllAsync] StackTrace: {ex.StackTrace}");
            return new List<ConfigurationItemDto>();
        }
    }

    public async Task<ConfigurationItemDto?> GetByIdAsync(int id)
    {
        try
        {
            Console.WriteLine($"[GetByIdAsync] Fetching ID: {id}");

            var response = await _httpClient.GetAsync($"api/configurations/{id}");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ApiResult<ConfigurationItemDto>>();
            return result?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GetByIdAsync] Exception: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> CreateAsync(CreateConfigurationDto dto)
    {
        try
        {
            Console.WriteLine($"[CreateAsync] Creating: {dto.Name}");

            var response = await _httpClient.PostAsJsonAsync("api/configurations", dto);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[CreateAsync] Error: {error}");
            }

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CreateAsync] Exception: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UpdateAsync(int id, UpdateConfigurationDto dto)
    {
        try
        {
            Console.WriteLine($"[UpdateAsync] Updating ID: {id}");

            var response = await _httpClient.PutAsJsonAsync($"api/configurations/{id}", dto);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[UpdateAsync] Error: {error}");
            }

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[UpdateAsync] Exception: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            Console.WriteLine($"[DeleteAsync] Deleting ID: {id}");

            var response = await _httpClient.DeleteAsync($"api/configurations/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DeleteAsync] Exception: {ex.Message}");
            return false;
        }
    }

    public async Task<List<string>> GetApplicationNamesAsync()
    {
        try
        {
            Console.WriteLine("[GetApplicationNamesAsync] Fetching application names");

            var response = await _httpClient.GetAsync("api/configurations/applications");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ApiResult<List<string>>>();

            Console.WriteLine($"[GetApplicationNamesAsync] Found: {result?.Data?.Count ?? 0} applications");

            return result?.Data ?? new List<string>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GetApplicationNamesAsync] Exception: {ex.Message}");
            return new List<string>();
        }
    }
}