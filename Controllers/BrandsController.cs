using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Dapper;
using NehaSurgicalAPI.Models;
using NehaSurgicalAPI.DTOs;

namespace NehaSurgicalAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BrandsController : ControllerBase
{
    private readonly NpgsqlConnection _connection;

    public BrandsController(NpgsqlConnection connection)
    {
        _connection = connection;
    }

    // GET: api/Brands?isActive=Y
    [HttpGet]
    public async Task<IActionResult> GetAllBrands([FromQuery] string? isActive = null)
    {
        try
        {
            var sql = @"SELECT 
                brand_id as BrandId,
                name as Name,
                is_active as IsActive,
                created_at as CreatedAt,
                updated_at as UpdatedAt
                FROM Brands";

            // Add WHERE clause if isActive is specified
            if (!string.IsNullOrEmpty(isActive))
            {
                sql += " WHERE is_active = @IsActive";
            }

            sql += " ORDER BY brand_id DESC";

            var brands = await _connection.QueryAsync<Brand>(sql, new { IsActive = isActive });
            var brandDtos = brands.Select(b => MapToDto(b)).ToList();

            return Ok(new { message = "Brands retrieved successfully", data = brandDtos });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // GET: api/Brands/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetBrandById(int id)
    {
        try
        {
            var sql = @"SELECT 
                brand_id as BrandId,
                name as Name,
                is_active as IsActive,
                created_at as CreatedAt,
                updated_at as UpdatedAt
                FROM Brands 
                WHERE brand_id = @BrandId";

            var brand = await _connection.QueryFirstOrDefaultAsync<Brand>(sql, new { BrandId = id });

            if (brand == null)
            {
                return NotFound(new { message = $"Brand with ID {id} not found" });
            }

            return Ok(new { message = "Brand retrieved successfully", data = MapToDto(brand) });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // POST: api/Brands
    [HttpPost]
    public async Task<IActionResult> CreateBrand([FromBody] CreateBrandDto brandDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var sql = @"INSERT INTO Brands (name, is_active, created_at, updated_at) 
                        VALUES (@Name, @IsActive, NOW(), NOW())
                        RETURNING brand_id as BrandId,
                                  name as Name,
                                  is_active as IsActive,
                                  created_at as CreatedAt,
                                  updated_at as UpdatedAt";

            var brand = await _connection.QueryFirstAsync<Brand>(sql, brandDto);

            return CreatedAtAction(nameof(GetBrandById), new { id = brand.BrandId }, 
                new { message = "Brand created successfully", data = MapToDto(brand) });
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "23505")
        {
            return Conflict(new { message = "A brand with this name already exists" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // PUT: api/Brands/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBrand(int id, [FromBody] UpdateBrandDto brandDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Normalize isActive to 'Y'/'N'
            if (!string.IsNullOrEmpty(brandDto.IsActive))
            {
                if (brandDto.IsActive.Equals("true", StringComparison.OrdinalIgnoreCase) || brandDto.IsActive == "1")
                    brandDto.IsActive = "Y";
                else if (brandDto.IsActive.Equals("false", StringComparison.OrdinalIgnoreCase) || brandDto.IsActive == "0")
                    brandDto.IsActive = "N";
                else
                    brandDto.IsActive = brandDto.IsActive.ToUpper();
            }

            var sql = @"UPDATE Brands 
                        SET name = @Name, 
                            is_active = @IsActive,
                            updated_at = NOW()
                        WHERE brand_id = @BrandId
                        RETURNING brand_id as BrandId,
                                  name as Name,
                                  is_active as IsActive,
                                  created_at as CreatedAt,
                                  updated_at as UpdatedAt";

            var brand = await _connection.QueryFirstOrDefaultAsync<Brand>(sql, 
                new { 
                    BrandId = id, 
                    brandDto.Name, 
                    brandDto.IsActive 
                });

            if (brand == null)
            {
                return NotFound(new { message = $"Brand with ID {id} not found" });
            }

            return Ok(new { message = "Brand updated successfully", data = MapToDto(brand) });
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "23505")
        {
            return Conflict(new { message = "A brand with this name already exists" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // DELETE: api/Brands/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBrand(int id)
    {
        try
        {
            var sql = "DELETE FROM Brands WHERE brand_id = @BrandId";
            var rowsAffected = await _connection.ExecuteAsync(sql, new { BrandId = id });

            if (rowsAffected == 0)
            {
                return NotFound(new { message = $"Brand with ID {id} not found" });
            }

            return Ok(new { message = "Brand deleted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    private BrandDto MapToDto(Brand brand)
    {
        return new BrandDto
        {
            BrandId = brand.BrandId,
            Name = brand.Name,
            IsActive = brand.IsActive,
            CreatedAt = brand.CreatedAt
        };
    }
}
