using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeBoda.Application;
using SafeBoda.Core;

namespace SafeBoda.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class DriversController : ControllerBase
{
    private readonly ITripRepository _repository;

    public DriversController(ITripRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Driver>> GetDriverByIdAsync(Guid id)
    {
        var driver = await _repository.GetDriverByIdAsync(id);
        
        if (driver == null)
        {
            return NotFound(new { message = $"Driver with ID {id} not found" });
        }
        
        return Ok(driver);
    }

    [HttpPost]
    public async Task<ActionResult<Driver>> CreateDriverAsync([FromBody] Driver driver)
    {
        if (string.IsNullOrWhiteSpace(driver.Name) || 
            string.IsNullOrWhiteSpace(driver.PhoneNumber) || 
            string.IsNullOrWhiteSpace(driver.MotoPlateNumber))
        {
            return BadRequest(new { message = "Name, PhoneNumber, and MotoPlateNumber are required" });
        }

        var newDriver = new Driver(
            Id: Guid.NewGuid(),
            Name: driver.Name,
            PhoneNumber: driver.PhoneNumber,
            MotoPlateNumber: driver.MotoPlateNumber
        );

        var createdDriver = await _repository.CreateDriverAsync(newDriver);
        
        // ASP.NET Core strips "Async" suffix from action names for routing
        return CreatedAtAction("GetDriverById", new { id = createdDriver.Id }, createdDriver);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Driver>> UpdateDriverAsync(Guid id, [FromBody] Driver driver)
    {
        var existingDriver = await _repository.GetDriverByIdAsync(id);
        
        if (existingDriver == null)
        {
            return NotFound(new { message = $"Driver with ID {id} not found" });
        }

        if (string.IsNullOrWhiteSpace(driver.Name) || 
            string.IsNullOrWhiteSpace(driver.PhoneNumber) || 
            string.IsNullOrWhiteSpace(driver.MotoPlateNumber))
        {
            return BadRequest(new { message = "Name, PhoneNumber, and MotoPlateNumber are required" });
        }

        var updatedDriver = new Driver(
            Id: id,
            Name: driver.Name,
            PhoneNumber: driver.PhoneNumber,
            MotoPlateNumber: driver.MotoPlateNumber
        );

        var result = await _repository.UpdateDriverAsync(updatedDriver);
        
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteDriverAsync(Guid id)
    {
        var deleted = await _repository.DeleteDriverAsync(id);
        
        if (!deleted)
        {
            return NotFound(new { message = $"Driver with ID {id} not found" });
        }
        
        return NoContent();
    }
}
