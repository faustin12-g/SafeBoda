namespace SafeBoda.Admin.Models;

public class Trip
{
    public Guid Id { get; set; }
    public Guid RiderId { get; set; }
    public Guid DriverId { get; set; }
    public Location Start { get; set; } = new();
    public Location End { get; set; } = new();
    public decimal Fare { get; set; }
    public DateTime RequestTime { get; set; }
}

public class Location
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class TripsResponse
{
    public AuthenticatedUser AuthenticatedUser { get; set; } = new();
    public List<Trip> Trips { get; set; } = new();
}

public class AuthenticatedUser
{
    public string UserId { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
}
