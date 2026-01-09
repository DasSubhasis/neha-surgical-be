# Neha Surgical API - AI Development Guide

## Architecture Overview

**Technology Stack:**
- .NET 8.0 ASP.NET Core Web API
- PostgreSQL database with Dapper ORM (not Entity Framework)
- Dual authentication: API Key (frontend apps) + JWT Bearer (users)
- BCrypt for password hashing

**Key Architectural Patterns:**
- Direct SQL queries using Dapper (no repository/service layers for most controllers)
- Controllers inject `NpgsqlConnection` directly (scoped lifetime)
- Models use `[Column("snake_case")]` attributes to map PostgreSQL naming
- DTOs handle all API request/response contracts
- Middleware handles cross-cutting concerns (API key validation, JWT)

## Database Conventions

**Critical Pattern:** PostgreSQL uses `snake_case` for all table/column names:
```csharp
// Model uses PascalCase properties with [Column] mapping
public class Doctor {
    [Column("doctor_id")]
    public int DoctorId { get; set; }
    
    [Column("doctor_name")]
    public string DoctorName { get; set; }
}
```

**Boolean Fields:** Use `CHAR(1)` with 'Y'/'N' values (not true/false):
```sql
is_active CHAR(1) NOT NULL DEFAULT 'Y' CHECK (is_active IN ('Y', 'N'))
```

**Date/Time Handling:** Custom Dapper type handlers registered in `Program.cs` and controllers:
```csharp
SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
SqlMapper.AddTypeHandler(new TimeOnlyTypeHandler());
```

## Authentication System

**Dual-layer auth** (see `AUTHENTICATION_GUIDE.md` for details):
1. **API Key** (`X-API-Key` header) - validates frontend application
2. **JWT Bearer** (`Authorization: Bearer {token}`) - validates user session

**Middleware Order in Program.cs:**
```csharp
// API key middleware is COMMENTED OUT but code exists
// app.UseMiddleware<ApiKeyAuthMiddleware>();
app.UseAuthentication();  // JWT validation
app.UseAuthorization();
```

**Current State:** API key middleware exists but is disabled. Most endpoints bypass API key checks.

## Development Workflow

**Build & Run:**
```bash
dotnet run                    # Starts API on https://localhost:5001
dotnet watch run              # Hot reload during development
```

**Database Setup:**
1. Scripts in `Database/` folder must be run manually in PostgreSQL
2. No EF migrations - all schema changes via SQL scripts
3. Connection string in `appsettings.json`

**Testing:**
- Swagger UI: https://localhost:5001/swagger
- Supports dual auth (API key + Bearer token in Swagger UI)
- Sample credentials in `AUTHENTICATION_GUIDE.md`

## Common Patterns

**Controller Structure:**
```csharp
[ApiController]
[Route("api/[controller]")]
public class DoctorsController : ControllerBase
{
    private readonly NpgsqlConnection _connection;
    
    public DoctorsController(NpgsqlConnection connection)
    {
        _connection = connection;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? isActive)
    {
        var sql = "SELECT doctor_id as DoctorId, ... FROM doctors";
        var results = await _connection.QueryAsync<Doctor>(sql);
        return Ok(new { message = "Success", data = results });
    }
}
```

**Response Format Convention:**
```csharp
// Success responses wrap data in { message, data } structure
return Ok(new { message = "Orders retrieved successfully", data = orderDtos });

// Error responses use { message } or { message, errors }
return BadRequest(new { message = "Validation failed", errors = ModelState });
```

**Complex Queries:** For orders/nested data, see `OrdersController.cs` - uses manual DTO mapping:
```csharp
var orderDto = await MapToOrderDto(order);  // Custom mapping method
```

## Project-Specific Quirks

**Connection State Management:**
```csharp
// Always check/open connection before queries
if (_connection.State != System.Data.ConnectionState.Open)
    await _connection.OpenAsync();
```

**Soft Deletes:** All tables use `is_active` flag (CHAR(1) 'Y'/'N'), not actual deletion.

**Order System Complexity:**
- Orders have item groups AND individual items (see `CreateOrdersTable.sql`)
- Audit trail in `OrderAudits` table
- Status workflow: Pending → Assigned → Dispatched → Completed (or Canceled)

**Session Support:** Configured but appears unused in current endpoints:
```csharp
builder.Services.AddSession(options => { ... });  // In Program.cs
```

## Key Files Reference

- **Authentication:** `AUTHENTICATION_GUIDE.md`, `Middleware/ApiKeyAuthMiddleware.cs`
- **Database Schema:** `Database/*.sql` (run manually, no migrations)
- **Type Handlers:** `Data/DateOnlyTypeHandler.cs` (required for DateOnly/TimeOnly)
- **Complex Example:** `Controllers/OrdersController.cs` (nested DTOs, manual mapping)
- **Configuration:** `appsettings.json` (JWT keys, CORS, connection string)

## When Adding New Features

1. **New Entity:** Create SQL script in `Database/`, model in `Models/`, DTOs in `DTOs/`
2. **New Controller:** Inject `NpgsqlConnection`, use Dapper, follow response format convention
3. **Date Fields:** Add type handler registration if using DateOnly/TimeOnly
4. **Auth Required:** Check if API key middleware should be re-enabled for endpoint
5. **CORS:** Update `appsettings.json` AllowedOrigins if new frontend origin needed
