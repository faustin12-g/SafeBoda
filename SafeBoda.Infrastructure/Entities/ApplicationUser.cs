using Microsoft.AspNetCore.Identity;

namespace SafeBoda.Infrastructure.Entities;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
}
