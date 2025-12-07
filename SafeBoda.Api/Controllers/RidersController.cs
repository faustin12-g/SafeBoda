using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeBoda.Application;
using SafeBoda.Core;

namespace SafeBoda.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class RidersController : ControllerBase
{
    private readonly ITripRepository _repository;

    public RidersController(ITripRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Rider>> GetRiderByIdAsync(Guid id)
    {
        var rider = await _repository.GetRiderByIdAsync(id);
        
        if (rider == null)
        {
            return NotFound(new { message = $"Rider with ID {id} not found" });
        }
        
        return Ok(rider);
    }

    [HttpPost]
    public async Task<ActionResult<Rider>> CreateRiderAsync([FromBody] Rider rider)
    {
        if (string.IsNullOrWhiteSpace(rider.Name) || string.IsNullOrWhiteSpace(rider.PhoneNumber))
        {
            return BadRequest(new { message = "Name and PhoneNumber are required" });
        }

        var newRider = new Rider(
            Id: Guid.NewGuid(),
            Name: rider.Name,
            PhoneNumber: rider.PhoneNumber
        );

        var createdRider = await _repository.CreateRiderAsync(newRider);
        
        // ASP.NET Core strips "Async" suffix from action names for routing
        return CreatedAtAction("GetRiderById", new { id = createdRider.Id }, createdRider);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Rider>> UpdateRiderAsync(Guid id, [FromBody] Rider rider)
    {
        var existingRider = await _repository.GetRiderByIdAsync(id);
        
        if (existingRider == null)
        {
            return NotFound(new { message = $"Rider with ID {id} not found" });
        }

        if (string.IsNullOrWhiteSpace(rider.Name) || string.IsNullOrWhiteSpace(rider.PhoneNumber))
        {
            return BadRequest(new { message = "Name and PhoneNumber are required" });
        }

        var updatedRider = new Rider(
            Id: id,
            Name: rider.Name,
            PhoneNumber: rider.PhoneNumber
        );

        var result = await _repository.UpdateRiderAsync(updatedRider);
        
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteRiderAsync(Guid id)
    {
        var deleted = await _repository.DeleteRiderAsync(id);
        
        if (!deleted)
        {
            return NotFound(new { message = $"Rider with ID {id} not found" });
        }
        
        return NoContent();
    }
}
