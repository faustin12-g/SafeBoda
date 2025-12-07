using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafeBoda.Api.Models;
using SafeBoda.Infrastructure.Data;
using SafeBoda.Infrastructure.Entities;

namespace SafeBoda.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly SafeBodaDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(SafeBodaDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userManager.Users.ToListAsync();
        
        var userList = new List<object>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userList.Add(new
            {
                user.Id,
                user.Email,
                user.FullName,
                Roles = roles
            });
        }
        
        return Ok(userList);
    }

    [HttpGet("trips")]
    public async Task<IActionResult> GetAllTrips()
    {
        var trips = await _context.Trips.ToListAsync();
        return Ok(trips);
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var totalUsers = await _userManager.Users.CountAsync();
        var totalTrips = await _context.Trips.CountAsync();
        var totalRiders = await _context.Riders.CountAsync();
        var totalDrivers = await _context.Drivers.CountAsync();

        return Ok(new
        {
            totalUsers,
            totalTrips,
            totalRiders,
            totalDrivers
        });
    }

    [HttpGet("riders")]
    public async Task<IActionResult> GetAllRiders()
    {
        var riders = await _context.Riders.ToListAsync();
        return Ok(riders);
    }

    [HttpGet("drivers")]
    public async Task<IActionResult> GetAllDrivers()
    {
        var drivers = await _context.Drivers.ToListAsync();
        return Ok(drivers);
    }

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || 
            string.IsNullOrWhiteSpace(request.FullName) || 
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Email, Full Name, and Password are required" });
        }

        if (request.Roles == null || !request.Roles.Any())
        {
            return BadRequest(new { message = "At least one role must be assigned" });
        }

        // Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return BadRequest(new { message = "A user with this email already exists" });
        }

        // Create new user
        var newUser = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(newUser, request.Password);
        
        if (!result.Succeeded)
        {
            return BadRequest(new { message = "Failed to create user", errors = result.Errors });
        }

        // Assign roles
        foreach (var role in request.Roles)
        {
            if (await _userManager.Users.AnyAsync())
            {
                await _userManager.AddToRoleAsync(newUser, role);
            }
        }

        var roles = await _userManager.GetRolesAsync(newUser);
        
        return CreatedAtAction(nameof(GetAllUsers), new
        {
            newUser.Id,
            newUser.Email,
            newUser.FullName,
            Roles = roles
        });
    }

    [HttpDelete("users/{userId}")]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            return BadRequest(new { message = "Failed to delete user", errors = result.Errors });
        }

        return Ok(new { message = "User deleted successfully" });
    }
}
