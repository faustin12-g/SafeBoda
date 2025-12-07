using SafeBoda.Core;

namespace SafeBoda.Application;

public interface ITripRepository
{
    Task<IEnumerable<Trip>> GetActiveTripsAsync();
    Task<Trip?> GetTripByIdAsync(Guid id);
    Task<Trip> CreateTripAsync(Trip trip);
    Task<Trip> UpdateTripAsync(Trip trip);
    Task<bool> DeleteTripAsync(Guid id);
    
    Task<Rider?> GetRiderByIdAsync(Guid id);
    Task<Rider> CreateRiderAsync(Rider rider);
    Task<Rider> UpdateRiderAsync(Rider rider);
    Task<bool> DeleteRiderAsync(Guid id);
    
    Task<Driver?> GetDriverByIdAsync(Guid id);
    Task<Driver> CreateDriverAsync(Driver driver);
    Task<Driver> UpdateDriverAsync(Driver driver);
    Task<bool> DeleteDriverAsync(Guid id);
}

