namespace SafeBoda.Infrastructure.Entities;

public class TripEntity
{
    public Guid Id { get; set; }
    public Guid RiderId { get; set; }
    public Guid DriverId { get; set; }
    public double StartLatitude { get; set; }
    public double StartLongitude { get; set; }
    public double EndLatitude { get; set; }
    public double EndLongitude { get; set; }
    public decimal Fare { get; set; }
    public DateTime RequestTime { get; set; }
}

public class RiderEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}

public class DriverEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string MotoPlateNumber { get; set; } = string.Empty;
}

