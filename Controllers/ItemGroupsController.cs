using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Dapper;
using NehaSurgicalAPI.Models;
using NehaSurgicalAPI.DTOs;

namespace NehaSurgicalAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemGroupsController : ControllerBase
{
    private readonly NpgsqlConnection _connection;

    public ItemGroupsController(NpgsqlConnection connection)
    {
        _connection = connection;
    }

    // GET: api/ItemGroups?isActive=Y
    [HttpGet]
    public async Task<IActionResult> GetAllItemGroups([FromQuery] string? isActive = null)
    {
        try
        {
            var sql = @"SELECT 
                item_group_id as ItemGroupId,
                name as Name,
                description as Description,
                is_active as IsActive,
                created_at as CreatedAt,
                updated_at as UpdatedAt
                FROM ItemGroups";

            // Add WHERE clause if isActive is specified
            if (!string.IsNullOrEmpty(isActive))
            {
                sql += " WHERE is_active = @IsActive";
            }

            sql += " ORDER BY item_group_id DESC";

            var itemGroups = await _connection.QueryAsync<ItemGroup>(sql, new { IsActive = isActive });
            var itemGroupDtos = itemGroups.Select(ig => MapToDto(ig)).ToList();

            return Ok(new { message = "Item groups retrieved successfully", data = itemGroupDtos });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // GET: api/ItemGroups/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetItemGroupById(int id)
    {
        try
        {
            var sql = @"SELECT 
                item_group_id as ItemGroupId,
                name as Name,
                description as Description,
                is_active as IsActive,
                created_at as CreatedAt,
                updated_at as UpdatedAt
                FROM ItemGroups 
                WHERE item_group_id = @ItemGroupId";

            var itemGroup = await _connection.QueryFirstOrDefaultAsync<ItemGroup>(sql, new { ItemGroupId = id });

            if (itemGroup == null)
            {
                return NotFound(new { message = $"Item group with ID {id} not found" });
            }

            return Ok(new { message = "Item group retrieved successfully", data = MapToDto(itemGroup) });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // POST: api/ItemGroups
    [HttpPost]
    public async Task<IActionResult> CreateItemGroup([FromBody] CreateItemGroupDto itemGroupDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var sql = @"INSERT INTO ItemGroups (name, description, is_active, created_at, updated_at) 
                        VALUES (@Name, @Description, @IsActive, NOW(), NOW())
                        RETURNING item_group_id as ItemGroupId,
                                  name as Name,
                                  description as Description,
                                  is_active as IsActive,
                                  created_at as CreatedAt,
                                  updated_at as UpdatedAt";

            var itemGroup = await _connection.QueryFirstAsync<ItemGroup>(sql, itemGroupDto);

            return CreatedAtAction(nameof(GetItemGroupById), new { id = itemGroup.ItemGroupId }, 
                new { message = "Item group created successfully", data = MapToDto(itemGroup) });
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "23505")
        {
            return Conflict(new { message = "An item group with this name already exists" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // PUT: api/ItemGroups/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateItemGroup(int id, [FromBody] UpdateItemGroupDto itemGroupDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var sql = @"UPDATE ItemGroups 
                        SET name = @Name, 
                            description = @Description, 
                            is_active = @IsActive,
                            updated_at = NOW()
                        WHERE item_group_id = @ItemGroupId
                        RETURNING item_group_id as ItemGroupId,
                                  name as Name,
                                  description as Description,
                                  is_active as IsActive,
                                  created_at as CreatedAt,
                                  updated_at as UpdatedAt";

            var itemGroup = await _connection.QueryFirstOrDefaultAsync<ItemGroup>(sql, 
                new { 
                    ItemGroupId = id, 
                    itemGroupDto.Name, 
                    itemGroupDto.Description, 
                    itemGroupDto.IsActive 
                });

            if (itemGroup == null)
            {
                return NotFound(new { message = $"Item group with ID {id} not found" });
            }

            return Ok(new { message = "Item group updated successfully", data = MapToDto(itemGroup) });
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "23505")
        {
            return Conflict(new { message = "An item group with this name already exists" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // DELETE: api/ItemGroups/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteItemGroup(int id)
    {
        try
        {
            var sql = "DELETE FROM ItemGroups WHERE item_group_id = @ItemGroupId";
            var rowsAffected = await _connection.ExecuteAsync(sql, new { ItemGroupId = id });

            if (rowsAffected == 0)
            {
                return NotFound(new { message = $"Item group with ID {id} not found" });
            }

            return Ok(new { message = "Item group deleted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    private ItemGroupDto MapToDto(ItemGroup itemGroup)
    {
        return new ItemGroupDto
        {
            ItemGroupId = itemGroup.ItemGroupId,
            Name = itemGroup.Name,
            Description = itemGroup.Description,
            IsActive = itemGroup.IsActive
        };
    }
}
