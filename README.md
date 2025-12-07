[![Review Assignment Due Date](https://classroom.github.com/assets/deadline-readme-button-22041afd0340ce965d47ae6ef1cefeee28c7c493a6346c4f15d667ab976d596c.svg)](https://classroom.github.com/a/42_L1Zp_)
[![Open in Codespaces](https://classroom.github.com/assets/launch-codespace-2972f46106e565e64193e422d61a12cf1da4916b45550586e14ef0a7c637dd04.svg)](https://classroom.github.com/open-in-codespaces?assignment_repo_id=21141538)



# SafeBoda - Ride-Hailing Platform

## Overview
SafeBoda is a comprehensive ride-hailing platform built with .NET, featuring a RESTful API with JWT authentication and a modern Blazor WebAssembly admin portal for managing users, riders, drivers, and trips.

## Features

### Backend API
- **JWT Authentication & Authorization** with role-based access control
- **RESTful API** with Swagger documentation
- **PostgreSQL Database** with Entity Framework Core
- **Role Management**: Admin, Rider, and Driver roles
- **CRUD Operations** for Users, Riders, Drivers, and Trips
- **Statistics Dashboard** API endpoints
- **Asynchronous Programming** - All database operations use async/await for improved performance
- **In-Memory Caching** - GET api/trips endpoint uses caching to reduce database load
- **Comprehensive Testing** - Unit tests and integration tests with xUnit, Moq, and WebApplicationFactory

### Admin Portal (Blazor WebAssembly)
- **Modern UI** with Bootstrap 5 and SafeBoda branding
- **Authentication System** with JWT token management
- **Interactive Dashboard** with Chart.js visualizations
- **Collapsible Sidebar** with state persistence
- **Searchable Dropdowns** with autocomplete for trip creation
- **CRUD Management** for Users, Riders, Drivers, and Trips
- **Responsive Design** for mobile and desktop

## Prerequisites

- .NET 9.0 SDK or later
- PostgreSQL 12 or later
- Visual Studio 2022 or VS Code (optional)

## Quick Start

```bash
# 1. Update database connection in SafeBoda.Api/appsettings.json
# 2. Run migrations
dotnet ef database update --project SafeBoda.Infrastructure --startup-project SafeBoda.Api

# 3. Start the API (Terminal 1)
cd SafeBoda.Api
dotnet run

# 4. Start the Admin Portal (Terminal 2)
cd SafeBoda.Admin
dotnet run

# 5. Login to Admin Portal
# URL: http://localhost:5086
# Email: admin@safeboda.com
# Password: Admin@123
```

## Detailed Setup Instructions

### 1. Clone the Repository
```bash
git clone <repository-url>
cd module-1-safeboda-foundation-setup-faustin12-g
```

### 2. Configure PostgreSQL Database

Update the connection string in `SafeBoda.Api/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "SafeBodaDb": "Host=localhost;Port=5432;Database=SafeBodaDb;Username=postgres;Password=YOUR_PASSWORD"
  }
}
```

### 3. Run Database Migrations
```bash
dotnet ef database update --project SafeBoda.Infrastructure --startup-project SafeBoda.Api
```

### 4. Run the API
```bash
cd SafeBoda.Api
dotnet run
```
The API will start at `http://localhost:5103`

### 5. Run the Admin Portal
Open a new terminal:
```bash
cd SafeBoda.Admin
dotnet run
```
The admin portal will start at `http://localhost:5086`

### 6. Access the Applications

- **API Swagger**: http://localhost:5103/swagger
- **Admin Portal**: http://localhost:5086

## Default Admin Credentials

An admin account is automatically created when the API starts:
- **Email**: `admin@safeboda.com`
- **Password**: `Admin@123`

Use these credentials to log in to the admin portal or obtain a JWT token via the API.

## Getting Started - Complete Workflow

Follow this workflow to get familiar with the admin portal:

### 1. First Login
- Navigate to `http://localhost:5086`
- Login with `admin@safeboda.com` / `Admin@123`
- Explore the Dashboard

### 2. Add Some Riders
- Go to **Riders** page
- Click **"Add Rider"**
- Create 3-5 riders with different names and phone numbers
- Example: "John Doe" - "+250788123456"

### 3. Add Some Drivers
- Go to **Drivers** page
- Click **"Add Driver"**
- Create 3-5 drivers with names, phones, and plate numbers
- Example: "Jane Smith" - "+250788234567" - "RAB123A"

### 4. Create Your First Trip
- Go to **Trips** page
- Click **"Add Trip"**
- Use the searchable dropdown to select a rider (try typing to search!)
- Use the searchable dropdown to select a driver
- Enter coordinates:
  - Start: Latitude `-1.9441`, Longitude `30.0619` (Kigali)
  - End: Latitude `-1.9706`, Longitude `30.1044`
- Enter fare: `5000`
- Click **"Create"**

### 5. View Dashboard Statistics
- Return to **Dashboard**
- See your statistics updated
- View the charts showing your data

### 6. Try Editing and Deleting
- Edit a rider's phone number
- Edit a driver's plate number
- Delete a test trip
- See changes reflected immediately

## Using the Admin Portal

### Step 1: Login to Admin Portal

1. Open your browser and navigate to `http://localhost:5086`
2. You'll see the login page
3. Enter the default admin credentials:
   - **Email**: `admin@safeboda.com`
   - **Password**: `Admin@123`
4. Click **"Sign In"**
5. You'll be redirected to the Dashboard

### Step 2: Navigate the Dashboard

The Dashboard shows:
- **Statistics Cards**: 
  - Total Users
  - Total Trips
  - Total Riders
  - Total Drivers
- **Charts**:
  - Trips per Month (Bar Chart)
  - Users by Role (Pie Chart)
- **Quick Actions**: Links to manage different entities

### Step 3: Manage Users

#### View All Users
1. Click **"Users"** in the sidebar
2. You'll see a table with all registered users
3. Each user shows: Email, Full Name, and Roles

#### Create a New Admin User
1. Click the **"Add User"** button
2. Fill in the form:
   - **Email**: Enter a valid email
   - **Full Name**: Enter the user's name
   - **Password**: Must meet requirements (6+ chars, uppercase, lowercase, digit)
3. Click **"Create"**
4. The new user will appear in the list

#### Delete a User
1. Find the user in the table
2. Click the **red "Delete"** button
3. Confirm the deletion
4. The user will be removed

### Step 4: Manage Riders

#### View All Riders
1. Click **"Riders"** in the sidebar
2. You'll see all registered riders with their phone numbers

#### Add a New Rider
1. Click the **"Add Rider"** button
2. Fill in the form:
   - **Name**: Enter the rider's full name
   - **Phone Number**: Enter phone number (e.g., +250788123456)
3. Click **"Create"**
4. The new rider appears in the list

#### Edit a Rider
1. Click the **blue "Edit"** button next to a rider
2. Update the name or phone number
3. Click **"Update"**
4. Changes are saved immediately

#### Delete a Rider
1. Click the **red "Delete"** button
2. Confirm the deletion
3. The rider is removed from the system

### Step 5: Manage Drivers

#### View All Drivers
1. Click **"Drivers"** in the sidebar
2. You'll see all drivers with their phone numbers and moto plate numbers

#### Add a New Driver
1. Click the **"Add Driver"** button
2. Fill in the form:
   - **Name**: Enter the driver's full name
   - **Phone Number**: Enter phone number
   - **Moto Plate Number**: Enter the motorcycle registration number
3. Click **"Create"**
4. The new driver appears in the list

#### Edit a Driver
1. Click the **blue "Edit"** button
2. Update any field (name, phone, or plate number)
3. Click **"Update"**
4. Changes are saved

#### Delete a Driver
1. Click the **red "Delete"** button
2. Confirm the deletion
3. The driver is removed

### Step 6: Manage Trips

#### View All Trips
1. Click **"Trips"** in the sidebar
2. You'll see all trips with:
   - Start and End locations (latitude/longitude)
   - Fare amount
   - Request time
   - Status

#### Create a New Trip (Using Searchable Dropdowns)

1. Click the **"Add Trip"** button
2. **Select a Rider**:
   - Click the "Select Rider" input field
   - A dropdown appears with all available riders
   - Type to search by name or phone number
   - Click on a rider to select them
   - Or use arrow keys and press Enter
3. **Select a Driver**:
   - Click the "Select Driver" input field
   - A dropdown appears with all available drivers
   - Type to search by name or phone number
   - Click on a driver to select them
4. **Enter Trip Details**:
   - **Start Latitude**: e.g., -1.9441
   - **Start Longitude**: e.g., 30.0619
   - **End Latitude**: e.g., -1.9706
   - **End Longitude**: e.g., 30.1044
   - **Fare**: Enter the trip cost (e.g., 5000)
5. Click **"Create"**
6. The new trip appears in the list

#### Edit a Trip
1. Click the **blue "Edit"** button next to a trip
2. Update the location coordinates or fare
3. Click **"Update"**
4. Changes are saved

### Step 7: Using the Searchable Dropdown

The searchable dropdown is a key feature for selecting riders and drivers:

**Features**:
- **Click to Open**: Click the input to see all options
- **Search**: Type to filter results in real-time
- **Keyboard Navigation**:
  - `Arrow Down/Up`: Navigate through options
  - `Enter`: Select highlighted option
  - `Escape`: Close dropdown
- **Visual Feedback**: Selected option shows in green
- **No Results**: Shows "No results found" when search doesn't match

**Example Usage**:
1. Click "Select Rider" input
2. Type "John" - only riders with "John" in their name appear
3. Use arrow keys to highlight the correct rider
4. Press Enter or click to select

### Step 8: Logout

1. Click the **"Logout"** button in the sidebar
2. You'll be redirected to the login page
3. Your session is cleared

---

## Common Issues and Solutions

### Issue 1: Cannot login to admin portal
**Solution**: 
- Ensure the API is running on `http://localhost:5103`
- Verify you're using the correct credentials: `admin@safeboda.com` / `Admin@123`
- Check browser console for errors

### Issue 2: "Failed to fetch" error
**Solution**: 
- Make sure the API is running before starting the admin portal
- Verify CORS is configured correctly in the API
- Check that both applications are using the correct ports

### Issue 3: Searchable dropdown shows no data
**Solution**: 
- Ensure you have created riders/drivers first
- Refresh the page to reload data
- Check browser console for API errors
- Verify the API is returning data (check Swagger)

### Issue 4: Database connection error when starting API
**Solution**: 
- Ensure PostgreSQL is running
- Verify connection string in `SafeBoda.Api/appsettings.json`
- Check PostgreSQL port (default: 5432)
- Confirm database exists or run migrations

### Issue 5: Charts not displaying on dashboard
**Solution**: 
- Ensure Chart.js is loaded (check browser console)
- Verify you have some data in the database
- Try refreshing the page

### Issue 6: Changes not saving
**Solution**: 
- Check browser console for errors
- Ensure the API is running and accessible
- Verify you have proper permissions (logged in as admin)
- Check network tab for failed requests

---

## Password Requirements

When registering users, passwords must meet these requirements:
- At least 6 characters long
- Contains at least one digit (0-9)
- Contains at least one lowercase letter (a-z)
- Contains at least one uppercase letter (A-Z)

---

## Available Roles

1. **Admin**: Full access to all endpoints including admin panel
2. **Rider**: Can access trip-related endpoints
3. **Driver**: Can access trip-related endpoints

---

## API Endpoints Reference

> **Note**: The admin portal handles all API calls automatically. This section is for developers who want to integrate with the API directly.

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login and get JWT token

### Admin Operations
- `GET /api/admin/users` - Get all users
- `POST /api/admin/users` - Create admin user
- `DELETE /api/admin/users/{id}` - Delete user
- `GET /api/admin/stats` - Get statistics
- `GET /api/admin/trips` - Get all trips
- `GET /api/admin/riders` - Get all riders
- `GET /api/admin/drivers` - Get all drivers

### Riders CRUD
- `GET /api/riders/{id}` - Get rider
- `POST /api/riders` - Create rider
- `PUT /api/riders/{id}` - Update rider
- `DELETE /api/riders/{id}` - Delete rider

### Drivers CRUD
- `GET /api/drivers/{id}` - Get driver
- `POST /api/drivers` - Create driver
- `PUT /api/drivers/{id}` - Update driver
- `DELETE /api/drivers/{id}` - Delete driver

### Trips
- `GET /api/trips` - Get active trips
- `POST /api/trips/request` - Request trip

**API Documentation**: Access Swagger UI at `http://localhost:5103/swagger` for detailed API documentation and testing.

---

## Testing

The project includes comprehensive unit and integration tests for the API.

### Running Tests

```bash
# Run all tests
dotnet test SafeBoda.Api.Tests\SafeBoda.Api.Tests.csproj

# Run tests with detailed output
dotnet test SafeBoda.Api.Tests\SafeBoda.Api.Tests.csproj --verbosity normal
```

### Test Coverage

#### Unit Tests (`TripsControllerTests.cs`)
- Tests `TripsController` in isolation using Moq framework
- Mocks `ITripRepository` to test controller logic without database dependencies
- Covers all controller actions:
  - `GetActiveTripsAsync` - Successful retrieval, empty results
  - `GetTripByIdAsync` - Found, not found scenarios
  - `CreateTripAsync` - Valid requests, invalid inputs
  - `UpdateTripAsync` - Successful updates, not found scenarios
  - `DeleteTripAsync` - Successful deletion, not found scenarios
- Verifies correct repository method calls
- Tests edge cases and error conditions

#### Integration Tests (`TripsControllerIntegrationTests.cs`)
- Tests the full application flow using `WebApplicationFactory`
- Uses in-memory database for testing (no real database required)
- Tests complete HTTP request/response cycle
- Verifies database interactions
- Tests JWT authentication
- Covers all CRUD operations end-to-end

### Test Technologies
- **xUnit** - Testing framework
- **Moq** - Mocking framework for unit tests
- **Microsoft.AspNetCore.Mvc.Testing** - Integration testing with WebApplicationFactory
- **Microsoft.EntityFrameworkCore.InMemory** - In-memory database for integration tests

---

## Performance Optimizations

### Asynchronous Programming
All database operations have been converted to asynchronous methods:
- Repository methods use `async Task<T>` signatures
- All EF Core calls use async methods (`ToListAsync`, `FirstOrDefaultAsync`, `SaveChangesAsync`, etc.)
- Controller actions are async to improve scalability
- Benefits:
  - Non-blocking I/O operations
  - Better resource utilization
  - Improved application responsiveness
  - Better handling of concurrent requests

### In-Memory Caching
The `GET /api/trips` endpoint implements intelligent caching:
- **Cache Duration**: 1 minute (absolute expiration)
- **Cache Key**: "ActiveTrips"
- **Cache Behavior**:
  - First request: Fetches from database and stores in cache
  - Subsequent requests (within 1 minute): Returns cached data (no database query)
  - After expiration: Next request fetches fresh data from database
- **Cache Invalidation**: Cache is automatically cleared when:
  - A new trip is created
  - A trip is updated
  - A trip is deleted
- **Benefits**:
  - Reduced database load
  - Faster response times for repeated requests
  - Automatic data freshness through expiration and invalidation

---

## Project Structure

```
SafeBoda/
├── SafeBoda.Api/              # ASP.NET Core Web API
│   ├── Controllers/           # API Controllers
│   ├── Models/               # DTOs and Request/Response models
│   └── Services/             # JWT Token Service
├── SafeBoda.Admin/           # Blazor WebAssembly Admin Portal
│   ├── Pages/                # Blazor pages (Dashboard, Users, etc.)
│   ├── Shared/               # Shared components (Sidebar, Layout, SearchableSelect)
│   ├── Services/             # API Client and Auth Service
│   ├── Models/               # Client-side models
│   └── wwwroot/              # Static files, CSS, JS
├── SafeBoda.Core/            # Domain models
├── SafeBoda.Application/     # Business logic and interfaces
├── SafeBoda.Infrastructure/  # Data access and EF Core
│   ├── Data/                 # DbContext
│   ├── Entities/             # Database entities
│   ├── Migrations/           # EF Core migrations
│   └── Repositories/         # Repository implementations
└── SafeBoda.Api.Tests/       # Test project
    ├── TripsControllerTests.cs           # Unit tests for TripsController
    └── TripsControllerIntegrationTests.cs # Integration tests for TripsController
```

---

## Technologies Used

### Backend
- .NET 9.0
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- JWT Authentication
- Swagger/OpenAPI
- In-Memory Caching (IMemoryCache)
- Asynchronous Programming (async/await)

### Frontend (Admin Portal)
- Blazor WebAssembly
- Bootstrap 5
- Chart.js
- Bootstrap Icons
- LocalStorage for state persistence

---

## Key Features Implemented

1. **JWT Authentication** with role-based authorization
2. **PostgreSQL Database** with Entity Framework Core
3. **Blazor Admin Portal** with modern UI
4. **Searchable Dropdowns** with autocomplete
5. **Interactive Dashboard** with charts
6. **CRUD Operations** for all entities
7. **Collapsible Sidebar** with state persistence
8. **Responsive Design** for mobile and desktop
9. **API Documentation** with Swagger
10. **Role Management** (Admin, Rider, Driver)
11. **Asynchronous Programming** - All repository methods and controllers use async/await
12. **In-Memory Caching** - GET api/trips endpoint caches data for 1 minute to improve performance
13. **Comprehensive Testing** - Unit tests with Moq and integration tests with WebApplicationFactory

---

## Developer Information

**Created by**: Faustin Nshimiyimana  
**Email**: nshimiefaustinpeace@gmail.com  

---

## License

This project is part of an academic assignment.

---

## Thank You!

[![Review Assignment Due Date](https://classroom.github.com/assets/deadline-readme-button-22041afd0340ce965d47ae6ef1cefeee28c7c493a6346c4f15d667ab976d596c.svg)](https://classroom.github.com/a/o_SrBHns)
[![Open in Codespaces](https://classroom.github.com/assets/launch-codespace-2972f46106e565e64193e422d61a12cf1da4916b45550586e14ef0a7c637dd04.svg)](https://classroom.github.com/open-in-codespaces?assignment_repo_id=21806815)
