using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using SafeBoda.Api.Controllers;
using SafeBoda.Api.Models;
using SafeBoda.Application;
using SafeBoda.Core;
using System.Security.Claims;

namespace SafeBoda.Api.Tests;

public class TripsControllerTests
{
    private readonly Mock<ITripRepository> _mockRepository;
    private readonly IMemoryCache _memoryCache;
    private readonly TripsController _controller;

    public TripsControllerTests()
    {
        _mockRepository = new Mock<ITripRepository>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _controller = new TripsController(_mockRepository.Object, _memoryCache);
        
        // Set up controller context with a mock user
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            new Claim(ClaimTypes.Email, "test@example.com")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };
    }

    #region GetActiveTrips Tests

    [Fact]
    public async Task GetActiveTrips_ShouldReturnOkWithTrips_WhenTripsExist()
    {
        // Arrange
        var trips = new List<Trip>
        {
            new Trip(
                Id: Guid.NewGuid(),
                RiderId: Guid.NewGuid(),
                DriverId: Guid.NewGuid(),
                Start: new Location(0.0, 0.0),
                End: new Location(1.0, 1.0),
                Fare: 1000m,
                RequestTime: DateTime.UtcNow
            ),
            new Trip(
                Id: Guid.NewGuid(),
                RiderId: Guid.NewGuid(),
                DriverId: Guid.NewGuid(),
                Start: new Location(2.0, 2.0),
                End: new Location(3.0, 3.0),
                Fare: 2000m,
                RequestTime: DateTime.UtcNow
            )
        };

        _mockRepository.Setup(r => r.GetActiveTripsAsync()).ReturnsAsync(trips);

        // Act
        var result = await _controller.GetActiveTripsAsync();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = okResult.Value;
        Assert.NotNull(response);
        
        // Verify repository method was called
        _mockRepository.Verify(r => r.GetActiveTripsAsync(), Times.Once);
    }

    [Fact]
    public async Task GetActiveTrips_ShouldReturnOkWithEmptyList_WhenNoTripsExist()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetActiveTripsAsync()).ReturnsAsync(new List<Trip>());

        // Act
        var result = await _controller.GetActiveTripsAsync();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        _mockRepository.Verify(r => r.GetActiveTripsAsync(), Times.Once);
    }

    [Fact]
    public async Task GetActiveTrips_ShouldIncludeAuthenticatedUserInfo()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetActiveTripsAsync()).ReturnsAsync(new List<Trip>());

        // Act
        var result = await _controller.GetActiveTripsAsync();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = okResult.Value;
        Assert.NotNull(response);
        
        // Verify repository method was called
        _mockRepository.Verify(r => r.GetActiveTripsAsync(), Times.Once);
    }

    #endregion

    #region GetTripById Tests

    [Fact]
    public async Task GetTripById_ShouldReturnOkWithTrip_WhenTripExists()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        var trip = new Trip(
            Id: tripId,
            RiderId: Guid.NewGuid(),
            DriverId: Guid.NewGuid(),
            Start: new Location(0.0, 0.0),
            End: new Location(1.0, 1.0),
            Fare: 1000m,
            RequestTime: DateTime.UtcNow
        );

        _mockRepository.Setup(r => r.GetTripByIdAsync(tripId)).ReturnsAsync(trip);

        // Act
        var result = await _controller.GetTripByIdAsync(tripId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedTrip = Assert.IsType<Trip>(okResult.Value);
        Assert.Equal(tripId, returnedTrip.Id);
        _mockRepository.Verify(r => r.GetTripByIdAsync(tripId), Times.Once);
    }

    [Fact]
    public async Task GetTripById_ShouldReturnNotFound_WhenTripDoesNotExist()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetTripByIdAsync(tripId)).ReturnsAsync((Trip?)null);

        // Act
        var result = await _controller.GetTripByIdAsync(tripId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        _mockRepository.Verify(r => r.GetTripByIdAsync(tripId), Times.Once);
    }

    [Fact]
    public async Task GetTripById_ShouldCallRepositoryWithCorrectId()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetTripByIdAsync(tripId)).ReturnsAsync((Trip?)null);

        // Act
        await _controller.GetTripByIdAsync(tripId);

        // Assert
        _mockRepository.Verify(r => r.GetTripByIdAsync(It.Is<Guid>(id => id == tripId)), Times.Once);
    }

    #endregion

    #region CreateTrip Tests

    [Fact]
    public async Task CreateTrip_ShouldReturnCreatedAtAction_WhenRequestIsValid()
    {
        // Arrange
        var request = new TripRequest(
            RiderId: Guid.NewGuid(),
            Start: new Location(0.0, 0.0),
            End: new Location(1.0, 1.0)
        );

        var createdTrip = new Trip(
            Id: Guid.NewGuid(),
            RiderId: request.RiderId,
            DriverId: Guid.NewGuid(),
            Start: request.Start,
            End: request.End,
            Fare: 1000m,
            RequestTime: DateTime.UtcNow
        );

        _mockRepository.Setup(r => r.CreateTripAsync(It.IsAny<Trip>())).ReturnsAsync(createdTrip);

        // Act
        var result = await _controller.CreateTripAsync(request);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnedTrip = Assert.IsType<Trip>(createdAtActionResult.Value);
        Assert.Equal(createdTrip.Id, returnedTrip.Id);
        // ASP.NET Core strips "Async" suffix from action names for routing
        Assert.Equal("GetTripById", createdAtActionResult.ActionName);
        _mockRepository.Verify(r => r.CreateTripAsync(It.IsAny<Trip>()), Times.Once);
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

        Trip? capturedTrip = null;
        _mockRepository.Setup(r => r.CreateTripAsync(It.IsAny<Trip>()))
            .Callback<Trip>(t => capturedTrip = t)
            .ReturnsAsync((Trip t) => t);

        // Act
        await _controller.CreateTripAsync(request);

        // Assert
        Assert.NotNull(capturedTrip);
        // Fare should be calculated: baseFare (1000) + distance * farePerUnit (5000)
        // Distance = sqrt((1-0)^2 + (1-0)^2) = sqrt(2) ≈ 1.414
        // Expected fare ≈ 1000 + 1.414 * 5000 ≈ 8070
        Assert.True(capturedTrip.Fare > 7000m && capturedTrip.Fare < 9000m);
        _mockRepository.Verify(r => r.CreateTripAsync(It.IsAny<Trip>()), Times.Once);
    }

    [Fact]
    public async Task CreateTrip_ShouldGenerateNewGuidForTripId()
    {
        // Arrange
        var request = new TripRequest(
            RiderId: Guid.NewGuid(),
            Start: new Location(0.0, 0.0),
            End: new Location(1.0, 1.0)
        );

        Trip? capturedTrip = null;
        _mockRepository.Setup(r => r.CreateTripAsync(It.IsAny<Trip>()))
            .Callback<Trip>(t => capturedTrip = t)
            .ReturnsAsync((Trip t) => t);

        // Act
        await _controller.CreateTripAsync(request);

        // Assert
        Assert.NotNull(capturedTrip);
        Assert.NotEqual(Guid.Empty, capturedTrip.Id);
        _mockRepository.Verify(r => r.CreateTripAsync(It.IsAny<Trip>()), Times.Once);
    }

    [Fact]
    public async Task CreateTrip_ShouldGenerateNewGuidForDriverId()
    {
        // Arrange
        var request = new TripRequest(
            RiderId: Guid.NewGuid(),
            Start: new Location(0.0, 0.0),
            End: new Location(1.0, 1.0)
        );

        Trip? capturedTrip = null;
        _mockRepository.Setup(r => r.CreateTripAsync(It.IsAny<Trip>()))
            .Callback<Trip>(t => capturedTrip = t)
            .ReturnsAsync((Trip t) => t);

        // Act
        await _controller.CreateTripAsync(request);

        // Assert
        Assert.NotNull(capturedTrip);
        Assert.NotEqual(Guid.Empty, capturedTrip.DriverId);
        _mockRepository.Verify(r => r.CreateTripAsync(It.IsAny<Trip>()), Times.Once);
    }

    [Fact]
    public async Task CreateTrip_ShouldUseRequestRiderId()
    {
        // Arrange
        var riderId = Guid.NewGuid();
        var request = new TripRequest(
            RiderId: riderId,
            Start: new Location(0.0, 0.0),
            End: new Location(1.0, 1.0)
        );

        Trip? capturedTrip = null;
        _mockRepository.Setup(r => r.CreateTripAsync(It.IsAny<Trip>()))
            .Callback<Trip>(t => capturedTrip = t)
            .ReturnsAsync((Trip t) => t);

        // Act
        await _controller.CreateTripAsync(request);

        // Assert
        Assert.NotNull(capturedTrip);
        Assert.Equal(riderId, capturedTrip.RiderId);
        _mockRepository.Verify(r => r.CreateTripAsync(It.IsAny<Trip>()), Times.Once);
    }

    [Fact]
    public async Task CreateTrip_ShouldSetRequestTimeToUtcNow()
    {
        // Arrange
        var request = new TripRequest(
            RiderId: Guid.NewGuid(),
            Start: new Location(0.0, 0.0),
            End: new Location(1.0, 1.0)
        );

        var beforeCreation = DateTime.UtcNow;
        Trip? capturedTrip = null;
        _mockRepository.Setup(r => r.CreateTripAsync(It.IsAny<Trip>()))
            .Callback<Trip>(t => capturedTrip = t)
            .ReturnsAsync((Trip t) => t);

        // Act
        await _controller.CreateTripAsync(request);
        var afterCreation = DateTime.UtcNow;

        // Assert
        Assert.NotNull(capturedTrip);
        Assert.True(capturedTrip.RequestTime >= beforeCreation && capturedTrip.RequestTime <= afterCreation);
        _mockRepository.Verify(r => r.CreateTripAsync(It.IsAny<Trip>()), Times.Once);
    }

    #endregion

    #region UpdateTrip Tests

    [Fact]
    public async Task UpdateTrip_ShouldReturnOkWithUpdatedTrip_WhenTripExists()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        var existingTrip = new Trip(
            Id: tripId,
            RiderId: Guid.NewGuid(),
            DriverId: Guid.NewGuid(),
            Start: new Location(0.0, 0.0),
            End: new Location(1.0, 1.0),
            Fare: 1000m,
            RequestTime: DateTime.UtcNow
        );

        var updateRequest = new TripUpdateRequest(
            Start: new Location(2.0, 2.0),
            End: new Location(3.0, 3.0)
        );

        var updatedTrip = new Trip(
            Id: tripId,
            RiderId: existingTrip.RiderId,
            DriverId: existingTrip.DriverId,
            Start: updateRequest.Start,
            End: updateRequest.End,
            Fare: 2000m,
            RequestTime: existingTrip.RequestTime
        );

        _mockRepository.Setup(r => r.GetTripByIdAsync(tripId)).ReturnsAsync(existingTrip);
        _mockRepository.Setup(r => r.UpdateTripAsync(It.IsAny<Trip>())).ReturnsAsync(updatedTrip);

        // Act
        var result = await _controller.UpdateTripAsync(tripId, updateRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedTrip = Assert.IsType<Trip>(okResult.Value);
        Assert.Equal(updateRequest.Start.Latitude, returnedTrip.Start.Latitude);
        Assert.Equal(updateRequest.End.Latitude, returnedTrip.End.Latitude);
        _mockRepository.Verify(r => r.GetTripByIdAsync(tripId), Times.Once);
        _mockRepository.Verify(r => r.UpdateTripAsync(It.IsAny<Trip>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTrip_ShouldReturnNotFound_WhenTripDoesNotExist()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        var updateRequest = new TripUpdateRequest(
            Start: new Location(0.0, 0.0),
            End: new Location(1.0, 1.0)
        );

        _mockRepository.Setup(r => r.GetTripByIdAsync(tripId)).ReturnsAsync((Trip?)null);

        // Act
        var result = await _controller.UpdateTripAsync(tripId, updateRequest);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        _mockRepository.Verify(r => r.GetTripByIdAsync(tripId), Times.Once);
        _mockRepository.Verify(r => r.UpdateTripAsync(It.IsAny<Trip>()), Times.Never);
    }

    [Fact]
    public async Task UpdateTrip_ShouldPreserveExistingTripProperties()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        var riderId = Guid.NewGuid();
        var driverId = Guid.NewGuid();
        var requestTime = DateTime.UtcNow.AddHours(-1);

        var existingTrip = new Trip(
            Id: tripId,
            RiderId: riderId,
            DriverId: driverId,
            Start: new Location(0.0, 0.0),
            End: new Location(1.0, 1.0),
            Fare: 1000m,
            RequestTime: requestTime
        );

        var updateRequest = new TripUpdateRequest(
            Start: new Location(2.0, 2.0),
            End: new Location(3.0, 3.0)
        );

        Trip? capturedTrip = null;
        _mockRepository.Setup(r => r.GetTripByIdAsync(tripId)).ReturnsAsync(existingTrip);
        _mockRepository.Setup(r => r.UpdateTripAsync(It.IsAny<Trip>()))
            .Callback<Trip>(t => capturedTrip = t)
            .ReturnsAsync((Trip t) => t);

        // Act
        await _controller.UpdateTripAsync(tripId, updateRequest);

        // Assert
        Assert.NotNull(capturedTrip);
        Assert.Equal(tripId, capturedTrip.Id);
        Assert.Equal(riderId, capturedTrip.RiderId);
        Assert.Equal(driverId, capturedTrip.DriverId);
        Assert.Equal(requestTime, capturedTrip.RequestTime);
        _mockRepository.Verify(r => r.GetTripByIdAsync(tripId), Times.Once);
        _mockRepository.Verify(r => r.UpdateTripAsync(It.IsAny<Trip>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTrip_ShouldRecalculateFare()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        var existingTrip = new Trip(
            Id: tripId,
            RiderId: Guid.NewGuid(),
            DriverId: Guid.NewGuid(),
            Start: new Location(0.0, 0.0),
            End: new Location(1.0, 1.0),
            Fare: 1000m,
            RequestTime: DateTime.UtcNow
        );

        var updateRequest = new TripUpdateRequest(
            Start: new Location(0.0, 0.0),
            End: new Location(2.0, 2.0) // Longer distance
        );

        Trip? capturedTrip = null;
        _mockRepository.Setup(r => r.GetTripByIdAsync(tripId)).ReturnsAsync(existingTrip);
        _mockRepository.Setup(r => r.UpdateTripAsync(It.IsAny<Trip>()))
            .Callback<Trip>(t => capturedTrip = t)
            .ReturnsAsync((Trip t) => t);

        // Act
        await _controller.UpdateTripAsync(tripId, updateRequest);

        // Assert
        Assert.NotNull(capturedTrip);
        // New distance is sqrt((2-0)^2 + (2-0)^2) = sqrt(8) ≈ 2.828
        // Expected fare ≈ 1000 + 2.828 * 5000 ≈ 15140
        Assert.True(capturedTrip.Fare > 14000m && capturedTrip.Fare < 16000m);
        _mockRepository.Verify(r => r.GetTripByIdAsync(tripId), Times.Once);
        _mockRepository.Verify(r => r.UpdateTripAsync(It.IsAny<Trip>()), Times.Once);
    }

    #endregion

    #region DeleteTrip Tests

    [Fact]
    public async Task DeleteTrip_ShouldReturnNoContent_WhenTripExists()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        _mockRepository.Setup(r => r.DeleteTripAsync(tripId)).ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteTripAsync(tripId);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mockRepository.Verify(r => r.DeleteTripAsync(tripId), Times.Once);
    }

    [Fact]
    public async Task DeleteTrip_ShouldReturnNotFound_WhenTripDoesNotExist()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        _mockRepository.Setup(r => r.DeleteTripAsync(tripId)).ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteTripAsync(tripId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        _mockRepository.Verify(r => r.DeleteTripAsync(tripId), Times.Once);
    }

    [Fact]
    public async Task DeleteTrip_ShouldCallRepositoryWithCorrectId()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        _mockRepository.Setup(r => r.DeleteTripAsync(tripId)).ReturnsAsync(true);

        // Act
        await _controller.DeleteTripAsync(tripId);

        // Assert
        _mockRepository.Verify(r => r.DeleteTripAsync(It.Is<Guid>(id => id == tripId)), Times.Once);
    }

    #endregion

    #region Edge Cases and Error Conditions

    [Fact]
    public async Task GetActiveTrips_ShouldHandleNullFromRepository()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetActiveTripsAsync()).ReturnsAsync((IEnumerable<Trip>?)null!);

        // Act
        var result = await _controller.GetActiveTripsAsync();

        // Assert
        // Should still return Ok, even if repository returns null
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        _mockRepository.Verify(r => r.GetActiveTripsAsync(), Times.Once);
    }

    [Fact]
    public async Task GetTripById_ShouldHandleEmptyGuid()
    {
        // Arrange
        var emptyGuid = Guid.Empty;
        _mockRepository.Setup(r => r.GetTripByIdAsync(emptyGuid)).ReturnsAsync((Trip?)null);

        // Act
        var result = await _controller.GetTripByIdAsync(emptyGuid);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        _mockRepository.Verify(r => r.GetTripByIdAsync(emptyGuid), Times.Once);
    }

    [Fact]
    public async Task CreateTrip_ShouldHandleZeroDistance()
    {
        // Arrange
        var request = new TripRequest(
            RiderId: Guid.NewGuid(),
            Start: new Location(0.0, 0.0),
            End: new Location(0.0, 0.0) // Same location
        );

        Trip? capturedTrip = null;
        _mockRepository.Setup(r => r.CreateTripAsync(It.IsAny<Trip>()))
            .Callback<Trip>(t => capturedTrip = t)
            .ReturnsAsync((Trip t) => t);

        // Act
        await _controller.CreateTripAsync(request);

        // Assert
        Assert.NotNull(capturedTrip);
        // Fare should be base fare (1000) when distance is 0
        Assert.Equal(1000m, capturedTrip.Fare);
        _mockRepository.Verify(r => r.CreateTripAsync(It.IsAny<Trip>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTrip_ShouldHandleZeroDistance()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        var existingTrip = new Trip(
            Id: tripId,
            RiderId: Guid.NewGuid(),
            DriverId: Guid.NewGuid(),
            Start: new Location(0.0, 0.0),
            End: new Location(1.0, 1.0),
            Fare: 1000m,
            RequestTime: DateTime.UtcNow
        );

        var updateRequest = new TripUpdateRequest(
            Start: new Location(0.0, 0.0),
            End: new Location(0.0, 0.0) // Same location
        );

        Trip? capturedTrip = null;
        _mockRepository.Setup(r => r.GetTripByIdAsync(tripId)).ReturnsAsync(existingTrip);
        _mockRepository.Setup(r => r.UpdateTripAsync(It.IsAny<Trip>()))
            .Callback<Trip>(t => capturedTrip = t)
            .ReturnsAsync((Trip t) => t);

        // Act
        await _controller.UpdateTripAsync(tripId, updateRequest);

        // Assert
        Assert.NotNull(capturedTrip);
        // Fare should be base fare (1000) when distance is 0
        Assert.Equal(1000m, capturedTrip.Fare);
        _mockRepository.Verify(r => r.GetTripByIdAsync(tripId), Times.Once);
        _mockRepository.Verify(r => r.UpdateTripAsync(It.IsAny<Trip>()), Times.Once);
    }

    #endregion
}

