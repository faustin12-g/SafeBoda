# SafeBoda Project - Step 2: Exposing Trips via API
## Comprehensive Line-by-Line Explanation

---

## Table of Contents
1. [Introduction](#introduction)
2. [What is an API?](#what-is-an-api)
3. [Step 1: Creating the API Project](#step-1-creating-the-api-project)
4. [Step 2: Adding Project References](#step-2-adding-project-references)
5. [Step 3: Understanding Dependency Injection](#step-3-understanding-dependency-injection)
6. [Step 4: Registering Dependencies in Program.cs](#step-4-registering-dependencies-in-programcs)
7. [Step 5: Creating the Controller](#step-5-creating-the-controller)
8. [Step 6: Implementing GET Endpoint](#step-6-implementing-get-endpoint)
9. [Step 7: Implementing POST Endpoint](#step-7-implementing-post-endpoint)
10. [Step 8: Configuring Swagger](#step-8-configuring-swagger)
11. [Step 9: Testing the API](#step-9-testing-the-api)
12. [Understanding HTTP Methods](#understanding-http-methods)
13. [Understanding Routing](#understanding-routing)
14. [Summary](#summary)

---

## Introduction

Welcome to Step 2! In this module, we're going to transform our SafeBoda application from a collection of classes and interfaces into a **real, working API** that can be accessed over the internet. This is a huge step - we're making our application accessible to mobile apps, web browsers, and other services!

**What we'll build:**
- A Web API that exposes trip data
- Endpoints that clients can call to get and create trips
- Interactive documentation using Swagger
- A foundation for future API endpoints

---

## What is an API?

**API** stands for **Application Programming Interface**. Think of it as a **menu in a restaurant**:

- The **menu** (API) tells you what dishes (endpoints) are available
- You **order** (make a request) by choosing a dish
- The **kitchen** (server) prepares your order (processes the request)
- You **receive** your food (get a response)

In our case:
- **API** = The menu of available operations (get trips, create trips, etc.)
- **Endpoint** = A specific dish on the menu (like "GET /api/trips")
- **Request** = Your order (what you want)
- **Response** = The food (the data you get back)

**Why do we need an API?**
- Mobile apps need to communicate with our server
- Web browsers need to fetch data
- Other services might want to integrate with SafeBoda
- It provides a standard way to access our data

**RESTful API:**
Our API follows **REST** principles:
- **R**epresentational **S**tate **T**ransfer
- Uses standard HTTP methods (GET, POST, PUT, DELETE)
- Returns data in JSON format
- Stateless (each request is independent)

---

## Step 1: Creating the API Project

### The Command
```bash
dotnet new webapi -n SafeBoda.Api
```

**Breaking it down:**
- **`dotnet new`**: Create something new
- **`webapi`**: Template for a Web API project (not a website, but an API)
- **`-n SafeBoda.Api`**: Name it "SafeBoda.Api"

### What This Creates
When you run this command, it creates:
1. **`SafeBoda.Api/`** folder
2. **`SafeBoda.Api.csproj`** - Project file
3. **`Program.cs`** - Entry point and configuration
4. **`Controllers/`** folder - Where we'll put our controllers
5. **`appsettings.json`** - Configuration file
6. **`Properties/launchSettings.json`** - How to run the project

### Adding to Solution
After creating, add it to the solution:
```bash
dotnet sln add SafeBoda.Api/SafeBoda.Api.csproj
```

### Understanding the API Project File
Let's look at `SafeBoda.Api.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>
```

**Key difference from Class Library:**
- **`Sdk="Microsoft.NET.Sdk.Web"`** instead of `Microsoft.NET.Sdk`
  - This adds all the web/API functionality
  - Includes HTTP server, routing, controllers, etc.
  - Makes it a **runnable** project (not just a library)

**What's different about a Web API project?**
- It can **run** and listen for HTTP requests
- It has a built-in web server (Kestrel)
- It can handle routing, authentication, etc.
- It's designed to return JSON data (not HTML pages)

---

## Step 2: Adding Project References

### Why We Need References
Our API project needs to use:
- **SafeBoda.Core**: To use the `Trip`, `Rider`, `Driver`, `Location` models
- **SafeBoda.Application**: To use the `ITripRepository` interface

### The Command
```bash
dotnet add SafeBoda.Api/SafeBoda.Api.csproj reference SafeBoda.Core/SafeBoda.Core.csproj
dotnet add SafeBoda.Api/SafeBoda.Api.csproj reference SafeBoda.Application/SafeBoda.Application.csproj
```

**What this does:**
- Tells the API project: "You can use code from these projects"
- When building, it will also build the referenced projects
- Makes the namespaces available (with `using` statements)

### The Result in Project File
After adding references, `SafeBoda.Api.csproj` looks like:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\SafeBoda.Core\SafeBoda.Core.csproj" />
    <ProjectReference Include="..\SafeBoda.Application\SafeBoda.Application.csproj" />
  </ItemGroup>
</Project>
```

**The `<ItemGroup>` section:**
- Contains all project references
- **`Include="..\SafeBoda.Core\..."`**: 
  - `..\` means "go up one folder"
  - Then navigate to the referenced project
- Each reference tells the build system: "Build this project first, then use it"

**Dependency Chain:**
```
SafeBoda.Api
  ├──→ SafeBoda.Application
  │     └──→ SafeBoda.Core
  └──→ SafeBoda.Core (direct reference too)
```

This means:
- API can use Application interfaces
- API can use Core models directly
- Application can use Core models
- Core has no dependencies (pure models)

---

## Step 3: Understanding Dependency Injection

### What is Dependency Injection?
**Dependency Injection (DI)** is a design pattern that makes code more flexible and testable. Instead of creating objects yourself, you **ask for them** and the framework provides them.

**Real-world analogy:**
- **Without DI**: You go to the store, buy ingredients, and cook yourself
- **With DI**: You go to a restaurant and order - they bring you the food (you don't care how they made it)

**In code:**
- **Without DI**: `var repo = new InMemoryTripRepository();` (you create it)
- **With DI**: Constructor receives `ITripRepository` (framework provides it)

### Why Use Dependency Injection?
1. **Testability**: Easy to swap real repository with a fake one for testing
2. **Flexibility**: Change implementation without changing code that uses it
3. **Loose Coupling**: Classes don't depend on concrete implementations
4. **Lifecycle Management**: Framework handles when to create/destroy objects

### Dependency Injection Container
ASP.NET Core has a built-in **DI Container** (also called a Service Container). It's like a **registry** that knows:
- What services are available
- How to create them
- When to create them (lifetime)

**Service Lifetimes:**
- **Singleton**: Created once, shared everywhere (like a global variable)
- **Scoped**: Created once per HTTP request (new for each API call)
- **Transient**: Created new every time it's requested

**For our repository, we use Scoped:**
- Each HTTP request gets its own repository instance
- Safe for concurrent requests
- Data is isolated per request

---

## Step 4: Registering Dependencies in Program.cs

### What is Program.cs?
`Program.cs` is the **entry point** of our API. It's the first code that runs when the application starts. It's responsible for:
- Configuring services (DI registration)
- Setting up middleware (request pipeline)
- Starting the web server

### Understanding Program.cs Line by Line

Let's examine the key parts:

#### Part 1: Creating the Builder
```csharp
var builder = WebApplication.CreateBuilder(args);
```

**Explanation:**
- **`WebApplication.CreateBuilder(args)`**: Creates a builder object
  - `args` are command-line arguments
  - The builder helps us configure the application
- **`var`**: C# keyword that lets the compiler figure out the type
  - Equivalent to `WebApplicationBuilder builder = ...`

**What the builder does:**
- Reads configuration files (`appsettings.json`)
- Sets up logging
- Prepares the DI container
- Configures the web server

#### Part 2: Adding Controllers
```csharp
builder.Services.AddControllers();
```

**Explanation:**
- **`builder.Services`**: Access to the DI container
- **`AddControllers()`**: Registers the controller system
  - Enables routing to controller actions
  - Sets up model binding (converting JSON to C# objects)
  - Configures API conventions

**What this enables:**
- We can create controller classes
- HTTP requests will be routed to controller methods
- JSON will be automatically converted to/from C# objects

#### Part 3: Registering Our Repository
```csharp
builder.Services.AddScoped<ITripRepository, InMemoryTripRepository>();
```

**Line-by-line breakdown:**
- **`builder.Services`**: The DI container
- **`AddScoped`**: Register with "Scoped" lifetime
  - New instance per HTTP request
  - Perfect for repositories (database-like operations)
- **`<ITripRepository, InMemoryTripRepository>`**: 
  - **First type** (`ITripRepository`): The interface (what we ask for)
  - **Second type** (`InMemoryTripRepository`): The implementation (what we get)
  - This is a **generic method** - the types go in `<>`

**What this means:**
- When someone asks for `ITripRepository` (in a constructor)
- The framework will create and provide `InMemoryTripRepository`
- It will be scoped (new per request)

**Why interface first, then implementation?**
- Code asks for the interface (loose coupling)
- We can swap implementations without changing code
- Example: Later, swap `InMemoryTripRepository` for `DatabaseTripRepository`

#### Part 4: Building the App
```csharp
var app = builder.Build();
```

**Explanation:**
- **`builder.Build()`**: Creates the `WebApplication` object
  - All services are registered
  - Configuration is loaded
  - Ready to configure middleware

**After this line:**
- We can't add more services
- We configure the request pipeline (middleware)

#### Part 5: Mapping Controllers
```csharp
app.MapControllers();
```

**Explanation:**
- **`app.MapControllers()`**: Enables controller routing
  - Scans for controller classes
  - Maps HTTP routes to controller actions
  - Example: `GET /api/trips` → `TripsController.GetActiveTrips()`

#### Part 6: Running the App
```csharp
app.Run();
```

**Explanation:**
- **`app.Run()`**: Starts the web server
  - Listens for HTTP requests
  - Blocks execution (runs forever until stopped)
  - This is where the API "lives"

### Complete Basic Program.cs (Simplified)
```csharp
using SafeBoda.Application;

// Step 1: Create builder
var builder = WebApplication.CreateBuilder(args);

// Step 2: Register services
builder.Services.AddControllers();
builder.Services.AddScoped<ITripRepository, InMemoryTripRepository>();

// Step 3: Build the app
var app = builder.Build();

// Step 4: Configure middleware
app.MapControllers();

// Step 5: Run
app.Run();
```

**The Request Pipeline:**
```
HTTP Request
    ↓
app.MapControllers() (routing)
    ↓
Controller Action (your code)
    ↓
HTTP Response
```

---

## Step 5: Creating the Controller

### What is a Controller?
A **Controller** is a class that handles HTTP requests. It's like a **waiter** in a restaurant:
- Receives the order (HTTP request)
- Gets what's needed (from repository via DI)
- Prepares the response (processes data)
- Serves it back (returns HTTP response)

### Controller Naming Convention
- Name ends with "Controller" (e.g., `TripsController`)
- Inherits from `ControllerBase`
- Lives in the `Controllers` folder

### Creating TripsController

Let's create `Controllers/TripsController.cs`:

```csharp
using Microsoft.AspNetCore.Mvc;
using SafeBoda.Application;
using SafeBoda.Core;

namespace SafeBoda.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TripsController : ControllerBase
{
    private readonly ITripRepository _tripRepository;

    public TripsController(ITripRepository tripRepository)
    {
        _tripRepository = tripRepository;
    }
}
```

**Line-by-line explanation:**

**Line 1-3: Using Statements**
```csharp
using Microsoft.AspNetCore.Mvc;
using SafeBoda.Application;
using SafeBoda.Core;
```
- **`Microsoft.AspNetCore.Mvc`**: Provides `ControllerBase`, `ApiController`, routing attributes
- **`SafeBoda.Application`**: For `ITripRepository` interface
- **`SafeBoda.Core`**: For `Trip` model

**Line 5: Namespace**
```csharp
namespace SafeBoda.Api.Controllers;
```
- File-scoped namespace (C# 10+)
- Groups this controller with other controllers

**Line 7: ApiController Attribute**
```csharp
[ApiController]
```
- **Attribute**: Metadata that adds behavior to the class
- **`[ApiController]`**: Enables API-specific features:
  - Automatic model validation
  - Automatic HTTP 400 responses for bad requests
  - Problem details for errors
  - Attribute routing requirement

**Line 8: Route Attribute**
```csharp
[Route("api/[controller]")]
```
- **`[Route(...)]`**: Defines the base route for all actions in this controller
- **`"api/[controller]"`**: 
  - `api/` is literal (always "api")
  - `[controller]` is a **token** that gets replaced with the controller name
  - For `TripsController`, `[controller]` becomes `Trips`
  - Final route: `api/trips`

**Example:**
- Controller: `TripsController` → Route: `api/trips`
- Controller: `UsersController` → Route: `api/users`
- Controller: `DriversController` → Route: `api/drivers`

**Line 9: Class Declaration**
```csharp
public class TripsController : ControllerBase
```
- **`public class`**: Public class (accessible from framework)
- **`TripsController`**: Class name (must end with "Controller")
- **`: ControllerBase`**: Inherits from `ControllerBase`
  - Provides HTTP response methods (`Ok()`, `NotFound()`, etc.)
  - Provides access to request/response objects
  - Provides model binding

**Line 11: Private Field**
```csharp
private readonly ITripRepository _tripRepository;
```
- **`private`**: Only accessible within this class
- **`readonly`**: Can only be set in constructor (immutable after construction)
- **`ITripRepository`**: The interface type
- **`_tripRepository`**: Field name (underscore prefix is C# convention for private fields)
- This stores the repository instance we'll receive via DI

**Line 13-16: Constructor**
```csharp
public TripsController(ITripRepository tripRepository)
{
    _tripRepository = tripRepository;
}
```
- **`public TripsController(...)`**: Constructor (runs when object is created)
- **`ITripRepository tripRepository`**: Parameter
  - Framework will provide this automatically (DI)
  - It will be an `InMemoryTripRepository` instance (as we registered)
- **`_tripRepository = tripRepository;`**: Store it in the field
  - Now we can use `_tripRepository` in our action methods

**Why constructor injection?**
- Framework creates the controller
- Framework sees it needs `ITripRepository`
- Framework looks up registered service
- Framework provides `InMemoryTripRepository`
- Framework calls constructor with it
- We store it for later use

---

## Step 6: Implementing GET Endpoint

### What is a GET Request?
**GET** is an HTTP method used to **retrieve** data. It's like asking "Can I see the menu?" or "What trips are available?"

**Characteristics:**
- **Safe**: Doesn't change data (idempotent)
- **Read-only**: Just fetches information
- **Can be cached**: Browser can cache responses

### The GET Action Method

```csharp
[HttpGet]
public async Task<ActionResult<IEnumerable<Trip>>> GetActiveTripsAsync()
{
    var trips = await _tripRepository.GetActiveTripsAsync();
    return Ok(trips);
}
```

**Line-by-line explanation:**

**Line 1: HttpGet Attribute**
```csharp
[HttpGet]
```
- **`[HttpGet]`**: Marks this method as handling GET requests
- No route specified, so it uses the controller's base route
- Final route: `GET /api/trips`

**Line 2: Method Signature**
```csharp
public async Task<ActionResult<IEnumerable<Trip>>> GetActiveTripsAsync()
```
- **`public`**: Must be public (framework needs to call it)
- **`async`**: Asynchronous method (can use `await`)
  - Doesn't block the thread while waiting
  - Better performance for I/O operations
- **`Task<...>`**: Represents an asynchronous operation
  - `Task` = "Something that will complete"
  - The `<>` contains what it returns
- **`ActionResult<IEnumerable<Trip>>`**: Return type
  - **`ActionResult<T>`**: Can return different HTTP responses
    - `Ok(trips)` → 200 OK with data
    - `NotFound()` → 404 Not Found
    - `BadRequest()` → 400 Bad Request
  - **`IEnumerable<Trip>`**: The data type when successful
    - Collection of `Trip` objects
- **`GetActiveTripsAsync()`**: Method name
  - `Async` suffix is convention for async methods

**Line 4: Getting Data**
```csharp
var trips = await _tripRepository.GetActiveTripsAsync();
```
- **`var trips`**: Variable to store the result
- **`await`**: Wait for the async operation to complete
  - Doesn't block the thread
  - Returns control to caller, resumes when done
- **`_tripRepository.GetActiveTripsAsync()`**: Call repository method
  - Uses the repository we got via DI
  - This might take time (database query, etc.)
  - `await` makes it non-blocking

**Line 5: Returning Response**
```csharp
return Ok(trips);
```
- **`Ok(trips)`**: Creates HTTP 200 OK response
  - **200 OK**: Success status code
  - **`trips`**: The data to return
  - Automatically serializes to JSON
- **`return`**: Sends response to client

**What happens:**
1. Client sends: `GET /api/trips`
2. Framework routes to: `TripsController.GetActiveTripsAsync()`
3. Method calls: `_tripRepository.GetActiveTripsAsync()`
4. Repository returns: List of trips
5. Method returns: `Ok(trips)`
6. Framework serializes: Trips to JSON
7. Client receives: `200 OK` with JSON data

**Example Response:**
```json
HTTP/1.1 200 OK
Content-Type: application/json

[
  {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "riderId": "660e8400-e29b-41d4-a716-446655440001",
    "driverId": "770e8400-e29b-41d4-a716-446655440002",
    "start": {
      "latitude": -1.9441,
      "longitude": 30.0619
    },
    "end": {
      "latitude": -1.9706,
      "longitude": 30.1044
    },
    "fare": 3000.0,
    "requestTime": "2025-01-15T10:30:00Z"
  }
]
```

---

## Step 7: Implementing POST Endpoint

### What is a POST Request?
**POST** is an HTTP method used to **create** new resources. It's like placing an order: "I want to create a new trip."

**Characteristics:**
- **Not safe**: Changes data (creates something new)
- **Not idempotent**: Calling twice creates two resources
- **Has a body**: Sends data in the request body (usually JSON)

### Creating the TripRequest Model

First, we need a model to receive the request data:

```csharp
using SafeBoda.Core;

namespace SafeBoda.Api.Models;

public record TripRequest(Guid RiderId, Location Start, Location End);
```

**Explanation:**
- **`TripRequest`**: A record to hold request data
- **`Guid RiderId`**: Which rider is requesting the trip
- **`Location Start`**: Where the trip starts
- **`Location End`**: Where the trip should end
- **Why not include everything?**
  - Client doesn't know the `Id` (we generate it)
  - Client doesn't know the `DriverId` (we assign it)
  - Client doesn't set `Fare` (we calculate it)
  - Client doesn't set `RequestTime` (we use current time)

### The POST Action Method

```csharp
[HttpPost("request")]
public async Task<ActionResult<Trip>> CreateTripAsync([FromBody] TripRequest request)
{
    var newTrip = new Trip(
        Id: Guid.NewGuid(),
        RiderId: request.RiderId,
        DriverId: Guid.NewGuid(), // In real app, assign available driver
        Start: request.Start,
        End: request.End,
        Fare: CalculateFare(request.Start, request.End),
        RequestTime: DateTime.UtcNow
    );

    var createdTrip = await _tripRepository.CreateTripAsync(newTrip);
    
    return CreatedAtAction("GetTripById", new { id = createdTrip.Id }, createdTrip);
}
```

**Line-by-line explanation:**

**Line 1: HttpPost Attribute**
```csharp
[HttpPost("request")]
```
- **`[HttpPost]`**: Handles POST requests
- **`"request"`**: Additional route segment
- Final route: `POST /api/trips/request`
- Could also be `[HttpPost]` alone → `POST /api/trips`

**Line 2: Method Signature**
```csharp
public async Task<ActionResult<Trip>> CreateTripAsync([FromBody] TripRequest request)
```
- **`async Task<ActionResult<Trip>>`**: Returns a Trip on success
- **`[FromBody]`**: Attribute telling framework to read from request body
  - Framework will deserialize JSON to `TripRequest`
  - Example: `{ "riderId": "...", "start": {...}, "end": {...} }` → `TripRequest` object
- **`TripRequest request`**: The deserialized request data

**Line 4-11: Creating the Trip**
```csharp
var newTrip = new Trip(
    Id: Guid.NewGuid(),
    RiderId: request.RiderId,
    DriverId: Guid.NewGuid(),
    Start: request.Start,
    End: request.End,
    Fare: CalculateFare(request.Start, request.End),
    RequestTime: DateTime.UtcNow
);
```
- **`new Trip(...)`**: Creates a new Trip record
- **`Id: Guid.NewGuid()`**: Generate unique ID
- **`RiderId: request.RiderId`**: From the request
- **`DriverId: Guid.NewGuid()`**: 
  - In real app, we'd find an available driver
  - For now, just generate a GUID
- **`Start: request.Start`**: From request
- **`End: request.End`**: From request
- **`Fare: CalculateFare(...)`**: Calculate based on distance
  - Helper method (we'll see it below)
- **`RequestTime: DateTime.UtcNow`**: Current time in UTC
  - UTC = Coordinated Universal Time (timezone-independent)

**Line 13: Saving the Trip**
```csharp
var createdTrip = await _tripRepository.CreateTripAsync(newTrip);
```
- Calls repository to save the trip
- `await` waits for it to complete
- Returns the created trip (might have additional fields set by repository)

**Line 15: Returning Response**
```csharp
return CreatedAtAction("GetTripById", new { id = createdTrip.Id }, createdTrip);
```
- **`CreatedAtAction(...)`**: Creates HTTP 201 Created response
  - **201 Created**: Success, resource was created
  - Includes `Location` header pointing to the new resource
- **`"GetTripById"`**: Name of action that can retrieve this trip
  - Framework generates URL: `/api/trips/{id}`
- **`new { id = createdTrip.Id }`**: Route parameters
  - Anonymous object with route values
- **`createdTrip`**: The created trip (returned in response body)

**Example Request:**
```http
POST /api/trips/request HTTP/1.1
Content-Type: application/json

{
  "riderId": "550e8400-e29b-41d4-a716-446655440000",
  "start": {
    "latitude": -1.9441,
    "longitude": 30.0619
  },
  "end": {
    "latitude": -1.9706,
    "longitude": 30.1044
  }
}
```

**Example Response:**
```http
HTTP/1.1 201 Created
Location: /api/trips/660e8400-e29b-41d4-a716-446655440001
Content-Type: application/json

{
  "id": "660e8400-e29b-41d4-a716-446655440001",
  "riderId": "550e8400-e29b-41d4-a716-446655440000",
  "driverId": "770e8400-e29b-41d4-a716-446655440002",
  "start": { "latitude": -1.9441, "longitude": 30.0619 },
  "end": { "latitude": -1.9706, "longitude": 30.1044 },
  "fare": 3500.0,
  "requestTime": "2025-01-15T10:45:00Z"
}
```

### Helper Method: CalculateFare

```csharp
private static decimal CalculateFare(Location start, Location end)
{
    var distance = Math.Sqrt(
        Math.Pow(end.Latitude - start.Latitude, 2) +
        Math.Pow(end.Longitude - start.Longitude, 2)
    );

    var baseFare = 1000m;
    var farePerUnit = 5000m;

    return baseFare + (decimal)(distance * (double)farePerUnit);
}
```

**Explanation:**
- **`private static`**: 
  - `private`: Only used in this class
  - `static`: Doesn't need an instance (no `this`)
- **`decimal`**: Return type (money should use decimal for precision)
- **Distance calculation**: 
  - Uses Euclidean distance formula: √((x₂-x₁)² + (y₂-y₁)²)
  - Simplified (not real GPS distance, but good enough for example)
- **Fare calculation**:
  - Base fare: 1000 (minimum charge)
  - Per unit: 5000 (per distance unit)
  - Total: base + (distance × perUnit)

**Note**: In a real app, you'd use proper geolocation distance calculation (Haversine formula) and more sophisticated pricing.

---

## Step 8: Configuring Swagger

### What is Swagger?
**Swagger** (also called OpenAPI) is a tool that:
- **Documents** your API automatically
- **Tests** your API interactively
- **Generates** client code for other languages
- Provides a **web UI** to explore endpoints

Think of it as an **interactive API manual** that's always up-to-date!

### Why Use Swagger?
- **Documentation**: Always matches your code
- **Testing**: Test endpoints without writing code
- **Discovery**: See what endpoints are available
- **Validation**: Check if requests/responses are correct

### Adding Swagger to Program.cs

```csharp
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
```

**Line 1: AddEndpointsApiExplorer**
```csharp
builder.Services.AddEndpointsApiExplorer();
```
- Enables API exploration
- Scans controllers and actions
- Discovers routes, parameters, return types
- Required for Swagger to work

**Line 2: AddSwaggerGen**
```csharp
builder.Services.AddSwaggerGen();
```
- Registers Swagger generator
- Creates OpenAPI specification (JSON document describing API)
- Configures Swagger UI

### Configuring Swagger UI

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SafeBoda API V1");
    });
}
```

**Line 1: Environment Check**
```csharp
if (app.Environment.IsDevelopment())
```
- Only enable Swagger in development
- In production, you might want to disable it (security)
- `IsDevelopment()` checks the environment

**Line 3: UseSwagger**
```csharp
app.UseSwagger();
```
- Enables Swagger middleware
- Serves the OpenAPI JSON document
- Available at: `/swagger/v1/swagger.json`

**Line 4-7: UseSwaggerUI**
```csharp
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SafeBoda API V1");
});
```
- Enables Swagger UI (the web interface)
- **`SwaggerEndpoint`**: Points to the JSON document
- **`"SafeBoda API V1"`**: Title shown in UI
- Available at: `/swagger` (usually)

### Adding Swagger Package

You need to add the Swagger package to your project:

```bash
dotnet add SafeBoda.Api/SafeBoda.Api.csproj package Swashbuckle.AspNetCore
```

**What this does:**
- Adds `Swashbuckle.AspNetCore` NuGet package
- Provides Swagger/OpenAPI functionality
- Includes Swagger UI

### Accessing Swagger UI

When you run the API:
1. Navigate to: `https://localhost:7244/swagger` (or your port)
2. You'll see a web page with:
   - List of all endpoints
   - Try it out buttons
   - Request/response examples
   - Schema definitions

**Swagger UI Features:**
- **Try it out**: Click to test endpoints
- **Execute**: Send real requests
- **See responses**: View actual API responses
- **Schema**: See data models
- **Parameters**: See what each endpoint expects

---

## Step 9: Testing the API

### Running the API

**Option 1: Command Line**
```bash
cd SafeBoda.Api
dotnet run
```

**Option 2: IDE**
- Press F5 in Visual Studio/Rider
- Or click "Run" button

**What happens:**
- API starts on a port (e.g., `https://localhost:7244`)
- You'll see output like:
  ```
  Now listening on: https://localhost:7244
  Now listening on: http://localhost:5103
  ```

### Testing with Swagger UI

1. **Open Swagger**: Navigate to `https://localhost:7244/swagger`
2. **Find GET endpoint**: Look for `GET /api/trips`
3. **Click "Try it out"**
4. **Click "Execute"**
5. **See response**: You should see a list of trips

**Testing POST:**
1. Find `POST /api/trips/request`
2. Click "Try it out"
3. Fill in the request body:
   ```json
   {
     "riderId": "550e8400-e29b-41d4-a716-446655440000",
     "start": {
       "latitude": -1.9441,
       "longitude": 30.0619
     },
     "end": {
       "latitude": -1.9706,
       "longitude": 30.1044
     }
   }
   ```
4. Click "Execute"
5. See the created trip in the response

### Testing with curl (Command Line)

**GET Request:**
```bash
curl https://localhost:7244/api/trips
```

**POST Request:**
```bash
curl -X POST https://localhost:7244/api/trips/request \
  -H "Content-Type: application/json" \
  -d '{
    "riderId": "550e8400-e29b-41d4-a716-446655440000",
    "start": { "latitude": -1.9441, "longitude": 30.0619 },
    "end": { "latitude": -1.9706, "longitude": 30.1044 }
  }'
```

### Testing with Postman

1. **Create new request**
2. **Set method**: GET or POST
3. **Set URL**: `https://localhost:7244/api/trips`
4. **For POST**: Add JSON body
5. **Send request**
6. **View response**

### Common Issues

**Issue: "This site can't be reached"**
- Check if API is running
- Check the port number
- Try HTTP instead of HTTPS

**Issue: "SSL certificate error"**
- Development certificates might not be trusted
- Click "Advanced" → "Proceed anyway" (development only)

**Issue: "404 Not Found"**
- Check the route (should be `/api/trips`)
- Check if controller is registered
- Check if `MapControllers()` is called

**Issue: "500 Internal Server Error"**
- Check the console output for errors
- Verify repository is registered
- Check if all dependencies are available

---

## Understanding HTTP Methods

### Common HTTP Methods

| Method | Purpose | Idempotent? | Safe? | Has Body? |
|--------|---------|-------------|-------|-----------|
| **GET** | Retrieve data | Yes | Yes | No |
| **POST** | Create new resource | No | No | Yes |
| **PUT** | Update/replace resource | Yes | No | Yes |
| **PATCH** | Partial update | No | No | Yes |
| **DELETE** | Delete resource | Yes | No | No |

**Idempotent**: Calling multiple times has the same effect as calling once
**Safe**: Doesn't change server state

### HTTP Status Codes

| Code | Meaning | When to Use |
|------|---------|-------------|
| **200 OK** | Success | GET, PUT, PATCH successful |
| **201 Created** | Resource created | POST successful |
| **204 No Content** | Success, no body | DELETE successful |
| **400 Bad Request** | Invalid request | Validation failed |
| **401 Unauthorized** | Not authenticated | Missing/invalid token |
| **403 Forbidden** | Not authorized | Authenticated but no permission |
| **404 Not Found** | Resource not found | ID doesn't exist |
| **500 Internal Server Error** | Server error | Exception occurred |

---

## Understanding Routing

### How Routing Works

**Route Template**: `api/[controller]`
- **`api/`**: Literal segment (always "api")
- **`[controller]`**: Token replaced with controller name (without "Controller" suffix)

**Action Route**: `[HttpGet]` or `[HttpPost("request")]`
- If empty: Uses controller route
- If specified: Appended to controller route

### Route Examples

| Controller | Base Route | Action | Method | Final Route |
|------------|------------|--------|--------|-------------|
| `TripsController` | `api/trips` | `[HttpGet]` | GET | `GET /api/trips` |
| `TripsController` | `api/trips` | `[HttpPost("request")]` | POST | `POST /api/trips/request` |
| `TripsController` | `api/trips` | `[HttpGet("{id}")]` | GET | `GET /api/trips/{id}` |
| `UsersController` | `api/users` | `[HttpGet]` | GET | `GET /api/users` |

### Route Parameters

```csharp
[HttpGet("{id:guid}")]
public async Task<ActionResult<Trip>> GetTripByIdAsync(Guid id)
```

**Explanation:**
- **`"{id:guid}"`**: Route parameter
  - `{id}`: Parameter name
  - `:guid`: Constraint (must be a GUID)
- **`Guid id`**: Method parameter (automatically bound from route)
- Example: `GET /api/trips/550e8400-...` → `id = 550e8400-...`

---

## Summary

In Step 2, we:

1. ✅ Created `SafeBoda.Api` Web API project
2. ✅ Added references to Core and Application projects
3. ✅ Registered `ITripRepository` with dependency injection
4. ✅ Created `TripsController` with dependency injection
5. ✅ Implemented `GET /api/trips` endpoint
6. ✅ Implemented `POST /api/trips/request` endpoint
7. ✅ Configured Swagger for API documentation
8. ✅ Tested the API using Swagger UI

**Key Concepts Learned:**
- **API**: Application Programming Interface (how clients interact with server)
- **Controller**: Handles HTTP requests and returns responses
- **Dependency Injection**: Framework provides dependencies automatically
- **Routing**: Maps URLs to controller actions
- **HTTP Methods**: GET (read), POST (create), etc.
- **Swagger**: Interactive API documentation
- **Async/Await**: Non-blocking asynchronous operations

**What We Built:**
- A working REST API
- Endpoints to get and create trips
- Interactive documentation
- Foundation for future endpoints

**What's Next?**
In future steps, we'll:
- Add more endpoints (PUT, DELETE)
- Connect to a real database
- Add authentication and authorization
- Add validation and error handling
- Deploy to the cloud

---

## Common Questions

**Q: Why use async/await?**
A: Makes the API more efficient. While waiting for database/network operations, the thread can handle other requests. Better scalability.

**Q: What's the difference between `ActionResult<T>` and just returning `T`?**
A: `ActionResult<T>` allows returning different HTTP status codes (`Ok()`, `NotFound()`, etc.). Just `T` always returns 200 OK.

**Q: Why use `[FromBody]`?**
A: Tells the framework to read from the request body (JSON). Without it, it might try to read from query string or route.

**Q: What is model binding?**
A: Automatic conversion of HTTP request data (JSON, form data, etc.) into C# objects. Framework does it automatically.

**Q: Why use Swagger in development only?**
A: Security and performance. In production, you might not want to expose API structure. But you can enable it if needed.

**Q: What's the difference between `Guid.NewGuid()` and a fixed ID?**
A: `Guid.NewGuid()` generates a unique ID each time. Fixed IDs would cause conflicts (can't have two trips with same ID).

**Q: Why calculate fare in the controller?**
A: In a real app, this would be in a service class. But for simplicity, we put it in the controller. Later, we'll refactor to follow better patterns.

---

## Conclusion

Congratulations! You've built a working REST API! 🎉

You now have:
- A Web API that can receive HTTP requests
- Endpoints to retrieve and create trips
- Interactive documentation with Swagger
- A foundation for building more features

The API is now accessible to:
- Mobile applications
- Web browsers
- Other services
- Testing tools

**Remember**: APIs are the bridge between your application and the outside world. A well-designed API makes your application powerful and easy to use!

**Next Steps**: Continue building more endpoints, add validation, connect to a database, and add security features. The journey to a production-ready API continues! 🚀

