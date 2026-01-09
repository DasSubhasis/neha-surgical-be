using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Dapper;
using NehaSurgicalAPI.Models;
using NehaSurgicalAPI.DTOs;

namespace NehaSurgicalAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly NpgsqlConnection _connection;

    public UsersController(NpgsqlConnection connection)
    {
        _connection = connection;
    }

    // GET: api/Users?isActive=Y&roleId=1
    [HttpGet]
    public async Task<IActionResult> GetAllUsers([FromQuery] string? isActive = null, [FromQuery] int? roleId = null)
    {
        try
        {
            var sql = @"SELECT 
                su.system_user_id as UserId,
                su.email as Email,
                su.full_name as FullName,
                su.phone_no as Phone,
                su.employee_id as EmployeeId,
                su.identifier as Identifier,
                su.role_id as RoleId,
                r.role_name as RoleName,
                su.is_active as IsActive,
                su.created_at as CreatedAt
                FROM SystemUsers su
                LEFT JOIN Roles r ON su.role_id = r.role_id
                WHERE 1=1";

            if (!string.IsNullOrEmpty(isActive))
            {
                sql += " AND su.is_active = @IsActive";
            }

            if (roleId.HasValue)
            {
                sql += " AND su.role_id = @RoleId";
            }

            sql += " ORDER BY su.system_user_id DESC";

            var users = await _connection.QueryAsync<User>(sql, new { IsActive = isActive, RoleId = roleId });
            var userDtos = users.Select(u => MapToDto(u)).ToList();

            return Ok(new { message = "Users retrieved successfully", data = userDtos });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // GET: api/Users/role/{roleId}
    [HttpGet("role/{roleId}")]
    public async Task<IActionResult> GetUsersByRole(int roleId)
    {
        try
        {
            // First, check if role exists
            var roleExists = await _connection.QueryFirstOrDefaultAsync<string>(
                "SELECT role_name FROM Roles WHERE role_id = @RoleId", 
                new { RoleId = roleId });

            if (roleExists == null)
            {
                return NotFound(new { message = $"Role with ID {roleId} not found" });
            }

            var sql = @"SELECT 
                su.system_user_id as UserId,
                su.email as Email,
                su.full_name as FullName,
                su.phone_no as Phone,
                su.employee_id as EmployeeId,
                su.identifier as Identifier,
                su.role_id as RoleId,
                r.role_name as RoleName,
                su.is_active as IsActive,
                su.created_at as CreatedAt
                FROM SystemUsers su
                LEFT JOIN Roles r ON su.role_id = r.role_id
                WHERE su.role_id = @RoleId
                ORDER BY su.system_user_id DESC";

            var users = await _connection.QueryAsync<User>(sql, new { RoleId = roleId });
            var userDtos = users.Select(u => MapToDto(u)).ToList();

            return Ok(new { 
                message = $"Users with role '{roleExists}' retrieved successfully", 
                roleId = roleId,
                roleName = roleExists,
                count = userDtos.Count,
                data = userDtos 
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // GET: api/Users/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        try
        {
            var sql = @"SELECT 
                su.system_user_id as UserId,
                su.email as Email,
                su.full_name as FullName,
                su.phone_no as Phone,
                su.employee_id as EmployeeId,
                su.identifier as Identifier,
                su.role_id as RoleId,
                r.role_name as RoleName,
                su.is_active as IsActive,
                su.created_at as CreatedAt
                FROM SystemUsers su
                LEFT JOIN Roles r ON su.role_id = r.role_id
                WHERE su.system_user_id = @UserId";

            var user = await _connection.QueryFirstOrDefaultAsync<User>(sql, new { UserId = id });

            if (user == null)
            {
                return NotFound(new { message = $"User with ID {id} not found" });
            }

            return Ok(new { message = "User retrieved successfully", data = MapToDto(user) });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // POST: api/Users
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto userDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var sql = @"INSERT INTO SystemUsers (email, full_name, phone_no, employee_id, identifier, role_id, is_active, created_at, updated_at) 
                        VALUES (@Email, @FullName, @Phone, @EmployeeId, @Identifier, @RoleId, @IsActive, NOW(), NOW())
                        RETURNING system_user_id as UserId,
                                  email as Email,
                                  full_name as FullName,
                                  phone_no as Phone,
                                  employee_id as EmployeeId,
                                  identifier as Identifier,
                                  role_id as RoleId,
                                  is_active as IsActive,
                                  created_at as CreatedAt";

            var user = await _connection.QueryFirstAsync<User>(sql, userDto);

            // Get role name
            if (user.RoleId.HasValue)
            {
                var roleName = await _connection.QueryFirstOrDefaultAsync<string>(
                    "SELECT role_name FROM Roles WHERE role_id = @RoleId", 
                    new { RoleId = user.RoleId });
                user.RoleName = roleName;
            }

            return CreatedAtAction(nameof(GetUserById), new { id = user.UserId }, 
                new { message = "User created successfully", data = MapToDto(user) });
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "23505")
        {
            if (ex.Message.Contains("username"))
            {
                return Conflict(new { message = "A user with this username already exists" });
            }
            return Conflict(new { message = "A user with this email already exists" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // PUT: api/Users/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto userDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var sql = @"UPDATE SystemUsers 
                        SET email = @Email, 
                            full_name = @FullName, 
                            phone_no = @Phone,
                            employee_id = @EmployeeId,
                            identifier = @Identifier,
                            role_id = @RoleId, 
                            is_active = @IsActive,
                            updated_at = NOW()
                        WHERE system_user_id = @UserId
                        RETURNING system_user_id as UserId,
                                  email as Email,
                                  full_name as FullName,
                                  phone_no as Phone,
                                  employee_id as EmployeeId,
                                  identifier as Identifier,
                                  role_id as RoleId,
                                  is_active as IsActive,
                                  created_at as CreatedAt";

            var user = await _connection.QueryFirstOrDefaultAsync<User>(sql, 
                new { 
                    UserId = id, 
                    userDto.Email, 
                    userDto.FullName, 
                    userDto.Phone,
                    userDto.EmployeeId,
                    userDto.Identifier,
                    userDto.RoleId, 
                    userDto.IsActive 
                });

            if (user == null)
            {
                return NotFound(new { message = $"User with ID {id} not found" });
            }

            // Get role name
            if (user.RoleId.HasValue)
            {
                var roleName = await _connection.QueryFirstOrDefaultAsync<string>(
                    "SELECT role_name FROM Roles WHERE role_id = @RoleId", 
                    new { RoleId = user.RoleId });
                user.RoleName = roleName;
            }

            return Ok(new { message = "User updated successfully", data = MapToDto(user) });
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "23505")
        {
            if (ex.Message.Contains("username"))
            {
                return Conflict(new { message = "A user with this username already exists" });
            }
            return Conflict(new { message = "A user with this email already exists" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // DELETE: api/Users/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            // Check if user exists and get their role
            var checkSql = @"SELECT su.system_user_id, su.full_name as FullName, r.role_name as RoleName
                            FROM SystemUsers su
                            LEFT JOIN Roles r ON su.role_id = r.role_id
                            WHERE su.system_user_id = @UserId";
            
            var user = await _connection.QueryFirstOrDefaultAsync<dynamic>(checkSql, new { UserId = id });

            if (user == null)
            {
                return NotFound(new { message = $"User with ID {id} not found" });
            }

            // Prevent deletion of Super Admin users
            var roleName = user.rolename?.ToString()?.ToLower() ?? "";
            if (roleName == "super admin" || roleName == "superadmin" || roleName == "admin")
            {
                return BadRequest(new { message = $"Cannot delete user '{user.fullname}' with {user.rolename} role. Super Admin users cannot be deleted." });
            }

            var sql = "DELETE FROM SystemUsers WHERE system_user_id = @UserId";
            var rowsAffected = await _connection.ExecuteAsync(sql, new { UserId = id });

            return Ok(new { message = "User deleted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    private UserDto MapToDto(User user)
    {
        return new UserDto
        {
            UserId = user.UserId,
            Username = user.Username ?? user.Email.Split('@')[0],
            Email = user.Email,
            FullName = user.FullName,
            Phone = user.Phone,
            EmployeeId = user.EmployeeId,
            Identifier = user.Identifier,
            RoleId = user.RoleId,
            RoleName = user.RoleName,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLogin = user.LastLogin
        };
    }
}
