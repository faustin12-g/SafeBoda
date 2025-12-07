using SafeBoda.Core;

namespace SafeBoda.Api.Models;

public record TripUpdateRequest(Location Start, Location End);
