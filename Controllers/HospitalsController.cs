using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Dapper;
using NehaSurgicalAPI.Models;
using NehaSurgicalAPI.DTOs;

namespace NehaSurgicalAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HospitalsController : ControllerBase
{
    private readonly NpgsqlConnection _connection;

    public HospitalsController(NpgsqlConnection connection)
    {
        _connection = connection;
    }

    // GET: api/Hospitals?isActive=Y
    [HttpGet]
    public async Task<IActionResult> GetAllHospitals([FromQuery] string? isActive = null)
    {
        try
        {
            var sql = @"SELECT 
                hospital_id as HospitalId,
                name as Name,
                address as Address,
                contact_person as ContactPerson,
                contact_no as ContactNo,
                email as Email,
                is_active as IsActive,
                created_at as CreatedAt,
                updated_at as UpdatedAt
                FROM Hospitals";

            // Add WHERE clause if isActive is specified
            if (!string.IsNullOrEmpty(isActive))
            {
                sql += " WHERE is_active = @IsActive";
            }

            sql += " ORDER BY hospital_id DESC";

            var hospitals = (await _connection.QueryAsync<Hospital>(sql, new { IsActive = isActive })).ToList();
            var hospitalDtos = new List<HospitalDto>();
            foreach (var hospital in hospitals)
            {
                var contacts = await _connection.QueryAsync<HospitalContact>(
                    "SELECT * FROM HospitalContacts WHERE hospital_id = @HospitalId ORDER BY contact_id",
                    new { HospitalId = hospital.HospitalId });
                hospitalDtos.Add(MapToDto(hospital, contacts.ToList()));
            }
            return Ok(new { message = "Hospitals retrieved successfully", data = hospitalDtos });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // GET: api/Hospitals/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetHospitalById(int id)
    {
        try
        {
            var sql = @"SELECT 
                hospital_id as HospitalId,
                name as Name,
                address as Address,
                contact_person as ContactPerson,
                contact_no as ContactNo,
                email as Email,
                is_active as IsActive,
                created_at as CreatedAt,
                updated_at as UpdatedAt
                FROM Hospitals 
                WHERE hospital_id = @HospitalId";

            var hospital = await _connection.QueryFirstOrDefaultAsync<Hospital>(sql, new { HospitalId = id });
            if (hospital == null)
            {
                return NotFound(new { message = $"Hospital with ID {id} not found" });
            }
            var contacts = await _connection.QueryAsync<HospitalContact>(
                "SELECT * FROM HospitalContacts WHERE hospital_id = @HospitalId ORDER BY contact_id",
                new { HospitalId = id });
            return Ok(new { message = "Hospital retrieved successfully", data = MapToDto(hospital, contacts.ToList()) });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // POST: api/Hospitals
    [HttpPost]
    public async Task<IActionResult> CreateHospital([FromBody] CreateHospitalDto hospitalDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var sql = @"INSERT INTO Hospitals (name, address, contact_person, contact_no, email, is_active, created_at, updated_at) 
                        VALUES (@Name, @Address, @ContactPerson, @ContactNo, @Email, @IsActive, NOW(), NOW())
                        RETURNING hospital_id as HospitalId,
                                  name as Name,
                                  address as Address,
                                  contact_person as ContactPerson,
                                  contact_no as ContactNo,
                                  email as Email,
                                  is_active as IsActive,
                                  created_at as CreatedAt,
                                  updated_at as UpdatedAt";

            var hospital = await _connection.QueryFirstAsync<Hospital>(sql, hospitalDto);

            // Insert contacts if provided
            if (hospitalDto.Contacts != null && hospitalDto.Contacts.Count > 0)
            {
                foreach (var contact in hospitalDto.Contacts)
                {
                    var contactSql = @"INSERT INTO HospitalContacts (hospital_id, name, mobile, email, location, department, remarks, created_at, updated_at)
                        VALUES (@HospitalId, @Name, @Mobile, @Email, @Location, @Department, @Remarks, NOW(), NOW())";
                    await _connection.ExecuteAsync(contactSql, new
                    {
                        HospitalId = hospital.HospitalId,
                        contact.Name,
                        contact.Mobile,
                        contact.Email,
                        contact.Location,
                        contact.Department,
                        contact.Remarks
                    });
                }
            }
            var contacts = await _connection.QueryAsync<HospitalContact>(
                "SELECT * FROM HospitalContacts WHERE hospital_id = @HospitalId ORDER BY contact_id",
                new { HospitalId = hospital.HospitalId });
            return CreatedAtAction(nameof(GetHospitalById), new { id = hospital.HospitalId },
                new { message = "Hospital created successfully", data = MapToDto(hospital, contacts.ToList()) });
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "23505")
        {
            return Conflict(new { message = "A hospital with this name already exists" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // PUT: api/Hospitals/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateHospital(int id, [FromBody] UpdateHospitalDto hospitalDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var sql = @"UPDATE Hospitals 
                        SET name = @Name, 
                            address = @Address, 
                            contact_person = @ContactPerson, 
                            contact_no = @ContactNo, 
                            email = @Email, 
                            is_active = @IsActive,
                            updated_at = NOW()
                        WHERE hospital_id = @HospitalId
                        RETURNING hospital_id as HospitalId,
                                  name as Name,
                                  address as Address,
                                  contact_person as ContactPerson,
                                  contact_no as ContactNo,
                                  email as Email,
                                  is_active as IsActive,
                                  created_at as CreatedAt,
                                  updated_at as UpdatedAt";

            var hospital = await _connection.QueryFirstOrDefaultAsync<Hospital>(sql,
                new {
                    HospitalId = id,
                    hospitalDto.Name,
                    hospitalDto.Address,
                    hospitalDto.ContactPerson,
                    hospitalDto.ContactNo,
                    hospitalDto.Email,
                    hospitalDto.IsActive
                });

            if (hospital == null)
            {
                return NotFound(new { message = $"Hospital with ID {id} not found" });
            }

            // Update contacts if provided
            if (hospitalDto.Contacts != null)
            {
                // Remove all existing contacts for this hospital
                await _connection.ExecuteAsync("DELETE FROM HospitalContacts WHERE hospital_id = @HospitalId", new { HospitalId = id });
                // Insert new contacts
                foreach (var contact in hospitalDto.Contacts)
                {
                    var contactSql = @"INSERT INTO HospitalContacts (hospital_id, name, mobile, email, location, department, remarks, created_at, updated_at)
                        VALUES (@HospitalId, @Name, @Mobile, @Email, @Location, @Department, @Remarks, NOW(), NOW())";
                    await _connection.ExecuteAsync(contactSql, new
                    {
                        HospitalId = id,
                        contact.Name,
                        contact.Mobile,
                        contact.Email,
                        contact.Location,
                        contact.Department,
                        contact.Remarks
                    });
                }
            }
            var contacts = await _connection.QueryAsync<HospitalContact>(
                "SELECT * FROM HospitalContacts WHERE hospital_id = @HospitalId ORDER BY contact_id",
                new { HospitalId = id });
            return Ok(new { message = "Hospital updated successfully", data = MapToDto(hospital, contacts.ToList()) });
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "23505")
        {
            return Conflict(new { message = "A hospital with this name already exists" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // DELETE: api/Hospitals/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteHospital(int id)
    {
        try
        {
            var sql = "DELETE FROM Hospitals WHERE hospital_id = @HospitalId";
            var rowsAffected = await _connection.ExecuteAsync(sql, new { HospitalId = id });

            if (rowsAffected == 0)
            {
                return NotFound(new { message = $"Hospital with ID {id} not found" });
            }

            return Ok(new { message = "Hospital deleted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    private HospitalDto MapToDto(Hospital hospital, List<HospitalContact>? contacts = null)
    {
        return new HospitalDto
        {
            HospitalId = hospital.HospitalId,
            Name = hospital.Name,
            Address = hospital.Address,
            ContactPerson = hospital.ContactPerson,
            ContactNo = hospital.ContactNo,
            Email = hospital.Email,
            IsActive = hospital.IsActive,
            Contacts = contacts?.Select(c => new HospitalContactDto
            {
                ContactId = c.ContactId,
                Name = c.Name,
                Mobile = c.Mobile,
                Email = c.Email,
                Location = c.Location,
                Department = c.Department,
                Remarks = c.Remarks
            }).ToList() ?? new List<HospitalContactDto>()
        };
    }
}
