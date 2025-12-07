using SafeBoda.Core;

namespace SafeBoda.Application;

public class InMemoryTripRepository : ITripRepository
{
    private readonly List<Trip> _trips = new();
    private readonly List<Rider> _riders = new();
    private readonly List<Driver> _drivers = new();

    public InMemoryTripRepository()
    {
        _trips.AddRange(new[]
        {
            new Trip(
                Id: Guid.NewGuid(),
                RiderId: Guid.NewGuid(),
                DriverId: Guid.NewGuid(),
                Start: new Location(-1.9441, 30.0619),
                End: new Location(-1.9706, 30.1044),
                Fare: 3000m,
                RequestTime: DateTime.Now.AddMinutes(-15)
            ),
            new Trip(
                Id: Guid.NewGuid(),
                RiderId: Guid.NewGuid(),
                DriverId: Guid.NewGuid(),
                Start: new Location(-1.9536, 30.0605),
                End: new Location(-1.9403, 30.0856),
                Fare: 2500m,
                RequestTime: DateTime.Now.AddMinutes(-10)
            )
        });
    }

    public Task<IEnumerable<Trip>> GetActiveTripsAsync() => Task.FromResult<IEnumerable<Trip>>(_trips);

    public Task<Trip?> GetTripByIdAsync(Guid id) => Task.FromResult<Trip?>(_trips.FirstOrDefault(t => t.Id == id));

    public Task<Trip> CreateTripAsync(Trip trip)
    {
        _trips.Add(trip);
        return Task.FromResult(trip);
    }

    public Task<Trip> UpdateTripAsync(Trip trip)
    {
        var index = _trips.FindIndex(t => t.Id == trip.Id);
        if (index >= 0)
        {
            _trips[index] = trip;
        }
        return Task.FromResult(trip);
    }

    public Task<bool> DeleteTripAsync(Guid id)
    {
        var trip = _trips.FirstOrDefault(t => t.Id == id);
        if (trip == null) return Task.FromResult(false);
        _trips.Remove(trip);
        return Task.FromResult(true);
    }

    public Task<Rider?> GetRiderByIdAsync(Guid id) => Task.FromResult<Rider?>(_riders.FirstOrDefault(r => r.Id == id));

    public Task<Rider> CreateRiderAsync(Rider rider)
    {
        _riders.Add(rider);
        return Task.FromResult(rider);
    }

    public Task<Rider> UpdateRiderAsync(Rider rider)
    {
        var index = _riders.FindIndex(r => r.Id == rider.Id);
        if (index >= 0)
        {
            _riders[index] = rider;
        }
        return Task.FromResult(rider);
    }

    public Task<bool> DeleteRiderAsync(Guid id)
    {
        var rider = _riders.FirstOrDefault(r => r.Id == id);
        if (rider == null) return Task.FromResult(false);
        _riders.Remove(rider);
        return Task.FromResult(true);
    }

    public Task<Driver?> GetDriverByIdAsync(Guid id) => Task.FromResult<Driver?>(_drivers.FirstOrDefault(d => d.Id == id));

    public Task<Driver> CreateDriverAsync(Driver driver)
    {
        _drivers.Add(driver);
        return Task.FromResult(driver);
    }

    public Task<Driver> UpdateDriverAsync(Driver driver)
    {
        var index = _drivers.FindIndex(d => d.Id == driver.Id);
        if (index >= 0)
        {
            _drivers[index] = driver;
        }
        return Task.FromResult(driver);
    }

    public Task<bool> DeleteDriverAsync(Guid id)
    {
        var driver = _drivers.FirstOrDefault(d => d.Id == id);
        if (driver == null) return Task.FromResult(false);
        _drivers.Remove(driver);
        return Task.FromResult(true);
    }
}

