using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Dapper;
using NehaSurgicalAPI.Models;
using NehaSurgicalAPI.DTOs;

namespace NehaSurgicalAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly NpgsqlConnection _connection;

    public CategoriesController(NpgsqlConnection connection)
    {
        _connection = connection;
    }

    // GET: api/Categories?isActive=Y
    [HttpGet]
    public async Task<IActionResult> GetAllCategories([FromQuery] string? isActive = null)
    {
        try
        {
            var sql = @"SELECT 
                category_id as CategoryId,
                name as Name,
                is_active as IsActive,
                created_at as CreatedAt,
                updated_at as UpdatedAt
                FROM Categories";

            // Add WHERE clause if isActive is specified
            if (!string.IsNullOrEmpty(isActive))
            {
                sql += " WHERE is_active = @IsActive";
            }

            sql += " ORDER BY category_id DESC";

            var categories = await _connection.QueryAsync<Category>(sql, new { IsActive = isActive });
            var categoryDtos = categories.Select(c => MapToDto(c)).ToList();

            return Ok(new { message = "Categories retrieved successfully", data = categoryDtos });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // GET: api/Categories/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategoryById(int id)
    {
        try
        {
            var sql = @"SELECT 
                category_id as CategoryId,
                name as Name,
                is_active as IsActive,
                created_at as CreatedAt,
                updated_at as UpdatedAt
                FROM Categories 
                WHERE category_id = @CategoryId";

            var category = await _connection.QueryFirstOrDefaultAsync<Category>(sql, new { CategoryId = id });

            if (category == null)
            {
                return NotFound(new { message = $"Category with ID {id} not found" });
            }

            return Ok(new { message = "Category retrieved successfully", data = MapToDto(category) });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // POST: api/Categories
    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto categoryDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var sql = @"INSERT INTO Categories (name, is_active, created_at, updated_at) 
                        VALUES (@Name, @IsActive, NOW(), NOW())
                        RETURNING category_id as CategoryId,
                                  name as Name,
                                  is_active as IsActive,
                                  created_at as CreatedAt,
                                  updated_at as UpdatedAt";

            var category = await _connection.QueryFirstAsync<Category>(sql, categoryDto);

            return CreatedAtAction(nameof(GetCategoryById), new { id = category.CategoryId }, 
                new { message = "Category created successfully", data = MapToDto(category) });
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "23505")
        {
            return Conflict(new { message = "A category with this name already exists" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // PUT: api/Categories/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDto categoryDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var sql = @"UPDATE Categories 
                        SET name = @Name, 
                            is_active = @IsActive,
                            updated_at = NOW()
                        WHERE category_id = @CategoryId
                        RETURNING category_id as CategoryId,
                                  name as Name,
                                  is_active as IsActive,
                                  created_at as CreatedAt,
                                  updated_at as UpdatedAt";

            var category = await _connection.QueryFirstOrDefaultAsync<Category>(sql, 
                new { 
                    CategoryId = id, 
                    categoryDto.Name, 
                    categoryDto.IsActive 
                });

            if (category == null)
            {
                return NotFound(new { message = $"Category with ID {id} not found" });
            }

            return Ok(new { message = "Category updated successfully", data = MapToDto(category) });
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "23505")
        {
            return Conflict(new { message = "A category with this name already exists" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // DELETE: api/Categories/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        try
        {
            var sql = "DELETE FROM Categories WHERE category_id = @CategoryId";
            var rowsAffected = await _connection.ExecuteAsync(sql, new { CategoryId = id });

            if (rowsAffected == 0)
            {
                return NotFound(new { message = $"Category with ID {id} not found" });
            }

            return Ok(new { message = "Category deleted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    private CategoryDto MapToDto(Category category)
    {
        return new CategoryDto
        {
            CategoryId = category.CategoryId,
            Name = category.Name,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt
        };
    }
}
