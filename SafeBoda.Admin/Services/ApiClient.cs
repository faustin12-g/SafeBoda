using SafeBoda.Admin.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace SafeBoda.Admin.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly AuthService _authService;

    public ApiClient(HttpClient httpClient, AuthService authService)
    {
        _httpClient = httpClient;
        _authService = authService;
    }

    private async Task SetAuthHeaderAsync()
    {
        var token = await _authService.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<TripsResponse?> GetTripsAsync()
    {
        await SetAuthHeaderAsync();
        try
        {
            return await _httpClient.GetFromJsonAsync<TripsResponse>("api/trips");
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<Driver>> GetDriversAsync()
    {
        await SetAuthHeaderAsync();
        try
        {
            var drivers = await _httpClient.GetFromJsonAsync<List<Driver>>("api/admin/drivers");
            return drivers ?? new List<Driver>();
        }
        catch
        {
            return new List<Driver>();
        }
    }

    public async Task<DashboardStats?> GetStatsAsync()
    {
        await SetAuthHeaderAsync();
        try
        {
            return await _httpClient.GetFromJsonAsync<DashboardStats>("api/admin/stats");
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> DeleteTripAsync(Guid tripId)
    {
        await SetAuthHeaderAsync();
        try
        {
            var response = await _httpClient.DeleteAsync($"api/trips/{tripId}");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<Trip?> CreateTripAsync(Trip trip)
    {
        await SetAuthHeaderAsync();
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/trips", trip);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Trip>();
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<Trip?> UpdateTripAsync(Guid tripId, Trip trip)
    {
        await SetAuthHeaderAsync();
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/trips/{tripId}", trip);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Trip>();
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    // Riders
    public async Task<List<Rider>> GetRidersAsync()
    {
        await SetAuthHeaderAsync();
        try
        {
            var riders = await _httpClient.GetFromJsonAsync<List<Rider>>("api/admin/riders");
            return riders ?? new List<Rider>();
        }
        catch
        {
            return new List<Rider>();
        }
    }

    public async Task<Rider?> CreateRiderAsync(Rider rider)
    {
        await SetAuthHeaderAsync();
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/riders", rider);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Rider>();
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<Rider?> UpdateRiderAsync(Guid riderId, Rider rider)
    {
        await SetAuthHeaderAsync();
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/riders/{riderId}", rider);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Rider>();
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> DeleteRiderAsync(Guid riderId)
    {
        await SetAuthHeaderAsync();
        try
        {
            var response = await _httpClient.DeleteAsync($"api/riders/{riderId}");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    // Drivers
    public async Task<Driver?> CreateDriverAsync(Driver driver)
    {
        await SetAuthHeaderAsync();
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/drivers", driver);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Driver>();
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<Driver?> UpdateDriverAsync(Guid driverId, Driver driver)
    {
        await SetAuthHeaderAsync();
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/drivers/{driverId}", driver);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Driver>();
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> DeleteDriverAsync(Guid driverId)
    {
        await SetAuthHeaderAsync();
        try
        {
            var response = await _httpClient.DeleteAsync($"api/drivers/{driverId}");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    // Users
    public async Task<List<User>> GetUsersAsync()
    {
        await SetAuthHeaderAsync();
        try
        {
            var users = await _httpClient.GetFromJsonAsync<List<User>>("api/admin/users");
            return users ?? new List<User>();
        }
        catch
        {
            return new List<User>();
        }
    }

    public async Task<User?> CreateUserAsync(CreateUserRequest request)
    {
        await SetAuthHeaderAsync();
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/admin/users", request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<User>();
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> DeleteUserAsync(string userId)
    {
        await SetAuthHeaderAsync();
        try
        {
            var response = await _httpClient.DeleteAsync($"api/admin/users/{userId}");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
