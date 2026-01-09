using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Dapper;
using NehaSurgicalAPI.Models;
using NehaSurgicalAPI.DTOs;

namespace NehaSurgicalAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PermissionsController : ControllerBase
{
    private readonly NpgsqlConnection _connection;

    public PermissionsController(NpgsqlConnection connection)
    {
        _connection = connection;
    }

    // GET: api/Permissions
    [HttpGet]
    public async Task<IActionResult> GetAllPermissions([FromQuery] string? isActive = null)
    {
        try
        {
            var sql = @"SELECT 
                permission_id as PermissionId,
                code as Code,
                name as Name,
                module as Module,
                description as Description,
                is_active as IsActive,
                created_at as CreatedAt,
                updated_at as UpdatedAt
                FROM Permissions";

            if (!string.IsNullOrEmpty(isActive))
            {
                sql += " WHERE is_active = @IsActive";
            }

            sql += " ORDER BY module, permission_id";

            var permissions = await _connection.QueryAsync<Permission>(sql, new { IsActive = isActive });
            var permissionDtos = permissions.Select(p => new PermissionDto
            {
                PermissionId = p.PermissionId,
                Code = p.Code,
                Name = p.Name,
                Module = p.Module,
                Description = p.Description
            }).ToList();

            return Ok(new { message = "Permissions retrieved successfully", data = permissionDtos });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // GET: api/Permissions/grouped
    [HttpGet("grouped")]
    public async Task<IActionResult> GetPermissionsByModule()
    {
        try
        {
            var sql = @"SELECT 
                permission_id as PermissionId,
                code as Code,
                name as Name,
                module as Module,
                description as Description
                FROM Permissions
                WHERE is_active = 'Y'
                ORDER BY module, permission_id";

            var permissions = await _connection.QueryAsync<Permission>(sql);

            var grouped = permissions
                .GroupBy(p => p.Module)
                .Select(g => new PermissionsByModuleDto
                {
                    Module = g.Key,
                    Permissions = g.Select(p => new PermissionDto
                    {
                        PermissionId = p.PermissionId,
                        Code = p.Code,
                        Name = p.Name,
                        Module = p.Module,
                        Description = p.Description
                    }).ToList()
                }).ToList();

            return Ok(new { message = "Permissions grouped by module retrieved successfully", data = grouped });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // GET: api/Permissions/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPermissionById(int id)
    {
        try
        {
            var sql = @"SELECT 
                permission_id as PermissionId,
                code as Code,
                name as Name,
                module as Module,
                description as Description
                FROM Permissions 
                WHERE permission_id = @PermissionId";

            var permission = await _connection.QueryFirstOrDefaultAsync<Permission>(sql, new { PermissionId = id });

            if (permission == null)
            {
                return NotFound(new { message = $"Permission with ID {id} not found" });
            }

            return Ok(new { message = "Permission retrieved successfully", data = new PermissionDto
            {
                PermissionId = permission.PermissionId,
                Code = permission.Code,
                Name = permission.Name,
                Module = permission.Module,
                Description = permission.Description
            }});
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
