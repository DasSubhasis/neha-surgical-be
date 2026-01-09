using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Dapper;
using NehaSurgicalAPI.Models;
using NehaSurgicalAPI.DTOs;

namespace NehaSurgicalAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SpecificationsController : ControllerBase
{
    private readonly NpgsqlConnection _connection;

    public SpecificationsController(NpgsqlConnection connection)
    {
        _connection = connection;
    }

    // GET: api/Specifications?isActive=Y
    [HttpGet]
    public async Task<IActionResult> GetAllSpecifications([FromQuery] string? isActive = null)
    {
        try
        {
            var sql = @"SELECT 
                specification_id as SpecificationId,
                name as Name,
                is_active as IsActive,
                created_at as CreatedAt,
                updated_at as UpdatedAt
                FROM Specifications";

            // Add WHERE clause if isActive is specified
            if (!string.IsNullOrEmpty(isActive))
            {
                sql += " WHERE is_active = @IsActive";
            }

            sql += " ORDER BY specification_id DESC";

            var specifications = await _connection.QueryAsync<Specification>(sql, new { IsActive = isActive });
            var specificationDtos = specifications.Select(s => MapToDto(s)).ToList();

            return Ok(new { message = "Specifications retrieved successfully", data = specificationDtos });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // GET: api/Specifications/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetSpecificationById(int id)
    {
        try
        {
            var sql = @"SELECT 
                specification_id as SpecificationId,
                name as Name,
                is_active as IsActive,
                created_at as CreatedAt,
                updated_at as UpdatedAt
                FROM Specifications 
                WHERE specification_id = @SpecificationId";

            var specification = await _connection.QueryFirstOrDefaultAsync<Specification>(sql, new { SpecificationId = id });

            if (specification == null)
            {
                return NotFound(new { message = $"Specification with ID {id} not found" });
            }

            return Ok(new { message = "Specification retrieved successfully", data = MapToDto(specification) });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // POST: api/Specifications
    [HttpPost]
    public async Task<IActionResult> CreateSpecification([FromBody] CreateSpecificationDto specificationDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var sql = @"INSERT INTO Specifications (name, is_active, created_at, updated_at) 
                        VALUES (@Name, @IsActive, NOW(), NOW())
                        RETURNING specification_id as SpecificationId,
                                  name as Name,
                                  is_active as IsActive,
                                  created_at as CreatedAt,
                                  updated_at as UpdatedAt";

            var specification = await _connection.QueryFirstAsync<Specification>(sql, specificationDto);

            return CreatedAtAction(nameof(GetSpecificationById), new { id = specification.SpecificationId }, 
                new { message = "Specification created successfully", data = MapToDto(specification) });
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "23505")
        {
            return Conflict(new { message = "A specification with this name already exists" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // PUT: api/Specifications/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSpecification(int id, [FromBody] UpdateSpecificationDto specificationDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var sql = @"UPDATE Specifications 
                        SET name = @Name, 
                            is_active = @IsActive,
                            updated_at = NOW()
                        WHERE specification_id = @SpecificationId
                        RETURNING specification_id as SpecificationId,
                                  name as Name,
                                  is_active as IsActive,
                                  created_at as CreatedAt,
                                  updated_at as UpdatedAt";

            var specification = await _connection.QueryFirstOrDefaultAsync<Specification>(sql, 
                new { 
                    SpecificationId = id, 
                    specificationDto.Name, 
                    specificationDto.IsActive 
                });

            if (specification == null)
            {
                return NotFound(new { message = $"Specification with ID {id} not found" });
            }

            return Ok(new { message = "Specification updated successfully", data = MapToDto(specification) });
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "23505")
        {
            return Conflict(new { message = "A specification with this name already exists" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // DELETE: api/Specifications/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSpecification(int id)
    {
        try
        {
            var sql = "DELETE FROM Specifications WHERE specification_id = @SpecificationId";
            var rowsAffected = await _connection.ExecuteAsync(sql, new { SpecificationId = id });

            if (rowsAffected == 0)
            {
                return NotFound(new { message = $"Specification with ID {id} not found" });
            }

            return Ok(new { message = "Specification deleted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    private SpecificationDto MapToDto(Specification specification)
    {
        return new SpecificationDto
        {
            SpecificationId = specification.SpecificationId,
            Name = specification.Name,
            IsActive = specification.IsActive,
            CreatedAt = specification.CreatedAt
        };
    }
}
