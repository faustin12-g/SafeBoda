using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SafeBoda.Api.Models;
using SafeBoda.Core;
using SafeBoda.Infrastructure.Data;
using SafeBoda.Infrastructure.Entities;
using System.IdentityModel.Tokens.Jwt;

namespace SafeBoda.Api.Tests;

public class TripsControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly string _testUserId;
    private readonly string _testUserEmail;

    public TripsControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _testUserId = Guid.NewGuid().ToString();
        _testUserEmail = "test@example.com";
        
        _client = _factory.CreateClient();
        
        // Set up authentication token
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {GenerateTestToken()}");
    }

    private string GenerateTestToken()
    {
        using var scope = _factory.Services.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var secretKey = configuration["JwtSettings:SecretKey"]!;
        var issuer = configuration["JwtSettings:Issuer"]!;
        var audience = configuration["JwtSettings:Audience"]!;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId),
            new Claim(ClaimTypes.Email, _testUserEmail),
            new Claim(ClaimTypes.Name, "Test User")
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private SafeBodaDbContext GetDbContext()
    {
        var scope = _factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<SafeBodaDbContext>();
    }

    #region GetActiveTrips Tests

    [Fact]
    public async Task GetActiveTrips_ShouldReturnOkWithTrips_WhenTripsExist()
    {
        // Arrange - Clean database first to ensure we only have our test data
        var cleanupScope = _factory.Services.CreateScope();
        var cleanupContext = cleanupScope.ServiceProvider.GetRequiredService<SafeBodaDbContext>();
        cleanupContext.Trips.RemoveRange(cleanupContext.Trips);
        await cleanupContext.SaveChangesAsync();
        cleanupScope.Dispose();

        var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SafeBodaDbContext>();
        
        var trip1 = new TripEntity
        {
            Id = Guid.NewGuid(),
            RiderId = Guid.NewGuid(),
            DriverId = Guid.NewGuid(),
            StartLatitude = 0.0,
            StartLongitude = 0.0,
            EndLatitude = 1.0,
            EndLongitude = 1.0,
            Fare = 1000m,
            RequestTime = DateTime.UtcNow
        };

        var trip2 = new TripEntity
        {
            Id = Guid.NewGuid(),
            RiderId = Guid.NewGuid(),
            DriverId = Guid.NewGuid(),
            StartLatitude = 2.0,
            StartLongitude = 2.0,
            EndLatitude = 3.0,
            EndLongitude = 3.0,
            Fare = 2000m,
            RequestTime = DateTime.UtcNow
        };

        dbContext.Trips.AddRange(trip1, trip2);
        await dbContext.SaveChangesAsync();
        scope.Dispose(); // Dispose scope but data should persist in in-memory DB

        // Act
        var response = await _client.GetAsync("/api/trips");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.TryGetProperty("trips", out var tripsArray));
        var trips = tripsArray.EnumerateArray().ToList();
        Assert.Equal(2, trips.Count);
        var tripIds = trips.Select(t => t.GetProperty("id").GetString()).ToList();
        Assert.Contains(trip1.Id.ToString(), tripIds);
        Assert.Contains(trip2.Id.ToString(), tripIds);
    }

    [Fact]
    public async Task GetActiveTrips_ShouldReturnOkWithEmptyList_WhenNoTripsExist()
    {
        // Arrange - ensure database is empty
        using var dbContext = GetDbContext();
        dbContext.Trips.RemoveRange(dbContext.Trips);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/trips");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.TryGetProperty("trips", out var trips));
        Assert.Equal(JsonValueKind.Array, trips.ValueKind);
    }

    [Fact]
    public async Task GetActiveTrips_ShouldIncludeAuthenticatedUserInfo()
    {
        // Act
        var response = await _client.GetAsync("/api/trips");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.TryGetProperty("authenticatedUser", out var user));
        Assert.True(user.TryGetProperty("userId", out var userId));
        Assert.True(user.TryGetProperty("userEmail", out var userEmail));
        Assert.Equal(_testUserId, userId.GetString());
        Assert.Equal(_testUserEmail, userEmail.GetString());
    }

    [Fact]
    public async Task GetActiveTrips_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        var unauthenticatedClient = _factory.CreateClient();

        // Act
        var response = await unauthenticatedClient.GetAsync("/api/trips");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region GetTripById Tests

    [Fact]
    public async Task GetTripById_ShouldReturnOkWithTrip_WhenTripExists()
    {
        // Arrange
        var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SafeBodaDbContext>();
        var tripId = Guid.NewGuid();
        var trip = new TripEntity
        {
            Id = tripId,
            RiderId = Guid.NewGuid(),
            DriverId = Guid.NewGuid(),
            StartLatitude = 0.0,
            StartLongitude = 0.0,
            EndLatitude = 1.0,
            EndLongitude = 1.0,
            Fare = 1000m,
            RequestTime = DateTime.UtcNow
        };

        dbContext.Trips.Add(trip);
        await dbContext.SaveChangesAsync();
        scope.Dispose(); // Dispose scope but data should persist in in-memory DB

        // Act
        var response = await _client.GetAsync($"/api/trips/{tripId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var returnedTrip = JsonSerializer.Deserialize<Trip>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        Assert.NotNull(returnedTrip);
        Assert.Equal(tripId, returnedTrip.Id);
    }

    [Fact]
    public async Task GetTripById_ShouldReturnNotFound_WhenTripDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/trips/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("not found", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetTripById_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        var unauthenticatedClient = _factory.CreateClient();
        var tripId = Guid.NewGuid();

        // Act
        var response = await unauthenticatedClient.GetAsync($"/api/trips/{tripId}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region CreateTrip Tests

    [Fact]
    public async Task CreateTrip_ShouldReturnCreated_WhenRequestIsValid()
    {
        // Arrange
        var request = new TripRequest(
            RiderId: Guid.NewGuid(),
            Start: new Location(0.0, 0.0),
            End: new Location(1.0, 1.0)
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/trips", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var trip = JsonSerializer.Deserialize<Trip>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        Assert.NotNull(trip);
        Assert.Equal(request.RiderId, trip.RiderId);
        Assert.Equal(request.Start.Latitude, trip.Start.Latitude);
        Assert.Equal(request.End.Latitude, trip.End.Latitude);
        Assert.True(trip.Fare > 0);
        
        // Verify location header
        var location = response.Headers.Location?.ToString();
        Assert.NotNull(location);
        Assert.Contains("/api/Trips/", location, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateTrip_ShouldCalculateFareCorrectly()
    {
        // Arrange
        var request = new TripRequest(
            RiderId: Guid.NewGuid(),
            Start: new Location(0.0, 0.0),
            End: new Location(1.0, 1.0)
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/trips", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var trip = JsonSerializer.Deserialize<Trip>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        Assert.NotNull(trip);
        // Distance = sqrt((1-0)^2 + (1-0)^2) = sqrt(2) ≈ 1.414
        // Expected fare ≈ 1000 + 1.414 * 5000 ≈ 8070
        Assert.True(trip.Fare > 7000m && trip.Fare < 9000m);
    }

    [Fact]
    public async Task CreateTrip_ShouldPersistTripToDatabase()
    {
        // Arrange
        var request = new TripRequest(
            RiderId: Guid.NewGuid(),
            Start: new Location(0.0, 0.0),
            End: new Location(1.0, 1.0)
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/trips", request);
        var content = await response.Content.ReadAsStringAsync();
        var createdTrip = JsonSerializer.Deserialize<Trip>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(createdTrip);

        // Verify trip exists in database - use a new scope to ensure we get fresh data
        var scope = _factory.Services.CreateScope();
        try
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<SafeBodaDbContext>();
            var tripInDb = await dbContext.Trips.FindAsync(createdTrip.Id);
            Assert.NotNull(tripInDb);
            Assert.Equal(createdTrip.RiderId, tripInDb.RiderId);
            Assert.Equal(createdTrip.DriverId, tripInDb.DriverId);
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task CreateTrip_ShouldReturnBadRequest_WhenRequestIsInvalid()
    {
        // Arrange - send invalid JSON
        var invalidJson = "{ invalid json }";

        // Act
        var response = await _client.PostAsync("/api/trips", 
            new StringContent(invalidJson, Encoding.UTF8, "application/json"));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateTrip_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        var unauthenticatedClient = _factory.CreateClient();
        var request = new TripRequest(
            RiderId: Guid.NewGuid(),
            Start: new Location(0.0, 0.0),
            End: new Location(1.0, 1.0)
        );

        // Act
        var response = await unauthenticatedClient.PostAsJsonAsync("/api/trips", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region UpdateTrip Tests

    [Fact]
    public async Task UpdateTrip_ShouldReturnOkWithUpdatedTrip_WhenTripExists()
    {
        // Arrange
        var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SafeBodaDbContext>();
        var tripId = Guid.NewGuid();
        var existingTrip = new TripEntity
        {
            Id = tripId,
            RiderId = Guid.NewGuid(),
            DriverId = Guid.NewGuid(),
            StartLatitude = 0.0,
            StartLongitude = 0.0,
            EndLatitude = 1.0,
            EndLongitude = 1.0,
            Fare = 1000m,
            RequestTime = DateTime.UtcNow
        };

        dbContext.Trips.Add(existingTrip);
        await dbContext.SaveChangesAsync();
        scope.Dispose(); // Dispose scope but data should persist in in-memory DB

        var updateRequest = new TripUpdateRequest(
            Start: new Location(2.0, 2.0),
            End: new Location(3.0, 3.0)
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/api/trips/{tripId}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var updatedTrip = JsonSerializer.Deserialize<Trip>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        Assert.NotNull(updatedTrip);
        Assert.Equal(2.0, updatedTrip.Start.Latitude);
        Assert.Equal(3.0, updatedTrip.End.Latitude);
        
        // Verify trip was updated in database - use a new scope
        var verifyScope = _factory.Services.CreateScope();
        try
        {
            var verifyContext = verifyScope.ServiceProvider.GetRequiredService<SafeBodaDbContext>();
            var tripInDb = await verifyContext.Trips.FindAsync(tripId);
            Assert.NotNull(tripInDb);
            Assert.Equal(2.0, tripInDb.StartLatitude);
            Assert.Equal(3.0, tripInDb.EndLatitude);
        }
        finally
        {
            verifyScope.Dispose();
        }
    }

    [Fact]
    public async Task UpdateTrip_ShouldReturnNotFound_WhenTripDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var updateRequest = new TripUpdateRequest(
            Start: new Location(0.0, 0.0),
            End: new Location(1.0, 1.0)
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/api/trips/{nonExistentId}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("not found", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateTrip_ShouldRecalculateFare()
    {
        // Arrange
        var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SafeBodaDbContext>();
        var tripId = Guid.NewGuid();
        var existingTrip = new TripEntity
        {
            Id = tripId,
            RiderId = Guid.NewGuid(),
            DriverId = Guid.NewGuid(),
            StartLatitude = 0.0,
            StartLongitude = 0.0,
            EndLatitude = 1.0,
            EndLongitude = 1.0,
            Fare = 1000m,
            RequestTime = DateTime.UtcNow
        };

        dbContext.Trips.Add(existingTrip);
        await dbContext.SaveChangesAsync();
        scope.Dispose(); // Dispose scope but data should persist in in-memory DB

        var updateRequest = new TripUpdateRequest(
            Start: new Location(0.0, 0.0),
            End: new Location(2.0, 2.0) // Longer distance
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/api/trips/{tripId}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var updatedTrip = JsonSerializer.Deserialize<Trip>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        Assert.NotNull(updatedTrip);
        // New distance is sqrt((2-0)^2 + (2-0)^2) = sqrt(8) ≈ 2.828
        // Expected fare ≈ 1000 + 2.828 * 5000 ≈ 15140
        Assert.True(updatedTrip.Fare > 14000m && updatedTrip.Fare < 16000m);
    }

    [Fact]
    public async Task UpdateTrip_ShouldPreserveExistingTripProperties()
    {
        // Arrange
        var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SafeBodaDbContext>();
        var tripId = Guid.NewGuid();
        var riderId = Guid.NewGuid();
        var driverId = Guid.NewGuid();
        var requestTime = DateTime.UtcNow.AddHours(-1);

        var existingTrip = new TripEntity
        {
            Id = tripId,
            RiderId = riderId,
            DriverId = driverId,
            StartLatitude = 0.0,
            StartLongitude = 0.0,
            EndLatitude = 1.0,
            EndLongitude = 1.0,
            Fare = 1000m,
            RequestTime = requestTime
        };

        dbContext.Trips.Add(existingTrip);
        await dbContext.SaveChangesAsync();
        scope.Dispose(); // Dispose scope but data should persist in in-memory DB

        var updateRequest = new TripUpdateRequest(
            Start: new Location(2.0, 2.0),
            End: new Location(3.0, 3.0)
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/api/trips/{tripId}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        // Verify properties were preserved in database - use a new scope
        var verifyScope = _factory.Services.CreateScope();
        try
        {
            var verifyContext = verifyScope.ServiceProvider.GetRequiredService<SafeBodaDbContext>();
            var tripInDb = await verifyContext.Trips.FindAsync(tripId);
            Assert.NotNull(tripInDb);
            Assert.Equal(tripId, tripInDb.Id);
            Assert.Equal(riderId, tripInDb.RiderId);
            Assert.Equal(driverId, tripInDb.DriverId);
            Assert.Equal(requestTime, tripInDb.RequestTime);
        }
        finally
        {
            verifyScope.Dispose();
        }
    }

    [Fact]
    public async Task UpdateTrip_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        var unauthenticatedClient = _factory.CreateClient();
        var tripId = Guid.NewGuid();
        var updateRequest = new TripUpdateRequest(
            Start: new Location(0.0, 0.0),
            End: new Location(1.0, 1.0)
        );

        // Act
        var response = await unauthenticatedClient.PutAsJsonAsync($"/api/trips/{tripId}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region DeleteTrip Tests

    [Fact]
    public async Task DeleteTrip_ShouldReturnNoContent_WhenTripExists()
    {
        // Arrange
        var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SafeBodaDbContext>();
        var tripId = Guid.NewGuid();
        var trip = new TripEntity
        {
            Id = tripId,
            RiderId = Guid.NewGuid(),
            DriverId = Guid.NewGuid(),
            StartLatitude = 0.0,
            StartLongitude = 0.0,
            EndLatitude = 1.0,
            EndLongitude = 1.0,
            Fare = 1000m,
            RequestTime = DateTime.UtcNow
        };

        dbContext.Trips.Add(trip);
        await dbContext.SaveChangesAsync();
        scope.Dispose(); // Dispose scope but data should persist in in-memory DB

        // Act
        var response = await _client.DeleteAsync($"/api/trips/{tripId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        
        // Verify trip was deleted from database - use a new scope
        var verifyScope = _factory.Services.CreateScope();
        try
        {
            var verifyContext = verifyScope.ServiceProvider.GetRequiredService<SafeBodaDbContext>();
            var tripInDb = await verifyContext.Trips.FindAsync(tripId);
            Assert.Null(tripInDb);
        }
        finally
        {
            verifyScope.Dispose();
        }
    }

    [Fact]
    public async Task DeleteTrip_ShouldReturnNotFound_WhenTripDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/trips/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("not found", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DeleteTrip_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        var unauthenticatedClient = _factory.CreateClient();
        var tripId = Guid.NewGuid();

        // Act
        var response = await unauthenticatedClient.DeleteAsync($"/api/trips/{tripId}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion
}

