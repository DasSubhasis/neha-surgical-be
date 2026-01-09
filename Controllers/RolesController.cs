using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Dapper;
using NehaSurgicalAPI.Models;
using NehaSurgicalAPI.DTOs;

namespace NehaSurgicalAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly NpgsqlConnection _connection;

    public RolesController(NpgsqlConnection connection)
    {
        _connection = connection;
    }

    // GET: api/Roles?isActive=Y
    [HttpGet]
    public async Task<IActionResult> GetAllRoles([FromQuery] string? isActive = null)
    {
        try
        {
            var sql = @"SELECT 
                role_id as RoleId,
                role_name as RoleName,
                description as Description,
                is_active as IsActive,
                created_at as CreatedAt,
                updated_at as UpdatedAt
                FROM Roles";

            if (!string.IsNullOrEmpty(isActive))
            {
                sql += " WHERE is_active = @IsActive";
            }

            sql += " ORDER BY role_id";

            var roles = await _connection.QueryAsync<Role>(sql, new { IsActive = isActive });

            var result = new List<RoleWithPermissionsDto>();

            foreach (var role in roles)
            {
                var permissionsSql = @"SELECT p.permission_id as PermissionId
                    FROM RolePermissions rp
                    INNER JOIN Permissions p ON rp.permission_id = p.permission_id
                    WHERE rp.role_id = @RoleId";

                var permissionIds = await _connection.QueryAsync<int>(permissionsSql, new { RoleId = role.RoleId });

                result.Add(new RoleWithPermissionsDto
                {
                    RoleId = role.RoleId,
                    Name = role.RoleName,
                    Description = role.Description,
                    Permissions = permissionIds.ToList(),
                    IsActive = role.IsActive,
                    CreatedAt = role.CreatedAt
                });
            }

            return Ok(new { message = "Roles retrieved successfully", data = result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // GET: api/Roles/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetRoleById(int id)
    {
        try
        {
            var sql = @"SELECT 
                role_id as RoleId,
                role_name as RoleName,
                description as Description,
                is_active as IsActive,
                created_at as CreatedAt,
                updated_at as UpdatedAt
                FROM Roles WHERE role_id = @RoleId";

            var role = await _connection.QueryFirstOrDefaultAsync<Role>(sql, new { RoleId = id });

            if (role == null)
            {
                return NotFound(new { message = $"Role with ID {id} not found" });
            }

            // Get permissions for this role
            var permissionsSql = @"SELECT 
                p.permission_id as PermissionId,
                p.code as Code,
                p.name as Name,
                p.module as Module,
                p.description as Description
                FROM RolePermissions rp
                INNER JOIN Permissions p ON rp.permission_id = p.permission_id
                WHERE rp.role_id = @RoleId
                ORDER BY p.module, p.permission_id";

            var permissions = await _connection.QueryAsync<Permission>(permissionsSql, new { RoleId = id });

            var result = new RoleWithPermissionsDto
            {
                RoleId = role.RoleId,
                Name = role.RoleName,
                Description = role.Description,
                Permissions = permissions.Select(p => p.PermissionId).ToList(),
                PermissionDetails = permissions.Select(p => new PermissionDto
                {
                    PermissionId = p.PermissionId,
                    Code = p.Code,
                    Name = p.Name,
                    Module = p.Module,
                    Description = p.Description
                }).ToList(),
                IsActive = role.IsActive,
                CreatedAt = role.CreatedAt
            };

            return Ok(new { message = "Role retrieved successfully", data = result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // POST: api/Roles
    [HttpPost]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleWithPermissionsDto roleDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Insert role
            var sql = @"INSERT INTO Roles (role_name, description, is_active, created_at, updated_at) 
                        VALUES (@Name, @Description, @IsActive, NOW(), NOW())
                        RETURNING role_id as RoleId, role_name as RoleName, description as Description, is_active as IsActive, created_at as CreatedAt";

            var role = await _connection.QueryFirstAsync<Role>(sql, roleDto);

            // Insert role permissions
            if (roleDto.Permissions.Any())
            {
                var permissionSql = @"INSERT INTO RolePermissions (role_id, permission_id, created_at) 
                                      VALUES (@RoleId, @PermissionId, NOW())";

                foreach (var permissionId in roleDto.Permissions)
                {
                    await _connection.ExecuteAsync(permissionSql, new { RoleId = role.RoleId, PermissionId = permissionId });
                }
            }

            var result = new RoleWithPermissionsDto
            {
                RoleId = role.RoleId,
                Name = role.RoleName,
                Description = role.Description,
                Permissions = roleDto.Permissions,
                IsActive = role.IsActive,
                CreatedAt = role.CreatedAt
            };

            return CreatedAtAction(nameof(GetRoleById), new { id = role.RoleId }, 
                new { message = "Role created successfully", data = result });
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "23505")
        {
            return Conflict(new { message = "A role with this name already exists" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // PUT: api/Roles/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleWithPermissionsDto roleDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Update role
            var sql = @"UPDATE Roles 
                        SET role_name = @Name, description = @Description, is_active = @IsActive, updated_at = NOW()
                        WHERE role_id = @RoleId
                        RETURNING role_id as RoleId, role_name as RoleName, description as Description, is_active as IsActive, created_at as CreatedAt";

            var role = await _connection.QueryFirstOrDefaultAsync<Role>(sql, 
                new { RoleId = id, roleDto.Name, roleDto.Description, roleDto.IsActive });

            if (role == null)
            {
                return NotFound(new { message = $"Role with ID {id} not found" });
            }

            // Delete existing permissions and insert new ones
            await _connection.ExecuteAsync("DELETE FROM RolePermissions WHERE role_id = @RoleId", new { RoleId = id });

            if (roleDto.Permissions.Any())
            {
                var permissionSql = @"INSERT INTO RolePermissions (role_id, permission_id, created_at) 
                                      VALUES (@RoleId, @PermissionId, NOW())";

                foreach (var permissionId in roleDto.Permissions)
                {
                    await _connection.ExecuteAsync(permissionSql, new { RoleId = id, PermissionId = permissionId });
                }
            }

            var result = new RoleWithPermissionsDto
            {
                RoleId = role.RoleId,
                Name = role.RoleName,
                Description = role.Description,
                Permissions = roleDto.Permissions,
                IsActive = role.IsActive,
                CreatedAt = role.CreatedAt
            };

            return Ok(new { message = "Role updated successfully", data = result });
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "23505")
        {
            return Conflict(new { message = "A role with this name already exists" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // DELETE: api/Roles/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRole(int id)
    {
        try
        {
            // RolePermissions will be deleted automatically due to CASCADE
            var sql = "DELETE FROM Roles WHERE role_id = @RoleId";
            var rowsAffected = await _connection.ExecuteAsync(sql, new { RoleId = id });

            if (rowsAffected == 0)
            {
                return NotFound(new { message = $"Role with ID {id} not found" });
            }

            return Ok(new { message = "Role deleted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
