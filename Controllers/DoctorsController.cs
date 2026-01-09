using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using NehaSurgicalAPI.DTOs;
using NehaSurgicalAPI.Models;

namespace NehaSurgicalAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DoctorsController : ControllerBase
{
    private readonly NpgsqlConnection _connection;
    private readonly ILogger<DoctorsController> _logger;

    public DoctorsController(NpgsqlConnection connection, ILogger<DoctorsController> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DoctorResponseDto>>> GetAllDoctors([FromQuery] string? isActive = null)
    {
        try
        {
            var sql = "SELECT doctor_id as DoctorId, doctor_name as DoctorName, contact_no as ContactNo, email as Email, " +
                      "specialization as Specialization, dob as Dob, doa as Doa, identifier as Identifier, " +
                      "registration_number as RegistrationNumber, location as Location, " +
                      "remarks as Remarks, is_active as IsActive FROM doctors";

            // Filter by is_active if parameter is provided
            if (!string.IsNullOrEmpty(isActive) && (isActive == "Y" || isActive == "N"))
            {
                sql += $" WHERE is_active = '{isActive}'";
            }

            sql += " ORDER BY doctor_name";

            var doctors = await _connection.QueryAsync<Doctor>(sql);
            return Ok(doctors.Select(MapToDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving doctors");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DoctorResponseDto>> GetDoctorById(int id)
    {
        try
        {
            var sql = "SELECT doctor_id as DoctorId, doctor_name as DoctorName, contact_no as ContactNo, email as Email, " +
                      "specialization as Specialization, dob as Dob, doa as Doa, identifier as Identifier, " +
                      "registration_number as RegistrationNumber, location as Location, " +
                      "remarks as Remarks, is_active as IsActive FROM doctors WHERE doctor_id = @Id";
            var doctor = await _connection.QueryFirstOrDefaultAsync<Doctor>(sql, new { Id = id });

            if (doctor == null)
            {
                return NotFound(new { message = $"Doctor with ID {id} not found" });
            }

            return Ok(MapToDto(doctor));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving doctor with ID {DoctorId}", id);
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<DoctorResponseDto>> CreateDoctor([FromBody] CreateDoctorDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Validation failed", errors = ModelState });
            }

            // Validate unique contact number
            var contactExists = await _connection.ExecuteScalarAsync<bool>(
                "SELECT CASE WHEN EXISTS(SELECT 1 FROM doctors WHERE contact_no = @ContactNo) THEN 1 ELSE 0 END",
                new { createDto.ContactNo });

            if (contactExists)
            {
                return BadRequest(new { message = $"A doctor with contact number '{createDto.ContactNo}' already exists." });
            }

            // Validate unique identifier if provided
            if (!string.IsNullOrWhiteSpace(createDto.Identifier))
            {
                var identifierExists = await _connection.ExecuteScalarAsync<bool>(
                    "SELECT CASE WHEN EXISTS(SELECT 1 FROM doctors WHERE identifier = @Identifier) THEN 1 ELSE 0 END",
                    new { createDto.Identifier });

                if (identifierExists)
                {
                    return BadRequest(new { message = $"A doctor with identifier '{createDto.Identifier}' already exists." });
                }
            }

            var sql = @"
                INSERT INTO doctors (doctor_name, contact_no, email, specialization, dob, doa, identifier, registration_number, location, remarks, is_active)
                VALUES (@DoctorName, @ContactNo, @Email, @Specialization, @Dob, @Doa, @Identifier, @RegistrationNumber, @Location, @Remarks, @IsActive)
                RETURNING doctor_id";

            var parameters = new
            {
                createDto.DoctorName,
                createDto.ContactNo,
                createDto.Email,
                createDto.Specialization,
                Dob = !string.IsNullOrWhiteSpace(createDto.Dob) ? DateTime.Parse(createDto.Dob) : (DateTime?)null,
                Doa = !string.IsNullOrWhiteSpace(createDto.Doa) ? DateTime.Parse(createDto.Doa) : (DateTime?)null,
                createDto.Identifier,
                createDto.RegistrationNumber,
                createDto.Location,
                createDto.Remarks,
                createDto.IsActive
            };

            var doctorId = await _connection.ExecuteScalarAsync<int>(sql, parameters);
            
            // Fetch the inserted record
            var doctor = await _connection.QuerySingleAsync<Doctor>(
                "SELECT doctor_id as DoctorId, doctor_name as DoctorName, contact_no as ContactNo, email as Email, " +
                "specialization as Specialization, dob as Dob, doa as Doa, identifier as Identifier, " +
                "registration_number as RegistrationNumber, location as Location, " +
                "remarks as Remarks, is_active as IsActive FROM doctors WHERE doctor_id = @DoctorId",
                new { DoctorId = doctorId });
            var response = new
            {
                message = "Doctor record inserted successfully",
                data = MapToDto(doctor)
            };
            return CreatedAtAction(nameof(GetDoctorById), new { id = doctor.DoctorId }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating doctor");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<DoctorResponseDto>> UpdateDoctor(int id, [FromBody] UpdateDoctorDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Validation failed", errors = ModelState });
            }

            // Check if doctor exists
            var exists = await _connection.ExecuteScalarAsync<bool>(
                "SELECT CASE WHEN EXISTS(SELECT 1 FROM doctors WHERE doctor_id = @Id) THEN 1 ELSE 0 END",
                new { Id = id });

            if (!exists)
            {
                return NotFound(new { message = $"Doctor with ID {id} not found." });
            }

            // Validate unique contact number (excluding current doctor)
            var contactExists = await _connection.ExecuteScalarAsync<bool>(
                "SELECT CASE WHEN EXISTS(SELECT 1 FROM doctors WHERE contact_no = @ContactNo AND doctor_id != @Id) THEN 1 ELSE 0 END",
                new { updateDto.ContactNo, Id = id });

            if (contactExists)
            {
                return BadRequest(new { message = $"A doctor with contact number '{updateDto.ContactNo}' already exists." });
            }

            // Validate unique identifier if provided (excluding current doctor)
            if (!string.IsNullOrWhiteSpace(updateDto.Identifier))
            {
                var identifierExists = await _connection.ExecuteScalarAsync<bool>(
                    "SELECT CASE WHEN EXISTS(SELECT 1 FROM doctors WHERE identifier = @Identifier AND doctor_id != @Id) THEN 1 ELSE 0 END",
                    new { updateDto.Identifier, Id = id });

                if (identifierExists)
                {
                    return BadRequest(new { message = $"A doctor with identifier '{updateDto.Identifier}' already exists." });
                }
            }

            var sql = @"
                UPDATE doctors 
                SET doctor_name = @DoctorName, contact_no = @ContactNo, email = @Email, 
                    specialization = @Specialization, dob = @Dob, doa = @Doa, 
                    identifier = @Identifier, registration_number = @RegistrationNumber, 
                    location = @Location, remarks = @Remarks, is_active = @IsActive
                WHERE doctor_id = @Id";

            var parameters = new
            {
                Id = id,
                updateDto.DoctorName,
                updateDto.ContactNo,
                updateDto.Email,
                updateDto.Specialization,
                Dob = !string.IsNullOrWhiteSpace(updateDto.Dob) ? DateTime.Parse(updateDto.Dob) : (DateTime?)null,
                Doa = !string.IsNullOrWhiteSpace(updateDto.Doa) ? DateTime.Parse(updateDto.Doa) : (DateTime?)null,
                updateDto.Identifier,
                updateDto.RegistrationNumber,
                updateDto.Location,
                updateDto.Remarks,
                updateDto.IsActive
            };

            await _connection.ExecuteAsync(sql, parameters);
            
            // Fetch the updated record
            var doctor = await _connection.QuerySingleAsync<Doctor>(
                "SELECT doctor_id as DoctorId, doctor_name as DoctorName, contact_no as ContactNo, email as Email, " +
                "specialization as Specialization, dob as Dob, doa as Doa, identifier as Identifier, " +
                "registration_number as RegistrationNumber, location as Location, " +
                "remarks as Remarks, is_active as IsActive FROM doctors WHERE doctor_id = @Id",
                new { Id = id });
            var response = new
            {
                message = "Doctor record updated successfully",
                data = MapToDto(doctor)
            };
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating doctor with ID {DoctorId}", id);
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDoctor(int id)
    {
        try
        {
            var sql = "DELETE FROM doctors WHERE doctor_id = @Id";
            var rowsAffected = await _connection.ExecuteAsync(sql, new { Id = id });

            if (rowsAffected == 0)
            {
                return NotFound(new { message = $"Doctor with ID {id} not found." });
            }

            return Ok(new { message = "Doctor record deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting doctor with ID {DoctorId}", id);
            return StatusCode(500, new { message = ex.Message });
        }
    }

    private static DoctorResponseDto MapToDto(Doctor doctor) => new()
    {
        DoctorId = doctor.DoctorId,
        DoctorName = doctor.DoctorName,
        ContactNo = doctor.ContactNo,
        Email = doctor.Email,
        Specialization = doctor.Specialization,
        Dob = doctor.Dob?.ToString("yyyy-MM-dd"),
        Doa = doctor.Doa?.ToString("yyyy-MM-dd"),
        Identifier = doctor.Identifier,
        RegistrationNumber = doctor.RegistrationNumber,
        Location = doctor.Location,
        Remarks = doctor.Remarks,
        IsActive = doctor.IsActive
    };
}
