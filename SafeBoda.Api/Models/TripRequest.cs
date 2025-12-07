using SafeBoda.Core;

namespace SafeBoda.Api.Models;

public record TripRequest(Guid RiderId, Location Start, Location End);

