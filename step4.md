# SafeBoda Project - Step 4: Securing the API
## Comprehensive Line-by-Line Explanation

---

## Table of Contents
1. [Introduction](#introduction)
2. [What is Authentication?](#what-is-authentication)
3. [What is Authorization?](#what-is-authorization)
4. [What are JSON Web Tokens (JWT)?](#what-are-json-web-tokens-jwt)
5. [Task 1: Integrate ASP.NET Core Identity](#task-1-integrate-aspnet-core-identity)
6. [Task 2: Create the AuthController](#task-2-create-the-authcontroller)
7. [Task 3: Configure JWT Bearer Authentication](#task-3-configure-jwt-bearer-authentication)
8. [Task 4: Secure the TripsController](#task-4-secure-the-tripscontroller)
9. [Task 5: Define User Roles](#task-5-define-user-roles)
10. [Task 6: Create the AdminController](#task-6-create-the-admincontroller)
11. [Task 7: Test the Secured Endpoints](#task-7-test-the-secured-endpoints)
12. [Understanding JWT Tokens](#understanding-jwt-tokens)
13. [Understanding Claims](#understanding-claims)
14. [Summary](#summary)

---

## Introduction

Welcome to Step 4! In this module, we're solving a **critical security problem**: **our API is currently open to everyone!** 

Anyone on the internet can:
- View all trips
- Create fake trips
- Delete data
- Access sensitive information

This is a **major security risk**. We need to:
1. **Authenticate** users (verify who they are)
2. **Authorize** users (control what they can do)

**What we'll build:**
- User registration and login system
- JWT token-based authentication
- Role-based authorization (Rider, Driver, Admin)
- Secured API endpoints
- Admin-only features

**The transformation:**
- **Before**: API open to everyone ❌
- **After**: API secured with authentication and authorization ✅

---

## What is Authentication?

### The Problem: Who Are You?

**Authentication** answers the question: **"Who are you?"**

**Real-world analogy:**
- **Airport security**: Checks your ID (authentication)
- **Bank**: Asks for PIN (authentication)
- **Email**: Requires password (authentication)

**In our API:**
- User provides email and password
- System verifies credentials
- If valid, user is **authenticated**

### How Authentication Works

```
1. User sends credentials (email + password)
   ↓
2. System checks if credentials are valid
   ↓
3. If valid: User is authenticated ✅
   If invalid: Access denied ❌
   ↓
4. Authenticated user receives a token
   ↓
5. User includes token in future requests
```

### Types of Authentication

| Type | How It Works | Use Case |
|------|--------------|---------|
| **Username/Password** | User provides credentials | Web apps, APIs |
| **Token-based (JWT)** | Server issues token after login | APIs, mobile apps |
| **OAuth** | Third-party login (Google, Facebook) | Social login |
| **API Keys** | Static key for service-to-service | Service integration |

**We're using JWT tokens** - perfect for APIs!

---

## What is Authorization?

### The Problem: What Can You Do?

**Authorization** answers the question: **"What are you allowed to do?"**

**Real-world analogy:**
- **Hotel**: Guest can access their room (authorized)
- **Hotel**: Guest cannot access other rooms (not authorized)
- **Hotel**: Manager can access all rooms (different authorization level)

**In our API:**
- **Rider**: Can create trips, view their trips
- **Driver**: Can view available trips, accept trips
- **Admin**: Can view all trips, manage users

### How Authorization Works

```
1. User is authenticated (we know who they are)
   ↓
2. System checks user's role/permissions
   ↓
3. System checks if action is allowed
   ↓
4. If allowed: Request proceeds ✅
   If not allowed: Access denied ❌
```

### Authorization Levels

| Level | Description | Example |
|-------|-------------|---------|
| **Public** | Anyone can access | Public website |
| **Authenticated** | Must be logged in | User profile |
| **Role-based** | Must have specific role | Admin dashboard |
| **Resource-based** | Must own the resource | Edit your own post |

**We're using role-based authorization** - simple and effective!

---

## What are JSON Web Tokens (JWT)?

### The Problem: Stateless Authentication

**Traditional approach (Session-based):**
```
1. User logs in
2. Server creates session (stored in memory/database)
3. Server sends session ID (cookie)
4. User sends session ID with each request
5. Server looks up session to verify user
```

**Problem**: Server must store sessions (not scalable)

**JWT approach (Token-based):**
```
1. User logs in
2. Server creates JWT token (contains user info)
3. Server sends token to user
4. User sends token with each request
5. Server validates token (no database lookup needed!)
```

**Benefit**: Stateless (server doesn't store anything)

### What is a JWT?

**JWT** = **JSON Web Token**

A JWT is a **compact, URL-safe token** that contains:
- **Header**: Algorithm and token type
- **Payload**: User information (claims)
- **Signature**: Ensures token hasn't been tampered with

**Structure:**
```
header.payload.signature
```

**Example JWT:**
```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c
```

**Decoded:**
```json
Header:
{
  "alg": "HS256",
  "typ": "JWT"
}

Payload:
{
  "sub": "1234567890",
  "name": "John Doe",
  "iat": 1516239022
}
```

### How JWT Works

```
1. User logs in with email/password
   ↓
2. Server validates credentials
   ↓
3. Server creates JWT with user info
   ↓
4. Server signs JWT with secret key
   ↓
5. Server sends JWT to client
   ↓
6. Client stores JWT (localStorage, memory)
   ↓
7. Client sends JWT in Authorization header
   ↓
8. Server validates JWT signature
   ↓
9. Server extracts user info from JWT
   ↓
10. Request proceeds with user context
```

### JWT Benefits

- ✅ **Stateless**: Server doesn't store sessions
- ✅ **Scalable**: Works across multiple servers
- ✅ **Self-contained**: Token has all user info
- ✅ **Secure**: Signed with secret key
- ✅ **Standard**: Works with any language/framework

---

## Task 1: Integrate ASP.NET Core Identity

### What is ASP.NET Core Identity?

**ASP.NET Core Identity** is a membership system that provides:
- User management (create, update, delete users)
- Password hashing (secure password storage)
- Role management (assign roles to users)
- Authentication helpers (login, logout)
- Database integration (stores users in database)

**Why use it?**
- ✅ Built-in security (password hashing, validation)
- ✅ Database integration (works with EF Core)
- ✅ Role management
- ✅ Extensible (can customize)

### Adding Identity Packages

**Required packages:**
```bash
dotnet add SafeBoda.Api/SafeBoda.Api.csproj package Microsoft.AspNetCore.Identity.EntityFrameworkCore
```

**What this provides:**
- Identity services
- EF Core integration
- User and role management
- Password hashing

### Updating DbContext

**Modify `SafeBodaDbContext.cs`:**

```csharp
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SafeBoda.Infrastructure.Entities;

namespace SafeBoda.Infrastructure.Data;

public class SafeBodaDbContext : IdentityDbContext<ApplicationUser>
{
    public SafeBodaDbContext(DbContextOptions<SafeBodaDbContext> options) : base(options)
    {
    }

    public DbSet<TripEntity> Trips { get; set; }
    public DbSet<RiderEntity> Riders { get; set; }
    public DbSet<DriverEntity> Drivers { get; set; }
}
```

**Key change:**
- **Before**: `DbContext`
- **After**: `IdentityDbContext<ApplicationUser>`

**What this does:**
- Inherits Identity tables (Users, Roles, UserRoles, etc.)
- Provides user management functionality
- Integrates with EF Core

### Creating ApplicationUser

**Create `SafeBoda.Infrastructure/Entities/ApplicationUser.cs`:**

```csharp
using Microsoft.AspNetCore.Identity;

namespace SafeBoda.Infrastructure.Entities;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
}
```

**Explanation:**
- **`: IdentityUser`**: Inherits from Identity's user class
  - Gets: `Id`, `Email`, `UserName`, `PasswordHash`, etc.
- **`FullName`**: Custom property (our addition)
- **`?`**: Nullable (optional field)

**What IdentityUser provides:**
- `Id`: Unique identifier
- `Email`: User's email
- `UserName`: Username
- `PasswordHash`: Hashed password (never store plain text!)
- `EmailConfirmed`: Email verification status
- And more...

### Registering Identity in Program.cs

**Add to `Program.cs`:**

```csharp
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<SafeBodaDbContext>()
.AddDefaultTokenProviders();
```

**Line-by-line explanation:**

**Line 1: AddIdentity**
```csharp
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(...)
```
- **`AddIdentity<TUser, TRole>`**: Registers Identity services
- **`<ApplicationUser, IdentityRole>`**: 
  - User type: `ApplicationUser`
  - Role type: `IdentityRole` (built-in)

**Line 2-7: Password Options**
```csharp
options.Password.RequireDigit = true;
options.Password.RequireLowercase = true;
options.Password.RequireUppercase = true;
options.Password.RequireNonAlphanumeric = false;
options.Password.RequiredLength = 6;
```
- **Password requirements**:
  - Must have digit: `true`
  - Must have lowercase: `true`
  - Must have uppercase: `true`
  - Must have special character: `false` (we're lenient)
  - Minimum length: `6` characters

**Line 9: User Options**
```csharp
options.User.RequireUniqueEmail = true;
```
- Each email must be unique (no duplicates)

**Line 11: Entity Framework Stores**
```csharp
.AddEntityFrameworkStores<SafeBodaDbContext>()
```
- Tells Identity to use our `SafeBodaDbContext`
- Stores users, roles in database via EF Core

**Line 12: Default Token Providers**
```csharp
.AddDefaultTokenProviders();
```
- Provides token generation (for password reset, email confirmation, etc.)

### Creating Migration for Identity

**Run migration command:**
```bash
dotnet ef migrations add AddIdentityTables --project SafeBoda.Infrastructure --startup-project SafeBoda.Api
dotnet ef database update --project SafeBoda.Infrastructure --startup-project SafeBoda.Api
```

**What this creates:**
- `AspNetUsers` table (users)
- `AspNetRoles` table (roles)
- `AspNetUserRoles` table (user-role assignments)
- `AspNetUserClaims` table (user claims)
- And more Identity tables

---

## Task 2: Create the AuthController

### What is AuthController?

**AuthController** handles:
- User registration (sign up)
- User login (sign in)
- Token generation

### Creating RegisterDto

**Create `SafeBoda.Api/Models/RegisterDto.cs`:**

```csharp
namespace SafeBoda.Api.Models;

public class RegisterDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = "Rider"; // Default role
}
```

**Explanation:**
- **DTO** = Data Transfer Object (data sent from client)
- **`Email`**: User's email (will be username)
- **`Password`**: User's password (will be hashed)
- **`FullName`**: User's full name
- **`Role`**: User's role (Rider, Driver, or Admin)
- **Default**: "Rider" if not specified

### Creating LoginDto

**Create `SafeBoda.Api/Models/LoginDto.cs`:**

```csharp
namespace SafeBoda.Api.Models;

public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
```

**Explanation:**
- Simple DTO for login
- Just email and password needed

### Creating AuthController

**Create `SafeBoda.Api/Controllers/AuthController.cs`:**

```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SafeBoda.Api.Models;
using SafeBoda.Api.Services;
using SafeBoda.Infrastructure.Entities;

namespace SafeBoda.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly TokenService _tokenService;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        TokenService tokenService,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _roleManager = roleManager;
    }
}
```

**Line-by-line explanation:**

**Line 1-5: Using Statements**
```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SafeBoda.Api.Models;
using SafeBoda.Api.Services;
using SafeBoda.Infrastructure.Entities;
```
- **`Microsoft.AspNetCore.Identity`**: For `UserManager`, `SignInManager`, `RoleManager`
- **`Microsoft.AspNetCore.Mvc`**: For `ControllerBase`, `ApiController`
- **`SafeBoda.Api.Models`**: For DTOs
- **`SafeBoda.Api.Services`**: For `TokenService`
- **`SafeBoda.Infrastructure.Entities`**: For `ApplicationUser`

**Line 9-10: Controller Attributes**
```csharp
[ApiController]
[Route("api/[controller]")]
```
- **`[ApiController]`**: API controller features
- **`[Route("api/[controller]")]`**: Base route `api/auth`

**Line 13-16: Dependencies**
```csharp
private readonly UserManager<ApplicationUser> _userManager;
private readonly SignInManager<ApplicationUser> _signInManager;
private readonly TokenService _tokenService;
private readonly RoleManager<IdentityRole> _roleManager;
```
- **`UserManager`**: Manages users (create, find, update, delete)
- **`SignInManager`**: Handles sign-in operations
- **`TokenService`**: Generates JWT tokens (we'll create this)
- **`RoleManager`**: Manages roles (create, assign)

**Line 18-26: Constructor**
```csharp
public AuthController(...)
{
    _userManager = userManager;
    _signInManager = signInManager;
    _tokenService = tokenService;
    _roleManager = roleManager;
}
```
- **Dependency Injection**: Framework provides these services
- Stored in private fields for use in action methods

### Implementing Register Endpoint

```csharp
[HttpPost("register")]
public async Task<IActionResult> Register([FromBody] RegisterDto model)
{
    // Validate role
    if (model.Role != "Rider" && model.Role != "Driver" && model.Role != "Admin")
    {
        return BadRequest(new { message = "Invalid role. Must be 'Rider', 'Driver', or 'Admin'." });
    }

    var user = new ApplicationUser
    {
        UserName = model.Email,
        Email = model.Email,
        FullName = model.FullName
    };

    var result = await _userManager.CreateAsync(user, model.Password);

    if (!result.Succeeded)
    {
        return BadRequest(new { errors = result.Errors });
    }

    // Ensure role exists
    if (!await _roleManager.RoleExistsAsync(model.Role))
    {
        await _roleManager.CreateAsync(new IdentityRole(model.Role));
    }

    // Assign role to user
    await _userManager.AddToRoleAsync(user, model.Role);

    return Ok(new { message = "User registered successfully", email = user.Email, role = model.Role });
}
```

**Line-by-line explanation:**

**Line 1: Route**
```csharp
[HttpPost("register")]
```
- **Route**: `POST /api/auth/register`
- Handles user registration

**Line 2: Method Signature**
```csharp
public async Task<IActionResult> Register([FromBody] RegisterDto model)
```
- **`async Task<IActionResult>`**: Asynchronous method
- **`[FromBody]`**: Read JSON from request body
- **`RegisterDto model`**: Deserialized registration data

**Line 4-7: Role Validation**
```csharp
if (model.Role != "Rider" && model.Role != "Driver" && model.Role != "Admin")
{
    return BadRequest(new { message = "Invalid role..." });
}
```
- Validates role is one of allowed values
- Returns 400 Bad Request if invalid

**Line 9-13: Create User Object**
```csharp
var user = new ApplicationUser
{
    UserName = model.Email,
    Email = model.Email,
    FullName = model.FullName
};
```
- Creates new user object
- **`UserName`**: Set to email (Identity requirement)
- **`Email`**: User's email
- **`FullName`**: User's full name

**Line 15: Create User**
```csharp
var result = await _userManager.CreateAsync(user, model.Password);
```
- **`CreateAsync`**: Creates user in database
  - Hashes password automatically (never stores plain text!)
  - Validates email uniqueness
  - Validates password strength
- **Returns**: `IdentityResult` (success or errors)

**Line 17-20: Check Result**
```csharp
if (!result.Succeeded)
{
    return BadRequest(new { errors = result.Errors });
}
```
- If creation failed, return errors
- Common errors: weak password, duplicate email

**Line 23-26: Ensure Role Exists**
```csharp
if (!await _roleManager.RoleExistsAsync(model.Role))
{
    await _roleManager.CreateAsync(new IdentityRole(model.Role));
}
```
- Checks if role exists in database
- Creates role if it doesn't exist
- Ensures roles are available

**Line 29: Assign Role**
```csharp
await _userManager.AddToRoleAsync(user, model.Role);
```
- Assigns role to user
- Stored in `AspNetUserRoles` table

**Line 31: Return Success**
```csharp
return Ok(new { message = "User registered successfully", email = user.Email, role = model.Role });
```
- Returns 200 OK with success message

### Implementing Login Endpoint

```csharp
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginDto model)
{
    var user = await _userManager.FindByEmailAsync(model.Email);
    if (user == null)
    {
        return Unauthorized(new { message = "Invalid email or password" });
    }

    var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
    if (!result.Succeeded)
    {
        return Unauthorized(new { message = "Invalid email or password" });
    }

    var roles = await _userManager.GetRolesAsync(user);
    var token = _tokenService.GenerateToken(user, roles);

    return Ok(new
    {
        token,
        email = user.Email,
        fullName = user.FullName,
        roles
    });
}
```

**Line-by-line explanation:**

**Line 1: Route**
```csharp
[HttpPost("login")]
```
- **Route**: `POST /api/auth/login`

**Line 3: Find User**
```csharp
var user = await _userManager.FindByEmailAsync(model.Email);
```
- Looks up user by email
- Returns `null` if not found

**Line 4-7: Check User Exists**
```csharp
if (user == null)
{
    return Unauthorized(new { message = "Invalid email or password" });
}
```
- Returns 401 Unauthorized if user not found
- Generic message (don't reveal if email exists)

**Line 9: Verify Password**
```csharp
var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
```
- **`CheckPasswordSignInAsync`**: Verifies password
  - Compares hashed password in database with provided password
  - `false` = don't lock out on failure
- Returns `SignInResult` (success or failure)

**Line 10-13: Check Result**
```csharp
if (!result.Succeeded)
{
    return Unauthorized(new { message = "Invalid email or password" });
}
```
- Returns 401 if password is wrong

**Line 15: Get Roles**
```csharp
var roles = await _userManager.GetRolesAsync(user);
```
- Retrieves all roles assigned to user
- Returns list: `["Rider"]`, `["Driver"]`, etc.

**Line 16: Generate Token**
```csharp
var token = _tokenService.GenerateToken(user, roles);
```
- Calls `TokenService` to generate JWT
- Includes user info and roles in token

**Line 18-23: Return Token**
```csharp
return Ok(new
{
    token,
    email = user.Email,
    fullName = user.FullName,
    roles
});
```
- Returns 200 OK with:
  - **`token`**: JWT token (client stores this)
  - **`email`**: User's email
  - **`fullName`**: User's name
  - **`roles`**: User's roles

---

## Task 3: Configure JWT Bearer Authentication

### Creating TokenService

**Create `SafeBoda.Api/Services/TokenService.cs`:**

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SafeBoda.Infrastructure.Entities;

namespace SafeBoda.Api.Services;

public class TokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(ApplicationUser user, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.Name, user.FullName ?? user.Email!)
        };

        // Add role claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["JwtSettings:SecretKey"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                Convert.ToDouble(_configuration["JwtSettings:ExpiryInMinutes"])),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

**Line-by-line explanation:**

**Line 1-5: Using Statements**
```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
```
- **`System.IdentityModel.Tokens.Jwt`**: JWT token classes
- **`System.Security.Claims`**: Claims (user info)
- **`Microsoft.IdentityModel.Tokens`**: Token signing

**Line 11-15: Constructor**
```csharp
private readonly IConfiguration _configuration;

public TokenService(IConfiguration configuration)
{
    _configuration = configuration;
}
```
- **`IConfiguration`**: Access to appsettings.json
- Stores configuration for token generation

**Line 17: GenerateToken Method**
```csharp
public string GenerateToken(ApplicationUser user, IList<string> roles)
```
- **Returns**: JWT token as string
- **Parameters**: User and their roles

**Line 19-25: Create Claims**
```csharp
var claims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, user.Id),
    new Claim(ClaimTypes.Email, user.Email!),
    new Claim(ClaimTypes.Name, user.FullName ?? user.Email!)
};
```
- **Claims**: Pieces of information about the user
- **`NameIdentifier`**: User's ID (unique)
- **`Email`**: User's email
- **`Name`**: User's name (or email if no name)
- **`!`**: Null-forgiving operator (we know it's not null)

**Line 27-30: Add Role Claims**
```csharp
foreach (var role in roles)
{
    claims.Add(new Claim(ClaimTypes.Role, role));
}
```
- Adds each role as a claim
- Example: `ClaimTypes.Role = "Admin"`

**Line 32-34: Create Signing Key**
```csharp
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
    _configuration["JwtSettings:SecretKey"]!));
var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
```
- **`SymmetricSecurityKey`**: Secret key for signing
- **`HmacSha256`**: Signing algorithm
- **Secret key**: Must be kept secret! (in appsettings.json)

**Line 36-43: Create Token**
```csharp
var token = new JwtSecurityToken(
    issuer: _configuration["JwtSettings:Issuer"],
    audience: _configuration["JwtSettings:Audience"],
    claims: claims,
    expires: DateTime.UtcNow.AddMinutes(
        Convert.ToDouble(_configuration["JwtSettings:ExpiryInMinutes"])),
    signingCredentials: credentials
);
```
- **`issuer`**: Who created the token (our API)
- **`audience`**: Who the token is for (our clients)
- **`claims`**: User information
- **`expires`**: When token expires (e.g., 60 minutes)
- **`signingCredentials`**: How to sign the token

**Line 45: Return Token String**
```csharp
return new JwtSecurityTokenHandler().WriteToken(token);
```
- Converts token object to string
- Returns JWT as string (e.g., `eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`)

### Adding JWT Settings to appsettings.json

**Add to `appsettings.json`:**

```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLongForSafeBodaApi!",
    "Issuer": "SafeBodaApi",
    "Audience": "SafeBodaClient",
    "ExpiryInMinutes": 60
  }
}
```

**Explanation:**
- **`SecretKey`**: Secret for signing tokens (keep it secret!)
  - Should be at least 32 characters
  - Use strong random string in production
- **`Issuer`**: Who issues tokens (our API name)
- **`Audience`**: Who receives tokens (our clients)
- **`ExpiryInMinutes`**: Token lifetime (60 minutes)

**⚠️ Security Note**: In production, store secret key in environment variables or secure vault, not in appsettings.json!

### Configuring JWT Authentication in Program.cs

**Add to `Program.cs`:**

```csharp
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();
```

**Line-by-line explanation:**

**Line 1-2: Get JWT Settings**
```csharp
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];
```
- Reads JWT settings from appsettings.json
- Gets secret key for token validation

**Line 4-8: Add Authentication**
```csharp
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
```
- **`AddAuthentication`**: Registers authentication services
- **`DefaultAuthenticateScheme`**: How to authenticate (JWT Bearer)
- **`DefaultChallengeScheme`**: How to challenge unauthenticated requests
- **`DefaultScheme`**: Default authentication scheme

**Line 9-20: Configure JWT Bearer**
```csharp
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!)),
        ClockSkew = TimeSpan.Zero
    };
});
```
- **`SaveToken`**: Save token in `HttpContext` (for later use)
- **`RequireHttpsMetadata`**: Don't require HTTPS in development
- **`TokenValidationParameters`**: How to validate tokens
  - **`ValidateIssuer`**: Check issuer matches
  - **`ValidateAudience`**: Check audience matches
  - **`ValidateLifetime`**: Check token hasn't expired
  - **`ValidateIssuerSigningKey`**: Check signature is valid
  - **`ValidIssuer`**: Expected issuer
  - **`ValidAudience`**: Expected audience
  - **`IssuerSigningKey`**: Key to verify signature
  - **`ClockSkew`**: Time tolerance (Zero = no tolerance)

**Line 22: Add Authorization**
```csharp
builder.Services.AddAuthorization();
```
- Registers authorization services
- Enables `[Authorize]` attribute

### Adding Middleware

**Add to `Program.cs` (after `app = builder.Build()`):**

```csharp
app.UseAuthentication();
app.UseAuthorization();
```

**Order matters!**
```csharp
app.UseAuthentication();  // Must come first
app.UseAuthorization();    // Then authorization
app.MapControllers();      // Finally, routing
```

**What this does:**
- **`UseAuthentication`**: Validates JWT tokens
- **`UseAuthorization`**: Checks permissions

### Registering TokenService

**Add to `Program.cs`:**

```csharp
builder.Services.AddScoped<TokenService>();
```

- Registers `TokenService` for dependency injection
- Scoped lifetime (one per request)

---

## Task 4: Secure the TripsController

### Adding [Authorize] Attribute

**Modify `TripsController.cs`:**

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeBoda.Application;
using SafeBoda.Core;

namespace SafeBoda.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]  // <-- Add this!
public class TripsController : ControllerBase
{
    // ... rest of controller
}
```

**What this does:**
- **`[Authorize]`**: Requires authentication
- All actions in controller require valid JWT token
- Unauthenticated requests return 401 Unauthorized

### How It Works

```
1. Client sends request with JWT token
   Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
   ↓
2. UseAuthentication middleware validates token
   ↓
3. If valid: Request proceeds ✅
   If invalid: Returns 401 Unauthorized ❌
   ↓
4. UseAuthorization middleware checks [Authorize]
   ↓
5. If authorized: Action executes ✅
   If not authorized: Returns 403 Forbidden ❌
```

### Testing Secured Endpoint

**Without token:**
```bash
curl https://localhost:7244/api/trips
```
**Response**: `401 Unauthorized`

**With token:**
```bash
curl https://localhost:7244/api/trips \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```
**Response**: `200 OK` with trips data

---

## Task 5: Define User Roles

### What are Roles?

**Roles** are labels that group users with similar permissions:
- **Rider**: Can book trips
- **Driver**: Can accept trips
- **Admin**: Can manage everything

### Creating Roles

**Roles are created automatically** when users register (see Register endpoint).

**Manual creation (for seeding):**

Add to `Program.cs` (after `app = builder.Build()`):

```csharp
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    
    string[] roles = { "Admin", "Rider", "Driver" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}
```

**What this does:**
- Creates roles if they don't exist
- Runs on application startup
- Ensures roles are available

### Assigning Roles

**During registration:**
- User specifies role in `RegisterDto`
- Role is assigned automatically

**Manually (for admin user):**
```csharp
var adminUser = await _userManager.FindByEmailAsync("admin@safeboda.com");
await _userManager.AddToRoleAsync(adminUser, "Admin");
```

---

## Task 6: Create the AdminController

### What is AdminController?

**AdminController** provides admin-only endpoints:
- View all users
- View all trips
- Manage users
- View statistics

### Creating AdminController

**Create `SafeBoda.Api/Controllers/AdminController.cs`:**

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
}
```

**Key features:**

**Line 13: Role-based Authorization**
```csharp
[Authorize(Roles = "Admin")]
```
- **`[Authorize(Roles = "Admin")]`**: Only Admin role can access
- All actions in controller require Admin role
- Non-admin users get 403 Forbidden

**Line 25: Get All Users**
```csharp
[HttpGet("users")]
public async Task<IActionResult> GetAllUsers()
```
- **Route**: `GET /api/admin/users`
- Returns all users with their roles
- Only accessible to Admins

### Creating Admin User

**For testing, create admin user manually:**

**Option 1: Via API (if you have a way to create admin)**
```json
POST /api/auth/register
{
  "email": "admin@safeboda.com",
  "password": "Admin@123",
  "fullName": "System Administrator",
  "role": "Admin"
}
```

**Option 2: Seed in Program.cs**
```csharp
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    
    // Create Admin role
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }
    
    // Create admin user
    var adminEmail = "admin@safeboda.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "System Administrator",
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(adminUser, "Admin@123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}
```

---

## Task 7: Test the Secured Endpoints

### Testing with Swagger UI

**1. Start the API:**
```bash
dotnet run
```

**2. Open Swagger UI:**
Navigate to `https://localhost:7244/swagger`

**3. Register a user:**
- Find `POST /api/auth/register`
- Click "Try it out"
- Enter:
  ```json
  {
    "email": "rider@test.com",
    "password": "Password123",
    "fullName": "Test Rider",
    "role": "Rider"
  }
  ```
- Click "Execute"
- Should return success

**4. Login:**
- Find `POST /api/auth/login`
- Click "Try it out"
- Enter:
  ```json
  {
    "email": "rider@test.com",
    "password": "Password123"
  }
  ```
- Click "Execute"
- **Copy the token** from response

**5. Authorize in Swagger:**
- Click "Authorize" button (top right)
- Enter: `Bearer YOUR_TOKEN_HERE`
- Click "Authorize"
- Close dialog

**6. Test secured endpoint:**
- Find `GET /api/trips`
- Click "Try it out"
- Click "Execute"
- Should return trips (with token) or 401 (without token)

### Testing with Postman

**1. Register User:**
```
POST https://localhost:7244/api/auth/register
Content-Type: application/json

{
  "email": "rider@test.com",
  "password": "Password123",
  "fullName": "Test Rider",
  "role": "Rider"
}
```

**2. Login:**
```
POST https://localhost:7244/api/auth/login
Content-Type: application/json

{
  "email": "rider@test.com",
  "password": "Password123"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "rider@test.com",
  "fullName": "Test Rider",
  "roles": ["Rider"]
}
```

**3. Use Token:**
```
GET https://localhost:7244/api/trips
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**4. Test Admin Endpoint:**
```
GET https://localhost:7244/api/admin/users
Authorization: Bearer ADMIN_TOKEN_HERE
```

**Without Admin role**: Returns `403 Forbidden`
**With Admin role**: Returns `200 OK` with users

### Testing with curl

**1. Register:**
```bash
curl -X POST https://localhost:7244/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "rider@test.com",
    "password": "Password123",
    "fullName": "Test Rider",
    "role": "Rider"
  }'
```

**2. Login:**
```bash
curl -X POST https://localhost:7244/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "rider@test.com",
    "password": "Password123"
  }'
```

**Save the token from response!**

**3. Use Token:**
```bash
curl https://localhost:7244/api/trips \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

---

## Understanding JWT Tokens

### Token Structure

**JWT has 3 parts (separated by dots):**
```
header.payload.signature
```

**Example:**
```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c
```

**1. Header (Base64 encoded):**
```json
{
  "alg": "HS256",
  "typ": "JWT"
}
```
- Algorithm: HMAC SHA256
- Type: JWT

**2. Payload (Base64 encoded):**
```json
{
  "sub": "1234567890",
  "name": "John Doe",
  "iat": 1516239022,
  "exp": 1516242622,
  "role": "Rider"
}
```
- **`sub`**: Subject (user ID)
- **`name`**: User's name
- **`iat`**: Issued at (timestamp)
- **`exp`**: Expires at (timestamp)
- **`role`**: User's role

**3. Signature:**
```
HMACSHA256(
  base64UrlEncode(header) + "." + base64UrlEncode(payload),
  secret
)
```
- Signed with secret key
- Prevents tampering

### Token Validation

**When server receives token:**
1. Split into 3 parts
2. Decode header and payload
3. Verify signature (using secret key)
4. Check expiration (`exp` claim)
5. Check issuer and audience
6. Extract user info from claims

**If all checks pass**: Token is valid ✅
**If any check fails**: Token is invalid ❌

---

## Understanding Claims

### What are Claims?

**Claims** are pieces of information about the user:
- User ID
- Email
- Name
- Roles
- Permissions

**Think of claims as "facts" about the user.**

### Standard Claims

| Claim Type | Description | Example |
|------------|-------------|---------|
| **NameIdentifier** | User's unique ID | `"550e8400-..."` |
| **Email** | User's email | `"user@example.com"` |
| **Name** | User's name | `"John Doe"` |
| **Role** | User's role | `"Admin"` |

### Accessing Claims in Controller

```csharp
[HttpGet]
[Authorize]
public async Task<IActionResult> GetProfile()
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    var email = User.FindFirstValue(ClaimTypes.Email);
    var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value);
    
    return Ok(new { userId, email, roles });
}
```

**`User`** is a `ClaimsPrincipal` object:
- Contains all claims from JWT
- Available in controllers with `[Authorize]`
- Extracted automatically by authentication middleware

---

## Summary

In Step 4, we:

1. ✅ Integrated ASP.NET Core Identity
2. ✅ Created AuthController with register/login
3. ✅ Configured JWT Bearer authentication
4. ✅ Secured TripsController with [Authorize]
5. ✅ Defined user roles (Rider, Driver, Admin)
6. ✅ Created AdminController with role-based authorization
7. ✅ Tested secured endpoints

**Key Concepts Learned:**
- **Authentication**: Verifying who the user is
- **Authorization**: Controlling what users can do
- **JWT Tokens**: Stateless authentication tokens
- **Claims**: User information in tokens
- **Roles**: Grouping users by permissions
- **ASP.NET Core Identity**: User management system
- **Bearer Authentication**: Token-based authentication

**The Transformation:**
- **Before**: API open to everyone ❌
- **After**: API secured with authentication and authorization ✅

**What We Built:**
- User registration and login
- JWT token generation
- Secured API endpoints
- Role-based access control
- Admin-only features

**What's Next?**
In future steps, we'll:
- Add password reset functionality
- Add email verification
- Add refresh tokens
- Add more granular permissions
- Deploy with secure configuration

---

## Common Questions

**Q: Why use JWT instead of sessions?**
A: JWTs are stateless (no server storage), scalable (works across servers), and perfect for APIs and mobile apps.

**Q: How long should tokens last?**
A: Depends on security needs. Short-lived (15-60 minutes) for high security, longer (hours/days) for convenience. Use refresh tokens for balance.

**Q: What if token is stolen?**
A: Tokens can be revoked by changing secret key (invalidates all tokens) or using token blacklist. Short expiration helps limit damage.

**Q: Can I have multiple roles?**
A: Yes! Users can have multiple roles. Check with `User.IsInRole("Admin")` or `[Authorize(Roles = "Admin,Manager")]`.

**Q: What's the difference between [Authorize] and [Authorize(Roles = "Admin")]?**
A: `[Authorize]` requires any authenticated user. `[Authorize(Roles = "Admin")]` requires Admin role specifically.

**Q: How do I refresh tokens?**
A: Implement refresh token endpoint that issues new JWT when refresh token is valid. Refresh tokens have longer expiration.

**Q: Should I store tokens in localStorage or cookies?**
A: Cookies are more secure (HttpOnly, SameSite), but localStorage works for SPAs. Consider XSS risks with localStorage.

**Q: What if user forgets password?**
A: Implement password reset flow: generate reset token, send email, verify token, allow new password.

---

## Conclusion

Congratulations! You've secured the SafeBoda API! 🎉

**What you've achieved:**
- Users must authenticate to access API
- Role-based access control
- Secure token-based authentication
- Admin-only features
- Production-ready security foundation

**The journey:**
- **Step 1**: Domain models
- **Step 2**: Web API endpoints
- **Step 3**: Database persistence
- **Step 4**: Authentication and authorization ✅

Your API is now **secure** and ready for real users! Users' data is protected, and only authorized users can access sensitive features.

**Remember**: Security is not optional - it's essential. The authentication and authorization system you've built protects your application and your users' data! 🔒

