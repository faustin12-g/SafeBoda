using Microsoft.EntityFrameworkCore;
using SafeBoda.Application;
using SafeBoda.Core;
using SafeBoda.Infrastructure.Data;
using SafeBoda.Infrastructure.Entities;

namespace SafeBoda.Infrastructure.Repositories;

public class EfTripRepository : ITripRepository
{
    private readonly SafeBodaDbContext _context;

    public EfTripRepository(SafeBodaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Trip>> GetActiveTripsAsync()
    {
        var entities = await _context.Trips.ToListAsync();
        return entities.Select(MapToTripDomain);
    }

    public async Task<Trip?> GetTripByIdAsync(Guid id)
    {
        var entity = await _context.Trips.FirstOrDefaultAsync(t => t.Id == id);
        return entity == null ? null : MapToTripDomain(entity);
    }

    public async Task<Trip> CreateTripAsync(Trip trip)
    {
        var entity = MapToTripEntity(trip);
        _context.Trips.Add(entity);
        await _context.SaveChangesAsync();
        return trip;
    }

    public async Task<Trip> UpdateTripAsync(Trip trip)
    {
        var existingEntity = await _context.Trips.FindAsync(trip.Id);
        if (existingEntity == null)
        {
            var entity = MapToTripEntity(trip);
            _context.Trips.Add(entity);
        }
        else
        {
            // Update existing entity properties
            existingEntity.RiderId = trip.RiderId;
            existingEntity.DriverId = trip.DriverId;
            existingEntity.StartLatitude = trip.Start.Latitude;
            existingEntity.StartLongitude = trip.Start.Longitude;
            existingEntity.EndLatitude = trip.End.Latitude;
            existingEntity.EndLongitude = trip.End.Longitude;
            existingEntity.Fare = trip.Fare;
            existingEntity.RequestTime = trip.RequestTime;
        }
        await _context.SaveChangesAsync();
        return trip;
    }

    private static Trip MapToTripDomain(TripEntity entity)
    {
        return new Trip(
            entity.Id,
            entity.RiderId,
            entity.DriverId,
            new Location(entity.StartLatitude, entity.StartLongitude),
            new Location(entity.EndLatitude, entity.EndLongitude),
            entity.Fare,
            entity.RequestTime
        );
    }

    private static TripEntity MapToTripEntity(Trip trip)
    {
        return new TripEntity
        {
            Id = trip.Id,
            RiderId = trip.RiderId,
            DriverId = trip.DriverId,
            StartLatitude = trip.Start.Latitude,
            StartLongitude = trip.Start.Longitude,
            EndLatitude = trip.End.Latitude,
            EndLongitude = trip.End.Longitude,
            Fare = trip.Fare,
            RequestTime = trip.RequestTime
        };
    }

    public async Task<bool> DeleteTripAsync(Guid id)
    {
        var trip = await _context.Trips.FirstOrDefaultAsync(t => t.Id == id);
        if (trip == null)
            return false;

        _context.Trips.Remove(trip);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Rider?> GetRiderByIdAsync(Guid id)
    {
        var entity = await _context.Riders.FirstOrDefaultAsync(r => r.Id == id);
        return entity == null ? null : MapToRiderDomain(entity);
    }

    public async Task<Driver?> GetDriverByIdAsync(Guid id)
    {
        var entity = await _context.Drivers.FirstOrDefaultAsync(d => d.Id == id);
        return entity == null ? null : MapToDriverDomain(entity);
    }

    public async Task<Rider> CreateRiderAsync(Rider rider)
    {
        var entity = MapToRiderEntity(rider);
        _context.Riders.Add(entity);
        await _context.SaveChangesAsync();
        return rider;
    }

    public async Task<Driver> CreateDriverAsync(Driver driver)
    {
        var entity = MapToDriverEntity(driver);
        _context.Drivers.Add(entity);
        await _context.SaveChangesAsync();
        return driver;
    }

    public async Task<Rider> UpdateRiderAsync(Rider rider)
    {
        var entity = MapToRiderEntity(rider);
        _context.Riders.Update(entity);
        await _context.SaveChangesAsync();
        return rider;
    }

    public async Task<bool> DeleteRiderAsync(Guid id)
    {
        var rider = await _context.Riders.FirstOrDefaultAsync(r => r.Id == id);
        if (rider == null)
            return false;

        _context.Riders.Remove(rider);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Driver> UpdateDriverAsync(Driver driver)
    {
        var entity = MapToDriverEntity(driver);
        _context.Drivers.Update(entity);
        await _context.SaveChangesAsync();
        return driver;
    }

    public async Task<bool> DeleteDriverAsync(Guid id)
    {
        var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.Id == id);
        if (driver == null)
            return false;

        _context.Drivers.Remove(driver);
        await _context.SaveChangesAsync();
        return true;
    }

    private static Rider MapToRiderDomain(RiderEntity entity)
    {
        return new Rider(entity.Id, entity.Name, entity.PhoneNumber);
    }

    private static RiderEntity MapToRiderEntity(Rider rider)
    {
        return new RiderEntity
        {
            Id = rider.Id,
            Name = rider.Name,
            PhoneNumber = rider.PhoneNumber
        };
    }

    private static Driver MapToDriverDomain(DriverEntity entity)
    {
        return new Driver(entity.Id, entity.Name, entity.PhoneNumber, entity.MotoPlateNumber);
    }

    private static DriverEntity MapToDriverEntity(Driver driver)
    {
        return new DriverEntity
        {
            Id = driver.Id,
            Name = driver.Name,
            PhoneNumber = driver.PhoneNumber,
            MotoPlateNumber = driver.MotoPlateNumber
        };
    }
}

