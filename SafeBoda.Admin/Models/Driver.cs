namespace SafeBoda.Admin.Models;

public class Driver
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string MotoPlateNumber { get; set; } = string.Empty;
}
