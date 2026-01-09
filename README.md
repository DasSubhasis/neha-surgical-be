# Neha Surgical API - Doctor CRUD API

## Overview
This API provides CRUD operations for managing Doctor records in the Neha Surgical Hospital Management System.

## Prerequisites
- .NET 8.0 SDK
- PostgreSQL database
- Entity Framework Core tools

## Setup Instructions

### 1. Install EF Core Tools (if not already installed)
```cmd
dotnet tool install --global dotnet-ef
```

### 2. Update Database Connection String
Edit `appsettings.json` and update the connection string with your PostgreSQL credentials:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=nehasurgical;Username=postgres;Password=your_password_here"
}
```

### 3. Stop the Running Application
If the application is currently running, stop it before building or running migrations.

### 4. Create Database Migration
```cmd
dotnet ef migrations add InitialCreate
```

### 5. Apply Migration to Database
```cmd
dotnet ef database update
```

This will create the `doctors` table with the following structure:
- `doctor_id` (SERIAL PRIMARY KEY)
- `name` (VARCHAR(100) NOT NULL)
- `contact_no` (VARCHAR(20) NOT NULL UNIQUE)
- `email` (VARCHAR(100))
- `specialization` (VARCHAR(100))
- `dob` (DATE) - Date of Birth
- `doa` (DATE) - Date of Appointment
- `identifier` (VARCHAR(100)) - Registration number or unique identifier
- `remarks` (VARCHAR(250))
- `is_active` (BOOLEAN NOT NULL DEFAULT TRUE)

### 6. Run the Application
```cmd
dotnet run
```

The API will be available at:
- **HTTP**: http://localhost:5000
- **HTTPS**: https://localhost:5001
- **Swagger UI**: https://localhost:5001/swagger

## API Endpoints

### Get All Doctors
```
GET /api/doctors
GET /api/doctors?includeInactive=true
```
Returns all active doctors. Use `includeInactive=true` to include inactive doctors.

**Response**: `200 OK`
```json
[
  {
    "doctorId": 1,
    "name": "Dr. John Smith",
    "contactNo": "9876543210",
    "email": "john.smith@example.com",
    "specialization": "Cardiology",
    "dob": "1980-05-15",
    "doa": "2020-01-10",
    "identifier": "MED12345",
    "remarks": "Senior consultant",
    "isActive": true
  }
]
```

### Get Doctor by ID
```
GET /api/doctors/{id}
```

**Response**: `200 OK` or `404 Not Found`

### Create Doctor
```
POST /api/doctors
Content-Type: application/json
```

**Request Body**:
```json
{
  "name": "Dr. John Smith",
  "contactNo": "9876543210",
  "email": "john.smith@example.com",
  "specialization": "Cardiology",
  "dob": "1980-05-15",
  "doa": "2020-01-10",
  "identifier": "MED12345",
  "remarks": "Senior consultant",
  "isActive": true
}
```

**Response**: `201 Created` or `400 Bad Request`

**Validations**:
- `name` and `contactNo` are required
- `contactNo` must be unique
- `identifier` must be unique (if provided)
- `email` must be valid email format (if provided)

### Update Doctor
```
PUT /api/doctors/{id}
Content-Type: application/json
```

**Request Body**:
```json
{
  "name": "Dr. John Smith",
  "contactNo": "9876543210",
  "email": "john.smith@example.com",
  "specialization": "Cardiology",
  "dob": "1980-05-15",
  "doa": "2020-01-10",
  "identifier": "MED12345",
  "remarks": "Senior consultant",
  "isActive": true
}
```

**Response**: `200 OK`, `400 Bad Request`, or `404 Not Found`

### Delete Doctor
```
DELETE /api/doctors/{id}
```

**Response**: `204 No Content` or `404 Not Found`

## Architecture

The API follows a layered architecture:

### Models Layer (`Models/`)
- `Doctor.cs`: Entity class with database mappings

### DTOs Layer (`DTOs/`)
- `DoctorResponseDto`: Response DTO for API responses
- `CreateDoctorDto`: DTO for creating new doctors
- `UpdateDoctorDto`: DTO for updating existing doctors

### Data Layer (`Data/`)
- `ApplicationDbContext.cs`: Entity Framework DbContext with database configuration

### Repository Layer (`Repositories/`)
- `IDoctorRepository`: Repository interface
- `DoctorRepository`: Repository implementation for data access

### Service Layer (`Services/`)
- `IDoctorService`: Service interface
- `DoctorService`: Business logic and validation

### Controller Layer (`Controllers/`)
- `DoctorsController`: API endpoints for CRUD operations

## Business Rules

1. **Contact Number Uniqueness**: Each doctor must have a unique contact number
2. **Identifier Uniqueness**: If an identifier is provided, it must be unique
3. **Active Flag**: Soft delete support via `isActive` flag
4. **Data Validation**: 
   - Name and Contact Number are required
   - Email must be valid format if provided
   - Field length constraints enforced

## Error Handling

The API provides appropriate HTTP status codes:
- `200 OK`: Successful GET/PUT
- `201 Created`: Successful POST
- `204 No Content`: Successful DELETE
- `400 Bad Request`: Validation errors
- `404 Not Found`: Resource not found
- `500 Internal Server Error`: Server errors

## Testing with Swagger

Once the application is running, navigate to the Swagger UI at `https://localhost:5001/swagger` to test all endpoints interactively.

## Next Steps

After stopping the running application, execute these commands to create and apply the database migration:

```cmd
cd "d:\Works\ZiCorp\Projects\Neha Surgical\Development\NehaSurgicalAPI"
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Then restart the application:
```cmd
dotnet run
```

## Docker

### Build the Docker image
```
docker build -t neha-surgical-api .
```

### Run the Docker container
```
docker run -d -p 8080:80 --name neha-surgical-api neha-surgical-api
```

- The API will be available at http://localhost:8080
- Adjust port mapping as needed.
- For custom configuration, mount your own `appsettings.json` as a volume.
