using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Dapper;
using NehaSurgicalAPI.Models;
using NehaSurgicalAPI.DTOs;

namespace NehaSurgicalAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SizesController : ControllerBase
{
    private readonly NpgsqlConnection _connection;

    public SizesController(NpgsqlConnection connection)
    {
        _connection = connection;
    }

    // GET: api/Sizes?isActive=Y
    [HttpGet]
    public async Task<IActionResult> GetAllSizes([FromQuery] string? isActive = null)
    {
        try
        {
            var sql = @"SELECT 
                size_id as SizeId,
                name as Name,
                is_active as IsActive,
                created_at as CreatedAt,
                updated_at as UpdatedAt
                FROM Sizes";

            // Add WHERE clause if isActive is specified
            if (!string.IsNullOrEmpty(isActive))
            {
                sql += " WHERE is_active = @IsActive";
            }

            sql += " ORDER BY size_id DESC";

            var sizes = await _connection.QueryAsync<Size>(sql, new { IsActive = isActive });
            var sizeDtos = sizes.Select(s => MapToDto(s)).ToList();

            return Ok(new { message = "Sizes retrieved successfully", data = sizeDtos });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // GET: api/Sizes/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetSizeById(int id)
    {
        try
        {
            var sql = @"SELECT 
                size_id as SizeId,
                name as Name,
                is_active as IsActive,
                created_at as CreatedAt,
                updated_at as UpdatedAt
                FROM Sizes 
                WHERE size_id = @SizeId";

            var size = await _connection.QueryFirstOrDefaultAsync<Size>(sql, new { SizeId = id });

            if (size == null)
            {
                return NotFound(new { message = $"Size with ID {id} not found" });
            }

            return Ok(new { message = "Size retrieved successfully", data = MapToDto(size) });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // POST: api/Sizes
    [HttpPost]
    public async Task<IActionResult> CreateSize([FromBody] CreateSizeDto sizeDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var sql = @"INSERT INTO Sizes (name, is_active, created_at, updated_at) 
                        VALUES (@Name, @IsActive, NOW(), NOW())
                        RETURNING size_id as SizeId,
                                  name as Name,
                                  is_active as IsActive,
                                  created_at as CreatedAt,
                                  updated_at as UpdatedAt";

            var size = await _connection.QueryFirstAsync<Size>(sql, sizeDto);

            return CreatedAtAction(nameof(GetSizeById), new { id = size.SizeId }, 
                new { message = "Size created successfully", data = MapToDto(size) });
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "23505")
        {
            return Conflict(new { message = "A size with this name already exists" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // PUT: api/Sizes/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSize(int id, [FromBody] UpdateSizeDto sizeDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var sql = @"UPDATE Sizes 
                        SET name = @Name, 
                            is_active = @IsActive,
                            updated_at = NOW()
                        WHERE size_id = @SizeId
                        RETURNING size_id as SizeId,
                                  name as Name,
                                  is_active as IsActive,
                                  created_at as CreatedAt,
                                  updated_at as UpdatedAt";

            var size = await _connection.QueryFirstOrDefaultAsync<Size>(sql, 
                new { 
                    SizeId = id, 
                    sizeDto.Name, 
                    sizeDto.IsActive 
                });

            if (size == null)
            {
                return NotFound(new { message = $"Size with ID {id} not found" });
            }

            return Ok(new { message = "Size updated successfully", data = MapToDto(size) });
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "23505")
        {
            return Conflict(new { message = "A size with this name already exists" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // DELETE: api/Sizes/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSize(int id)
    {
        try
        {
            var sql = "DELETE FROM Sizes WHERE size_id = @SizeId";
            var rowsAffected = await _connection.ExecuteAsync(sql, new { SizeId = id });

            if (rowsAffected == 0)
            {
                return NotFound(new { message = $"Size with ID {id} not found" });
            }

            return Ok(new { message = "Size deleted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    private SizeDto MapToDto(Size size)
    {
        return new SizeDto
        {
            SizeId = size.SizeId,
            Name = size.Name,
            IsActive = size.IsActive,
            CreatedAt = size.CreatedAt
        };
    }
}
