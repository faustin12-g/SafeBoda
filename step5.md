# SafeBoda Project - Step 5: Building the Admin Portal
## Comprehensive Line-by-Line Explanation

---

## Table of Contents
1. [Introduction](#introduction)
2. [What is Blazor?](#what-is-blazor)
3. [What is Blazor WebAssembly?](#what-is-blazor-webassembly)
4. [Step 1: Create a New Blazor Project](#step-1-create-a-new-blazor-project)
5. [Step 2: Configure CORS](#step-2-configure-cors)
6. [Step 3: Create a Typed HttpClient Service](#step-3-create-a-typed-httpclient-service)
7. [Step 4: Implement Authentication Service](#step-4-implement-authentication-service)
8. [Step 5: Build the Login Page](#step-5-build-the-login-page)
9. [Step 6: Create the Dashboard/Trips Page](#step-6-create-the-dashboardtrips-page)
10. [Step 7: Fetch and Display Active Trips](#step-7-fetch-and-display-active-trips)
11. [Step 8: Implement Logout Functionality](#step-8-implement-logout-functionality)
12. [Understanding Blazor Components](#understanding-blazor-components)
13. [Understanding Routing](#understanding-routing)
14. [Understanding State Management](#understanding-state-management)
15. [Summary](#summary)

---

## Introduction

Welcome to Step 5! In this module, we're building the **frontend** of our SafeBoda application - a beautiful, interactive admin portal that allows administrators to manage the platform.

**What we'll build:**
- A Blazor WebAssembly application
- Login page for authentication
- Dashboard with statistics
- Trips management page
- Real-time data display
- Secure API communication

**The transformation:**
- **Before**: Only API endpoints (no user interface)
- **After**: Beautiful admin portal with full UI

**Why Blazor?**
- Write C# instead of JavaScript
- Share code between frontend and backend
- Strong typing and IntelliSense
- Modern, component-based architecture

---

## What is Blazor?

### The Problem: Frontend Development

**Traditional web development:**
- **Backend**: C# (ASP.NET Core)
- **Frontend**: JavaScript (React, Angular, Vue)
- **Problem**: Two different languages, two different ecosystems

**Blazor solution:**
- **Backend**: C# (ASP.NET Core)
- **Frontend**: C# (Blazor)
- **Benefit**: One language for everything!

### What is Blazor?

**Blazor** is a web framework that lets you build interactive web UIs using **C# instead of JavaScript**.

**Key features:**
- Write C# code in the browser
- Component-based architecture
- Two hosting models:
  - **Blazor Server**: Runs on server, updates via SignalR
  - **Blazor WebAssembly**: Runs in browser, like JavaScript

**We're using Blazor WebAssembly** - runs entirely in the browser!

### How Blazor Works

```
1. Developer writes C# code
   ↓
2. .NET compiles to WebAssembly
   ↓
3. Browser downloads WebAssembly
   ↓
4. Browser runs WebAssembly (like JavaScript)
   ↓
5. UI updates automatically
```

**WebAssembly (WASM):**
- Low-level binary format
- Runs in browser (like JavaScript)
- Fast execution
- Supported by all modern browsers

---

## What is Blazor WebAssembly?

### Blazor Server vs WebAssembly

| Feature | Blazor Server | Blazor WebAssembly |
|---------|---------------|-------------------|
| **Where code runs** | Server | Browser |
| **Network required** | Always | Only for API calls |
| **Initial load** | Fast | Slower (downloads .NET) |
| **Scalability** | Server resources | Client resources |
| **Offline capable** | No | Yes (after initial load) |

**We chose WebAssembly** because:
- Works offline (after initial load)
- Scales better (runs on client)
- Better for admin portals (always connected anyway)

### Blazor WebAssembly Architecture

```
Browser
├── Blazor WebAssembly App (C#)
│   ├── Components (.razor files)
│   ├── Services (API calls)
│   └── Models (data structures)
│
└── API (ASP.NET Core)
    ├── Controllers
    ├── Database
    └── Authentication
```

**Communication:**
- Blazor app makes HTTP requests to API
- API returns JSON data
- Blazor renders UI with data

---

## Step 1: Create a New Blazor Project

### The Command

```bash
dotnet new blazorwasm -n SafeBoda.Admin
```

**Breaking it down:**
- **`dotnet new`**: Create new project
- **`blazorwasm`**: Blazor WebAssembly template
- **`-n SafeBoda.Admin`**: Project name

### What This Creates

```
SafeBoda.Admin/
├── Pages/              (Razor components/pages)
├── Shared/             (Shared components)
├── wwwroot/            (Static files: CSS, JS, images)
├── Program.cs           (Application entry point)
├── App.razor           (Root component)
└── SafeBoda.Admin.csproj
```

### Adding to Solution

```bash
dotnet sln add SafeBoda.Admin/SafeBoda.Admin.csproj
```

### Understanding the Project File

**`SafeBoda.Admin.csproj`:**

```xml
<Project Sdk="Microsoft.NET.Sdk.BlazorWebAssembly">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly" Version="9.0.4" />
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly.DevServer" Version="9.0.4" />
  </ItemGroup>
</Project>
```

**Key differences:**
- **`Sdk="Microsoft.NET.Sdk.BlazorWebAssembly"`**: Blazor WebAssembly SDK
  - Includes WebAssembly runtime
  - Includes Blazor components
  - Includes HTTP client

**Packages:**
- **`WebAssembly`**: Core Blazor WebAssembly functionality
- **`WebAssembly.DevServer`**: Development server

### Understanding Program.cs

**`SafeBoda.Admin/Program.cs`:**

```csharp
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SafeBoda.Admin;
using SafeBoda.Admin.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HttpClient
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri("http://localhost:5103/") 
});

// Register services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ApiClient>();

await builder.Build().RunAsync();
```

**Line-by-line explanation:**

**Line 1-4: Using Statements**
```csharp
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SafeBoda.Admin;
using SafeBoda.Admin.Services;
```
- **`Microsoft.AspNetCore.Components.Web`**: Web components
- **`Microsoft.AspNetCore.Components.WebAssembly.Hosting`**: WASM hosting
- **`SafeBoda.Admin`**: Our app namespace
- **`SafeBoda.Admin.Services`**: Our services

**Line 6: Create Builder**
```csharp
var builder = WebAssemblyHostBuilder.CreateDefault(args);
```
- **`WebAssemblyHostBuilder`**: Builder for Blazor WebAssembly app
- **`CreateDefault(args)`**: Creates builder with default configuration
- Similar to `WebApplication.CreateBuilder()` in API

**Line 7: Root Component**
```csharp
builder.RootComponents.Add<App>("#app");
```
- **`<App>`**: Our root component (App.razor)
- **`"#app"`**: CSS selector for HTML element
  - Looks for `<div id="app">` in index.html
  - Blazor app renders inside this element

**Line 8: Head Outlet**
```csharp
builder.RootComponents.Add<HeadOutlet>("head::after");
```
- Allows components to modify `<head>` tag
- Useful for page titles, meta tags

**Line 11-14: Configure HttpClient**
```csharp
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri("http://localhost:5103/") 
});
```
- **`AddScoped`**: One HttpClient per component scope
- **`BaseAddress`**: API base URL
  - All API calls will be relative to this
  - Example: `api/trips` → `http://localhost:5103/api/trips`

**Line 17-18: Register Services**
```csharp
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ApiClient>();
```
- Registers our custom services
- **Scoped**: One instance per component scope
- Available via dependency injection

**Line 20: Build and Run**
```csharp
await builder.Build().RunAsync();
```
- Builds the app
- Runs in browser
- Blocks until app is closed

---

## Step 2: Configure CORS

### What is CORS?

**CORS** = **Cross-Origin Resource Sharing**

**The problem:**
- Blazor app runs on: `http://localhost:5086`
- API runs on: `http://localhost:5103`
- **Different origins** = browser blocks requests!

**CORS solution:**
- API tells browser: "Allow requests from Blazor app"
- Browser allows the request

### Why Do We Need CORS?

**Same-Origin Policy:**
- Browser security feature
- Blocks requests to different origins
- **Origin** = protocol + domain + port

**Example:**
- `http://localhost:5086` ≠ `http://localhost:5103`
- Different ports = different origins
- Browser blocks by default

**CORS allows cross-origin requests** (with permission)

### Configuring CORS in API

**Add to `SafeBoda.Api/Program.cs`:**

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorAdmin", policy =>
    {
        policy.WithOrigins("https://localhost:7291", "http://localhost:5086")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
```

**Line-by-line explanation:**

**Line 1: AddCors**
```csharp
builder.Services.AddCors(options =>
```
- Registers CORS services
- `options` = CORS configuration builder

**Line 3: Add Policy**
```csharp
options.AddPolicy("AllowBlazorAdmin", policy =>
```
- **`"AllowBlazorAdmin"`**: Policy name
- **`policy`**: Policy configuration

**Line 5: Allowed Origins**
```csharp
policy.WithOrigins("https://localhost:7291", "http://localhost:5086")
```
- **`WithOrigins`**: Which origins are allowed
- **HTTPS**: `https://localhost:7291` (production-like)
- **HTTP**: `http://localhost:5086` (development)
- Only these origins can make requests

**Line 6: Allow Any Header**
```csharp
.AllowAnyHeader()
```
- Allows any HTTP headers
- Needed for `Authorization: Bearer token`

**Line 7: Allow Any Method**
```csharp
.AllowAnyMethod()
```
- Allows GET, POST, PUT, DELETE, etc.
- All HTTP methods allowed

**Line 8: Allow Credentials**
```csharp
.AllowCredentials()
```
- Allows cookies, authentication headers
- Needed for JWT tokens

### Using CORS Middleware

**Add to `Program.cs` (after `app = builder.Build()`):**

```csharp
app.UseCors("AllowBlazorAdmin");
```

**Order matters:**
```csharp
app.UseCors("AllowBlazorAdmin");  // Must come before UseAuthentication
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

**What this does:**
- Applies CORS policy to all requests
- Adds CORS headers to responses
- Allows Blazor app to make requests

### Testing CORS

**Without CORS:**
```
Browser: Request to API
API: No CORS headers
Browser: Blocks request
```

**With CORS:**
```
Browser: Request to API
API: Adds CORS headers
Browser: Allows request
```

---

## Step 3: Create a Typed HttpClient Service

### What is a Typed HttpClient?

**Typed HttpClient** = A service class that wraps `HttpClient` with specific methods for your API.

**Benefits:**
- Type-safe API calls
- Centralized API logic
- Easy to test
- Reusable across components

### Creating ApiClient Service

**Create `SafeBoda.Admin/Services/ApiClient.cs`:**

```csharp
using SafeBoda.Admin.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace SafeBoda.Admin.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly AuthService _authService;

    public ApiClient(HttpClient httpClient, AuthService authService)
    {
        _httpClient = httpClient;
        _authService = authService;
    }

    private async Task SetAuthHeaderAsync()
    {
        var token = await _authService.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<TripsResponse?> GetTripsAsync()
    {
        await SetAuthHeaderAsync();
        try
        {
            return await _httpClient.GetFromJsonAsync<TripsResponse>("api/trips");
        }
        catch
        {
            return null;
        }
    }
}
```

**Line-by-line explanation:**

**Line 1-3: Using Statements**
```csharp
using SafeBoda.Admin.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
```
- **`SafeBoda.Admin.Models`**: Our data models
- **`System.Net.Http.Headers`**: For `AuthenticationHeaderValue`
- **`System.Net.Http.Json`**: For `GetFromJsonAsync`, `PostAsJsonAsync`

**Line 7: Class Declaration**
```csharp
public class ApiClient
```
- Service class for API communication

**Line 9-10: Dependencies**
```csharp
private readonly HttpClient _httpClient;
private readonly AuthService _authService;
```
- **`HttpClient`**: For making HTTP requests
- **`AuthService`**: For getting JWT token

**Line 12-16: Constructor**
```csharp
public ApiClient(HttpClient httpClient, AuthService authService)
{
    _httpClient = httpClient;
    _authService = authService;
}
```
- **Dependency Injection**: Framework provides `HttpClient` and `AuthService`
- Stored in private fields

**Line 18-25: SetAuthHeaderAsync**
```csharp
private async Task SetAuthHeaderAsync()
{
    var token = await _authService.GetTokenAsync();
    if (!string.IsNullOrEmpty(token))
    {
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
    }
}
```
- **Purpose**: Adds JWT token to all requests
- **`GetTokenAsync()`**: Gets token from storage
- **`AuthenticationHeaderValue("Bearer", token)`**: Creates `Authorization: Bearer token` header
- **`DefaultRequestHeaders`**: Headers added to all requests

**Line 27-36: GetTripsAsync**
```csharp
public async Task<TripsResponse?> GetTripsAsync()
{
    await SetAuthHeaderAsync();
    try
    {
        return await _httpClient.GetFromJsonAsync<TripsResponse>("api/trips");
    }
    catch
    {
        return null;
    }
}
```
- **`SetAuthHeaderAsync()`**: Adds token first
- **`GetFromJsonAsync<T>`**: GET request + JSON deserialization
  - URL: `api/trips` (relative to BaseAddress)
  - Deserializes JSON to `TripsResponse`
- **`try-catch`**: Returns `null` on error (network, 401, etc.)

### More ApiClient Methods

**GetDriversAsync:**
```csharp
public async Task<List<Driver>> GetDriversAsync()
{
    await SetAuthHeaderAsync();
    try
    {
        var drivers = await _httpClient.GetFromJsonAsync<List<Driver>>("api/admin/drivers");
        return drivers ?? new List<Driver>();
    }
    catch
    {
        return new List<Driver>();
    }
}
```

**CreateTripAsync:**
```csharp
public async Task<Trip?> CreateTripAsync(Trip trip)
{
    await SetAuthHeaderAsync();
    try
    {
        var response = await _httpClient.PostAsJsonAsync("api/trips", trip);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<Trip>();
        }
        return null;
    }
    catch
    {
        return null;
    }
}
```
- **`PostAsJsonAsync`**: POST request with JSON body
- **`ReadFromJsonAsync`**: Deserializes response

**DeleteTripAsync:**
```csharp
public async Task<bool> DeleteTripAsync(Guid tripId)
{
    await SetAuthHeaderAsync();
    try
    {
        var response = await _httpClient.DeleteAsync($"api/trips/{tripId}");
        return response.IsSuccessStatusCode;
    }
    catch
    {
        return false;
    }
}
```
- **`DeleteAsync`**: DELETE request
- Returns `true` if successful

### Registering ApiClient

**Already done in `Program.cs`:**
```csharp
builder.Services.AddScoped<ApiClient>();
```

**Why Scoped?**
- One instance per component scope
- Shares `HttpClient` instance
- Efficient and safe

---

## Step 4: Implement Authentication Service

### What is AuthService?

**AuthService** manages:
- User login
- Token storage (localStorage)
- Authentication state
- Logout

### Creating AuthService

**Create `SafeBoda.Admin/Services/AuthService.cs`:**

```csharp
using Microsoft.JSInterop;
using SafeBoda.Admin.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace SafeBoda.Admin.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly IJSRuntime _jsRuntime;
    private const string TokenKey = "authToken";
    private const string UserKey = "currentUser";

    public event Action? OnAuthStateChanged;

    public AuthService(HttpClient httpClient, IJSRuntime jsRuntime)
    {
        _httpClient = httpClient;
        _jsRuntime = jsRuntime;
    }
}
```

**Line-by-line explanation:**

**Line 1-4: Using Statements**
```csharp
using Microsoft.JSInterop;
using SafeBoda.Admin.Models;
using System.Net.Http.Json;
using System.Text.Json;
```
- **`Microsoft.JSInterop`**: JavaScript interop (call JS from C#)
- **`SafeBoda.Admin.Models`**: Login models
- **`System.Net.Http.Json`**: HTTP JSON helpers
- **`System.Text.Json`**: JSON serialization

**Line 8: Class Declaration**
```csharp
public class AuthService
```

**Line 10-11: Dependencies**
```csharp
private readonly HttpClient _httpClient;
private readonly IJSRuntime _jsRuntime;
```
- **`HttpClient`**: For API calls
- **`IJSRuntime`**: For JavaScript interop (localStorage)

**Line 12-13: Constants**
```csharp
private const string TokenKey = "authToken";
private const string UserKey = "currentUser";
```
- Keys for localStorage
- **localStorage**: Browser storage (persists across sessions)

**Line 15: Event**
```csharp
public event Action? OnAuthStateChanged;
```
- **Event**: Notifies when auth state changes
- Components can subscribe to know when user logs in/out

**Line 17-21: Constructor**
```csharp
public AuthService(HttpClient httpClient, IJSRuntime jsRuntime)
{
    _httpClient = httpClient;
    _jsRuntime = jsRuntime;
}
```
- Dependency injection

### Implementing LoginAsync

```csharp
public async Task<(bool Success, string Message)> LoginAsync(LoginRequest request)
{
    try
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);
        
        if (response.IsSuccessStatusCode)
        {
            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (loginResponse != null)
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TokenKey, loginResponse.Token);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", UserKey, JsonSerializer.Serialize(loginResponse));
                OnAuthStateChanged?.Invoke();
                return (true, "Login successful");
            }
        }
        
        var error = await response.Content.ReadAsStringAsync();
        return (false, "Invalid email or password");
    }
    catch (Exception ex)
    {
        return (false, $"Connection error: {ex.Message}");
    }
}
```

**Line-by-line explanation:**

**Line 1: Method Signature**
```csharp
public async Task<(bool Success, string Message)> LoginAsync(LoginRequest request)
```
- **Returns**: Tuple `(bool, string)`
  - `Success`: Login succeeded?
  - `Message`: Success/error message

**Line 5: API Call**
```csharp
var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);
```
- POST request to login endpoint
- Sends `LoginRequest` (email, password)

**Line 7: Check Success**
```csharp
if (response.IsSuccessStatusCode)
```
- Status code 200-299 = success

**Line 9: Deserialize Response**
```csharp
var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
```
- Deserializes JSON to `LoginResponse`
- Contains: `Token`, `Email`, `FullName`, `Roles`

**Line 12: Store Token**
```csharp
await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TokenKey, loginResponse.Token);
```
- **`InvokeVoidAsync`**: Calls JavaScript function
- **`localStorage.setItem`**: Stores token in browser
- **Key**: `"authToken"`
- **Value**: JWT token string

**Line 13: Store User Info**
```csharp
await _jsRuntime.InvokeVoidAsync("localStorage.setItem", UserKey, JsonSerializer.Serialize(loginResponse));
```
- Stores user info (email, name, roles) as JSON string

**Line 14: Notify Listeners**
```csharp
OnAuthStateChanged?.Invoke();
```
- **`?.`**: Null-conditional operator
- Fires event if anyone is listening
- Components can react to login

**Line 15: Return Success**
```csharp
return (true, "Login successful");
```

**Line 20: Return Error**
```csharp
return (false, "Invalid email or password");
```

### Implementing LogoutAsync

```csharp
public async Task LogoutAsync()
{
    await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", TokenKey);
    await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", UserKey);
    OnAuthStateChanged?.Invoke();
}
```

**Explanation:**
- **`localStorage.removeItem`**: Removes token and user info
- **`OnAuthStateChanged?.Invoke()`**: Notifies listeners
- User is now logged out

### Implementing IsAuthenticatedAsync

```csharp
public async Task<bool> IsAuthenticatedAsync()
{
    var token = await GetTokenAsync();
    return !string.IsNullOrEmpty(token);
}
```

**Explanation:**
- Checks if token exists
- Returns `true` if authenticated

### Implementing GetTokenAsync

```csharp
public async Task<string?> GetTokenAsync()
{
    try
    {
        return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", TokenKey);
    }
    catch
    {
        return null;
    }
}
```

**Explanation:**
- **`localStorage.getItem`**: Gets token from storage
- Returns `null` if not found or error

### Implementing GetCurrentUserAsync

```csharp
public async Task<LoginResponse?> GetCurrentUserAsync()
{
    try
    {
        var userJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", UserKey);
        if (!string.IsNullOrEmpty(userJson))
        {
            return JsonSerializer.Deserialize<LoginResponse>(userJson, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });
        }
    }
    catch { }
    return null;
}
```

**Explanation:**
- Gets user JSON from localStorage
- Deserializes to `LoginResponse`
- **`PropertyNameCaseInsensitive`**: Handles case differences

### Registering AuthService

**Already done in `Program.cs`:**
```csharp
builder.Services.AddScoped<AuthService>();
```

---

## Step 5: Build the Login Page

### What is a Razor Component?

**Razor Component** = `.razor` file that contains:
- **HTML markup** (with C# expressions)
- **C# code** (`@code` block)
- **Component logic**

**Example structure:**
```razor
@page "/login"
@inject AuthService AuthService

<h1>Login</h1>
<button @onclick="HandleLogin">Login</button>

@code {
    private async Task HandleLogin() { }
}
```

### Creating Login Page

**Create `SafeBoda.Admin/Pages/Login.razor`:**

```razor
@page "/login"
@inject SafeBoda.Admin.Services.AuthService AuthService
@inject NavigationManager Navigation

<div class="login-container">
    <div class="login-form">
        <h3>Welcome Back</h3>
        
        @if (!string.IsNullOrEmpty(errorMessage))
        {
            <div class="alert alert-danger">@errorMessage</div>
        }

        <EditForm Model="@loginModel" OnValidSubmit="HandleLogin">
            <DataAnnotationsValidator />
            
            <div class="mb-3">
                <label class="form-label">Email Address</label>
                <InputText @bind-Value="loginModel.Email" class="form-control" />
                <ValidationMessage For="@(() => loginModel.Email)" />
            </div>

            <div class="mb-3">
                <label class="form-label">Password</label>
                <InputText @bind-Value="loginModel.Password" type="password" class="form-control" />
                <ValidationMessage For="@(() => loginModel.Password)" />
            </div>

            <button type="submit" class="btn btn-primary w-100" disabled="@isLoading">
                @if (isLoading)
                {
                    <span class="spinner-border spinner-border-sm me-2"></span>
                    <span>Signing in...</span>
                }
                else
                {
                    <span>Sign In</span>
                }
            </button>
        </EditForm>
    </div>
</div>

@code {
    private LoginModel loginModel = new();
    private string? errorMessage;
    private bool isLoading = false;

    private class LoginModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    private async Task HandleLogin()
    {
        isLoading = true;
        errorMessage = null;

        var request = new SafeBoda.Admin.Models.LoginRequest
        {
            Email = loginModel.Email,
            Password = loginModel.Password
        };

        var (success, message) = await AuthService.LoginAsync(request);

        if (success)
        {
            Navigation.NavigateTo("/dashboard");
        }
        else
        {
            errorMessage = message;
        }

        isLoading = false;
    }
}
```

**Line-by-line explanation:**

**Line 1: Route**
```razor
@page "/login"
```
- **`@page`**: Defines route
- URL: `/login`
- Browser navigates to this when URL is `/login`

**Line 2-3: Inject Services**
```razor
@inject SafeBoda.Admin.Services.AuthService AuthService
@inject NavigationManager Navigation
```
- **`@inject`**: Dependency injection
- **`AuthService`**: For login
- **`NavigationManager`**: For navigation

**Line 5: Container**
```razor
<div class="login-container">
```
- HTML structure
- CSS classes for styling

**Line 21: Error Message**
```razor
@if (!string.IsNullOrEmpty(errorMessage))
{
    <div class="alert alert-danger">@errorMessage</div>
}
```
- **`@if`**: C# conditional rendering
- Shows error if `errorMessage` is not empty
- **`@errorMessage`**: C# expression in HTML

**Line 23: EditForm**
```razor
<EditForm Model="@loginModel" OnValidSubmit="HandleLogin">
```
- **`EditForm`**: Blazor form component
- **`Model`**: Data model for form
- **`OnValidSubmit`**: Called when form is valid and submitted

**Line 24: DataAnnotationsValidator**
```razor
<DataAnnotationsValidator />
```
- Enables validation (if using DataAnnotations)

**Line 27-31: Email Input**
```razor
<label class="form-label">Email Address</label>
<InputText @bind-Value="loginModel.Email" class="form-control" />
<ValidationMessage For="@(() => loginModel.Email)" />
```
- **`InputText`**: Blazor input component
- **`@bind-Value`**: Two-way binding
  - Changes to input update `loginModel.Email`
  - Changes to `loginModel.Email` update input
- **`ValidationMessage`**: Shows validation errors

**Line 33-37: Password Input**
```razor
<InputText @bind-Value="loginModel.Password" type="password" class="form-control" />
```
- **`type="password"`**: Hides input (shows dots)

**Line 39-48: Submit Button**
```razor
<button type="submit" class="btn btn-primary w-100" disabled="@isLoading">
    @if (isLoading)
    {
        <span class="spinner-border spinner-border-sm me-2"></span>
        <span>Signing in...</span>
    }
    else
    {
        <span>Sign In</span>
    }
</button>
```
- **`disabled="@isLoading"`**: Disables button while loading
- Shows spinner when `isLoading` is true

**Line 52-58: Code Block**
```razor
@code {
    private LoginModel loginModel = new();
    private string? errorMessage;
    private bool isLoading = false;
```
- **`@code`**: C# code block
- **Component state**: Variables that control UI

**Line 60-64: LoginModel Class**
```razor
private class LoginModel
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
```
- Model for form data

**Line 66: HandleLogin Method**
```razor
private async Task HandleLogin()
{
    isLoading = true;
    errorMessage = null;
```
- Called when form is submitted
- Sets loading state

**Line 68-72: Create Request**
```razor
var request = new SafeBoda.Admin.Models.LoginRequest
{
    Email = loginModel.Email,
    Password = loginModel.Password
};
```
- Creates API request object

**Line 74: Call AuthService**
```razor
var (success, message) = await AuthService.LoginAsync(request);
```
- Calls login API
- Gets result tuple

**Line 76-82: Handle Result**
```razor
if (success)
{
    Navigation.NavigateTo("/dashboard");
}
else
{
    errorMessage = message;
}
```
- **Success**: Navigate to dashboard
- **Failure**: Show error message

**Line 84: Reset Loading**
```razor
isLoading = false;
```

---

## Step 6: Create the Dashboard/Trips Page

### Creating Trips Page

**Create `SafeBoda.Admin/Pages/Trips.razor`:**

```razor
@page "/trips"
@using SafeBoda.Admin.Shared
@inject SafeBoda.Admin.Services.AuthService AuthService
@inject SafeBoda.Admin.Services.ApiClient ApiClient

<AdminLayout CurrentPage="trips">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
            <h4 class="fw-bold mb-1">Active Trips</h4>
            <p class="text-muted mb-0">Monitor and manage all active trips</p>
        </div>
        <button class="btn btn-primary" @onclick="LoadTrips">
            <i class="bi bi-arrow-clockwise me-2"></i>Refresh
        </button>
    </div>

    @if (isLoading)
    {
        <div class="text-center py-5">
            <div class="spinner-border text-success" role="status"></div>
            <p class="mt-3 text-muted">Loading trips...</p>
        </div>
    }
    else if (trips == null || !trips.Any())
    {
        <div class="card p-5 text-center">
            <h5 class="mt-3 text-muted">No Active Trips</h5>
            <p class="text-muted mb-0">There are currently no active trips.</p>
        </div>
    }
    else
    {
        <div class="card">
            <div class="table-responsive">
                <table class="table">
                    <thead>
                        <tr>
                            <th>Start Location</th>
                            <th>End Location</th>
                            <th>Fare</th>
                            <th>Request Time</th>
                        </tr>
                    </thead>
                    <tbody>
                        @foreach (var trip in trips)
                        {
                            <tr>
                                <td>@trip.Start.Latitude, @trip.Start.Longitude</td>
                                <td>@trip.End.Latitude, @trip.End.Longitude</td>
                                <td>@trip.Fare.ToString("N0") RWF</td>
                                <td>@trip.RequestTime.ToString("MMM dd, HH:mm")</td>
                            </tr>
                        }
                    </tbody>
                </table>
            </div>
        </div>
    }
</AdminLayout>

@code {
    private bool isLoading = true;
    private List<SafeBoda.Admin.Models.Trip> trips = new();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadTrips();
            StateHasChanged();
        }
    }

    private async Task LoadTrips()
    {
        isLoading = true;
        StateHasChanged();

        var response = await ApiClient.GetTripsAsync();
        trips = response?.Trips ?? new List<SafeBoda.Admin.Models.Trip>();
        
        isLoading = false;
        StateHasChanged();
    }
}
```

**Line-by-line explanation:**

**Line 1: Route**
```razor
@page "/trips"
```
- Route: `/trips`

**Line 2: Using**
```razor
@using SafeBoda.Admin.Shared
```
- Imports shared components (AdminLayout)

**Line 3-4: Inject Services**
```razor
@inject SafeBoda.Admin.Services.AuthService AuthService
@inject SafeBoda.Admin.Services.ApiClient ApiClient
```

**Line 6: AdminLayout**
```razor
<AdminLayout CurrentPage="trips">
```
- **`AdminLayout`**: Custom layout component
- Provides sidebar, navigation
- **`CurrentPage`**: Highlights current page in sidebar

**Line 8-14: Header**
```razor
<div class="d-flex justify-content-between align-items-center mb-4">
    <div>
        <h4 class="fw-bold mb-1">Active Trips</h4>
        <p class="text-muted mb-0">Monitor and manage all active trips</p>
    </div>
    <button class="btn btn-primary" @onclick="LoadTrips">
        <i class="bi bi-arrow-clockwise me-2"></i>Refresh
    </button>
</div>
```
- **`@onclick`**: Event handler
- Calls `LoadTrips()` when clicked

**Line 16-22: Loading State**
```razor
@if (isLoading)
{
    <div class="text-center py-5">
        <div class="spinner-border text-success" role="status"></div>
        <p class="mt-3 text-muted">Loading trips...</p>
    </div>
}
```
- Shows spinner while loading

**Line 23-29: Empty State**
```razor
else if (trips == null || !trips.Any())
{
    <div class="card p-5 text-center">
        <h5 class="mt-3 text-muted">No Active Trips</h5>
    </div>
}
```
- Shows message when no trips

**Line 30-52: Table**
```razor
else
{
    <div class="card">
        <table class="table">
            <thead>
                <tr>
                    <th>Start Location</th>
                    <th>End Location</th>
                    <th>Fare</th>
                    <th>Request Time</th>
                </tr>
            </thead>
            <tbody>
                @foreach (var trip in trips)
                {
                    <tr>
                        <td>@trip.Start.Latitude, @trip.Start.Longitude</td>
                        <td>@trip.End.Latitude, @trip.End.Longitude</td>
                        <td>@trip.Fare.ToString("N0") RWF</td>
                        <td>@trip.RequestTime.ToString("MMM dd, HH:mm")</td>
                    </tr>
                }
            </tbody>
        </table>
    </div>
}
```
- **`@foreach`**: Loops through trips
- **`@trip.Property`**: Displays trip data
- **`.ToString("N0")`**: Formats number with commas

**Line 55-58: Code Block**
```razor
@code {
    private bool isLoading = true;
    private List<SafeBoda.Admin.Models.Trip> trips = new();
```
- Component state

**Line 60-66: OnAfterRenderAsync**
```razor
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        await LoadTrips();
        StateHasChanged();
    }
}
```
- **Lifecycle method**: Called after component renders
- **`firstRender`**: True only on first render
- Loads trips when page first loads
- **`StateHasChanged()`**: Tells Blazor to re-render

**Line 68-77: LoadTrips**
```razor
private async Task LoadTrips()
{
    isLoading = true;
    StateHasChanged();

    var response = await ApiClient.GetTripsAsync();
    trips = response?.Trips ?? new List<SafeBoda.Admin.Models.Trip>();
    
    isLoading = false;
    StateHasChanged();
}
```
- **`isLoading = true`**: Show spinner
- **`StateHasChanged()`**: Update UI
- **`GetTripsAsync()`**: Call API
- **`??`**: Null-coalescing (use empty list if null)
- **`isLoading = false`**: Hide spinner
- **`StateHasChanged()`**: Update UI again

### Creating AdminLayout Component

**Create `SafeBoda.Admin/Shared/AdminLayout.razor`:**

```razor
@inject SafeBoda.Admin.Services.AuthService AuthService
@inject NavigationManager Navigation

@if (!isAuthenticated)
{
    <div class="d-flex justify-content-center align-items-center" style="min-height: 100vh;">
        <div class="text-center">
            <div class="spinner-border text-success" role="status"></div>
            <p class="mt-3 text-muted">Checking authentication...</p>
        </div>
    </div>
}
else
{
    <div class="d-flex admin-layout">
        <Sidebar CurrentPage="@CurrentPage" />
        
        <div class="flex-grow-1 main-content">
            @ChildContent
        </div>
    </div>
}

@code {
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string CurrentPage { get; set; } = "";

    private bool isAuthenticated = false;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            isAuthenticated = await AuthService.IsAuthenticatedAsync();
            
            if (!isAuthenticated)
            {
                Navigation.NavigateTo("/login");
                return;
            }

            StateHasChanged();
        }
    }
}
```

**Explanation:**
- **Route guard**: Checks authentication
- **If not authenticated**: Shows spinner, redirects to login
- **If authenticated**: Shows sidebar and content
- **`@ChildContent`**: Renders child components (Trips page content)

---

## Step 7: Fetch and Display Active Trips

### Already Implemented!

The Trips page already fetches and displays trips (see Step 6).

**Key points:**
- Uses `ApiClient.GetTripsAsync()`
- Shows loading spinner
- Displays trips in table
- Handles empty state
- Refreshes on button click

### Enhancing the Display

**Add search, pagination, filters** (already in actual implementation):
- Search bar
- Pagination controls
- Sort by columns
- Filter by status

---

## Step 8: Implement Logout Functionality

### Adding Logout to Sidebar

**In `Sidebar.razor`:**

```razor
<nav class="nav flex-column">
    <a class="nav-link" href="#" @onclick="HandleLogout" @onclick:preventDefault>
        <i class="bi bi-box-arrow-left"></i>
        <span>Logout</span>
    </a>
</nav>

@code {
    private async Task HandleLogout()
    {
        await AuthService.LogoutAsync();
        Navigation.NavigateTo("/");
    }
}
```

**Line-by-line explanation:**

**Line 1: Nav Link**
```razor
<a class="nav-link" href="#" @onclick="HandleLogout" @onclick:preventDefault>
```
- **`@onclick`**: Calls `HandleLogout` when clicked
- **`@onclick:preventDefault`**: Prevents default link behavior

**Line 6: HandleLogout**
```razor
private async Task HandleLogout()
{
    await AuthService.LogoutAsync();
    Navigation.NavigateTo("/");
}
```
- **`LogoutAsync()`**: Removes token from localStorage
- **`NavigateTo("/")`**: Redirects to home/login

### How Logout Works

```
1. User clicks "Logout"
   ↓
2. HandleLogout() called
   ↓
3. AuthService.LogoutAsync()
   - Removes token from localStorage
   - Removes user info from localStorage
   - Fires OnAuthStateChanged event
   ↓
4. NavigateTo("/")
   - Redirects to home page
   ↓
5. Home page checks authentication
   - Not authenticated → Redirects to /login
```

---

## Understanding Blazor Components

### Component Structure

**Every `.razor` file is a component:**

```razor
@page "/route"           <!-- Route (optional) -->
@inject Service Service  <!-- Dependency injection -->
@using Namespace         <!-- Using statements -->

<!-- HTML markup -->
<div>@variable</div>

@code {
    // C# code
    private string variable = "Hello";
}
```

### Component Lifecycle

**Methods called in order:**

1. **`OnInitialized()`**: Component created
2. **`OnParametersSet()`**: Parameters set
3. **`OnAfterRenderAsync(bool firstRender)`**: After render
4. **`StateHasChanged()`**: Manually trigger re-render

### Data Binding

**One-way binding:**
```razor
<p>@trip.Fare</p>
```

**Two-way binding:**
```razor
<InputText @bind-Value="email" />
```

**Event binding:**
```razor
<button @onclick="HandleClick">Click</button>
```

---

## Understanding Routing

### Route Attributes

**`@page` directive:**
```razor
@page "/trips"
@page "/trips/{id}"
```

**Route parameters:**
```razor
@page "/trips/{id}"

@code {
    [Parameter]
    public string Id { get; set; } = string.Empty;
}
```

### Navigation

**Using NavigationManager:**
```csharp
@inject NavigationManager Navigation

Navigation.NavigateTo("/dashboard");
```

**Using links:**
```razor
<a href="/trips">Trips</a>
```

---

## Understanding State Management

### Component State

**Local state:**
```csharp
private List<Trip> trips = new();
private bool isLoading = false;
```

### Service State

**Shared state (via services):**
```csharp
// AuthService stores authentication state
// Multiple components can access it
```

### Event-Based Updates

**Events notify changes:**
```csharp
public event Action? OnAuthStateChanged;

// Components subscribe:
AuthService.OnAuthStateChanged += () => StateHasChanged();
```

---

## Summary

In Step 5, we:

1. Created Blazor WebAssembly project
2. Configured CORS in API
3. Created ApiClient service
4. Implemented AuthService
5. Built Login page
6. Created Dashboard/Trips pages
7. Fetched and displayed trips
8. Implemented logout functionality

**Key Concepts Learned:**
- **Blazor WebAssembly**: C# in the browser
- **Razor Components**: `.razor` files with HTML + C#
- **CORS**: Cross-origin resource sharing
- **HttpClient**: Making API calls
- **localStorage**: Browser storage
- **Dependency Injection**: Services in Blazor
- **Routing**: `@page` directive
- **Data Binding**: `@bind-Value`, `@onclick`
- **Component Lifecycle**: `OnAfterRenderAsync`

**The Transformation:**
- **Before**: Only API (no UI)
- **After**: Beautiful admin portal

**What We Built:**
- Interactive web application
- Login and authentication
- Dashboard with statistics
- Trips management
- Real-time data display

**What's Next?**
In future steps, we'll:
- Add more pages (Drivers, Riders, Users)
- Add charts and visualizations
- Add real-time updates
- Deploy to production

---

## Common Questions

**Q: Why Blazor instead of React/Angular?**
A: Write C# instead of JavaScript, share code with backend, strong typing, better tooling.

**Q: How does Blazor WebAssembly work?**
A: .NET code compiles to WebAssembly, runs in browser like JavaScript, but faster and type-safe.

**Q: What is localStorage?**
A: Browser storage that persists across sessions. Perfect for storing tokens.

**Q: Why do we need CORS?**
A: Browser security blocks cross-origin requests. CORS allows them (with permission).

**Q: How does two-way binding work?**
A: `@bind-Value` creates event handlers that update both UI and variable automatically.

**Q: What is StateHasChanged()?**
A: Tells Blazor to re-render the component. Called automatically, but can be called manually.

**Q: Can I use JavaScript in Blazor?**
A: Yes! Via `IJSRuntime` (JavaScript interop). But try to use C# when possible.

**Q: How do I handle errors?**
A: Use try-catch blocks, show error messages in UI, log errors for debugging.

---

## Conclusion

Congratulations! You've built a complete admin portal! 🎉

**What you've achieved:**
- Modern web application with Blazor
- Secure authentication
- Real-time data display
- Beautiful, interactive UI
- Full CRUD operations

**The journey:**
- **Step 1**: Domain models
- **Step 2**: Web API
- **Step 3**: Database persistence
- **Step 4**: Authentication & authorization
- **Step 5**: Admin portal

Your SafeBoda application is now **complete** with both backend API and frontend admin portal! Administrators can manage the platform through a beautiful, user-friendly interface.

**Remember**: A good UI makes complex systems easy to use. The admin portal you've built provides a professional interface for managing the SafeBoda platform! 🚀

