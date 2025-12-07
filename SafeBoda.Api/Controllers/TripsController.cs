using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using SafeBoda.Api.Models;
using SafeBoda.Application;
using SafeBoda.Core;
using System.Security.Claims;

namespace SafeBoda.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TripsController : ControllerBase
{
    private readonly ITripRepository _tripRepository;
    private readonly IMemoryCache _cache;
    private const string CacheKey = "ActiveTrips";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(1);

    public TripsController(ITripRepository tripRepository, IMemoryCache cache)
    {
        _tripRepository = tripRepository;
        _cache = cache;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Trip>>> GetActiveTripsAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userEmail = User.FindFirstValue(ClaimTypes.Email);
        
        // Try to get trips from cache
        if (!_cache.TryGetValue(CacheKey, out IEnumerable<Trip>? trips))
        {
            // Cache miss - fetch from database
            trips = await _tripRepository.GetActiveTripsAsync();
            
            // Store in cache with expiration
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheExpiration,
                SlidingExpiration = null // Use absolute expiration only
            };
            
            _cache.Set(CacheKey, trips, cacheOptions);
        }
        
        return Ok(new 
        { 
            authenticatedUser = new { userId, userEmail },
            trips 
        });
    }


    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Trip>> GetTripByIdAsync(Guid id)
    {
        var trip = await _tripRepository.GetTripByIdAsync(id);
        
        if (trip == null)
        {
            return NotFound(new { message = $"Trip with ID {id} not found" });
        }
        
        return Ok(trip);
    }


    [HttpPost]
    public async Task<ActionResult<Trip>> CreateTripAsync([FromBody] TripRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        var newTrip = new Trip(
            Id: Guid.NewGuid(),
            RiderId: request.RiderId,
            DriverId: Guid.NewGuid(),
            Start: request.Start,
            End: request.End,
            Fare: CalculateFare(request.Start, request.End),
            RequestTime: DateTime.UtcNow
        );


        var createdTrip = await _tripRepository.CreateTripAsync(newTrip);
        
        // Invalidate cache when a new trip is created
        _cache.Remove(CacheKey);
        
        // ASP.NET Core strips "Async" suffix from action names for routing
        return CreatedAtAction("GetTripById", new { id = createdTrip.Id }, createdTrip);
    }


    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Trip>> UpdateTripAsync(Guid id, [FromBody] TripUpdateRequest request)
    {
        var existingTrip = await _tripRepository.GetTripByIdAsync(id);
        
        if (existingTrip == null)
        {
            return NotFound(new { message = $"Trip with ID {id} not found" });
        }

        var updatedTrip = new Trip(
            Id: existingTrip.Id,
            RiderId: existingTrip.RiderId,
            DriverId: existingTrip.DriverId,
            Start: request.Start,
            End: request.End,
            Fare: CalculateFare(request.Start, request.End),
            RequestTime: existingTrip.RequestTime
        );

        var result = await _tripRepository.UpdateTripAsync(updatedTrip);
        
        // Invalidate cache when a trip is updated
        _cache.Remove(CacheKey);
        
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteTripAsync(Guid id)
    {
        var deleted = await _tripRepository.DeleteTripAsync(id);
        
        if (!deleted)
        {
            return NotFound(new { message = $"Trip with ID {id} not found" });
        }
        
        // Invalidate cache when a trip is deleted
        _cache.Remove(CacheKey);
        
        return NoContent(); 
    }


    private static decimal CalculateFare(Location start, Location end)
    {
        var distance = Math.Sqrt(
            Math.Pow(end.Latitude - start.Latitude, 2) +
            Math.Pow(end.Longitude - start.Longitude, 2)
        );

        var baseFare = 1000m; 
        var farePerUnit = 5000m;

        return baseFare + (decimal)(distance * (double)farePerUnit);
    }
}

