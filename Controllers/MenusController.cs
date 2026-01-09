using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Dapper;
using NehaSurgicalAPI.Models;
using NehaSurgicalAPI.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace NehaSurgicalAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenusController : ControllerBase
{
    private readonly NpgsqlConnection _connection;

    public MenusController(NpgsqlConnection connection)
    {
        _connection = connection;
    }

    // GET: api/Menus
    [HttpGet]
    public async Task<IActionResult> GetAllMenus([FromQuery] string? isActive = null)
    {
        try
        {
            var sql = @"SELECT 
                menu_id as MenuId,
                menu_name as MenuName,
                menu_path as MenuPath,
                menu_icon as MenuIcon,
                parent_menu_id as ParentMenuId,
                sort_order as SortOrder,
                is_active as IsActive
                FROM Menus";

            string? isActiveYN = null;
            if (!string.IsNullOrEmpty(isActive))
            {
                // Accept 'true'/'false' or 'Y'/'N' from query, normalize to 'Y'/'N'
                if (isActive.Equals("true", StringComparison.OrdinalIgnoreCase) || isActive == "1")
                    isActiveYN = "Y";
                else if (isActive.Equals("false", StringComparison.OrdinalIgnoreCase) || isActive == "0")
                    isActiveYN = "N";
                else
                    isActiveYN = isActive.ToUpper();
                sql += " WHERE is_active = @IsActive";
            }

            sql += " ORDER BY sort_order";

            var menus = await _connection.QueryAsync<Menu>(sql, new { IsActive = isActiveYN });
            var menuDtos = BuildMenuHierarchy(menus.ToList());

            return Ok(new { message = "Menus retrieved successfully", data = menuDtos });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // GET: api/Menus/user-menus (Get menus for logged-in user based on role)
    [HttpGet("user-menus")]
    [Authorize]
    public async Task<IActionResult> GetUserMenus()
    {
        try
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

            // Get user's role
            var userSql = "SELECT role_id FROM SystemUsers WHERE system_user_id = @UserId";
            var roleId = await _connection.QueryFirstOrDefaultAsync<int?>(userSql, new { UserId = userId });

            if (roleId == null)
            {
                return NotFound(new { message = "User role not found" });
            }

            // Get menus with permissions for this role
            var sql = @"SELECT 
                m.menu_id as MenuId,
                m.menu_name as MenuName,
                m.menu_path as MenuPath,
                m.menu_icon as MenuIcon,
                m.parent_menu_id as ParentMenuId,
                m.sort_order as SortOrder,
                rmp.can_view as CanView,
                rmp.can_create as CanCreate,
                rmp.can_edit as CanEdit,
                rmp.can_delete as CanDelete
                FROM Menus m
                INNER JOIN RoleMenuPermissions rmp ON m.menu_id = rmp.menu_id
                WHERE rmp.role_id = @RoleId 
                AND m.is_active = 'Y'
                AND rmp.can_view = 'Y'
                ORDER BY m.sort_order";

            var menus = await _connection.QueryAsync<MenuWithPermissionsDto>(sql, new { RoleId = roleId });
            var menuHierarchy = BuildPermissionMenuHierarchy(menus.ToList());

            return Ok(new { message = "User menus retrieved successfully", data = new UserMenuResponseDto { Menus = menuHierarchy } });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // GET: api/Menus/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetMenuById(int id)
    {
        try
        {
            var sql = @"SELECT 
                menu_id as MenuId,
                menu_name as MenuName,
                menu_path as MenuPath,
                menu_icon as MenuIcon,
                parent_menu_id as ParentMenuId,
                sort_order as SortOrder,
                is_active as IsActive
                FROM Menus WHERE menu_id = @MenuId";

            var menu = await _connection.QueryFirstOrDefaultAsync<Menu>(sql, new { MenuId = id });

            if (menu == null)
            {
                return NotFound(new { message = $"Menu with ID {id} not found" });
            }

            return Ok(new { message = "Menu retrieved successfully", data = new MenuDto
            {
                MenuId = menu.MenuId,
                MenuName = menu.MenuName,
                MenuPath = menu.MenuPath,
                MenuIcon = menu.MenuIcon,
                ParentMenuId = menu.ParentMenuId,
                SortOrder = menu.SortOrder,
                IsActive = menu.IsActive
            }});
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // POST: api/Menus
    [HttpPost]
    public async Task<IActionResult> CreateMenu([FromBody] CreateMenuDto menuDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Normalize isActive to 'Y'/'N'
            if (!string.IsNullOrEmpty(menuDto.IsActive))
            {
                if (menuDto.IsActive.Equals("true", StringComparison.OrdinalIgnoreCase) || menuDto.IsActive == "1")
                    menuDto.IsActive = "Y";
                else if (menuDto.IsActive.Equals("false", StringComparison.OrdinalIgnoreCase) || menuDto.IsActive == "0")
                    menuDto.IsActive = "N";
                else
                    menuDto.IsActive = menuDto.IsActive.ToUpper();
            }

            var sql = @"INSERT INTO Menus (menu_name, menu_path, menu_icon, parent_menu_id, sort_order, is_active, created_at, updated_at) 
                        VALUES (@MenuName, @MenuPath, @MenuIcon, @ParentMenuId, @SortOrder, @IsActive, NOW(), NOW())
                        RETURNING menu_id as MenuId, menu_name as MenuName, menu_path as MenuPath, 
                                  menu_icon as MenuIcon, parent_menu_id as ParentMenuId, sort_order as SortOrder, is_active as IsActive";

            var menu = await _connection.QueryFirstAsync<Menu>(sql, menuDto);

            return CreatedAtAction(nameof(GetMenuById), new { id = menu.MenuId }, 
                new { message = "Menu created successfully", data = new MenuDto
                {
                    MenuId = menu.MenuId,
                    MenuName = menu.MenuName,
                    MenuPath = menu.MenuPath,
                    MenuIcon = menu.MenuIcon,
                    ParentMenuId = menu.ParentMenuId,
                    SortOrder = menu.SortOrder,
                    IsActive = menu.IsActive
                }});
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // PUT: api/Menus/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMenu(int id, [FromBody] UpdateMenuDto menuDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Normalize isActive to 'Y'/'N'
            if (!string.IsNullOrEmpty(menuDto.IsActive))
            {
                if (menuDto.IsActive.Equals("true", StringComparison.OrdinalIgnoreCase) || menuDto.IsActive == "1")
                    menuDto.IsActive = "Y";
                else if (menuDto.IsActive.Equals("false", StringComparison.OrdinalIgnoreCase) || menuDto.IsActive == "0")
                    menuDto.IsActive = "N";
                else
                    menuDto.IsActive = menuDto.IsActive.ToUpper();
            }

            var sql = @"UPDATE Menus 
                        SET menu_name = @MenuName, menu_path = @MenuPath, menu_icon = @MenuIcon, 
                            parent_menu_id = @ParentMenuId, sort_order = @SortOrder, is_active = @IsActive, updated_at = NOW()
                        WHERE menu_id = @MenuId
                        RETURNING menu_id as MenuId, menu_name as MenuName, menu_path as MenuPath, 
                                  menu_icon as MenuIcon, parent_menu_id as ParentMenuId, sort_order as SortOrder, is_active as IsActive";

            var menu = await _connection.QueryFirstOrDefaultAsync<Menu>(sql, 
                new { 
                    MenuId = id, 
                    menuDto.MenuName, 
                    menuDto.MenuPath, 
                    menuDto.MenuIcon, 
                    menuDto.ParentMenuId, 
                    menuDto.SortOrder, 
                    menuDto.IsActive 
                });

            if (menu == null)
            {
                return NotFound(new { message = $"Menu with ID {id} not found" });
            }

            return Ok(new { message = "Menu updated successfully", data = new MenuDto
            {
                MenuId = menu.MenuId,
                MenuName = menu.MenuName,
                MenuPath = menu.MenuPath,
                MenuIcon = menu.MenuIcon,
                ParentMenuId = menu.ParentMenuId,
                SortOrder = menu.SortOrder,
                IsActive = menu.IsActive
            }});
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // DELETE: api/Menus/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMenu(int id)
    {
        try
        {
            var sql = "DELETE FROM Menus WHERE menu_id = @MenuId";
            var rowsAffected = await _connection.ExecuteAsync(sql, new { MenuId = id });

            if (rowsAffected == 0)
            {
                return NotFound(new { message = $"Menu with ID {id} not found" });
            }

            return Ok(new { message = "Menu deleted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    private List<MenuDto> BuildMenuHierarchy(List<Menu> menus)
    {
        var menuDtos = menus.Select(m => new MenuDto
        {
            MenuId = m.MenuId,
            MenuName = m.MenuName,
            MenuPath = m.MenuPath,
            MenuIcon = m.MenuIcon,
            ParentMenuId = m.ParentMenuId,
            SortOrder = m.SortOrder,
            IsActive = m.IsActive,
            SubMenus = new List<MenuDto>()
        }).ToList();

        var parentMenus = menuDtos.Where(m => m.ParentMenuId == null).ToList();
        
        foreach (var parent in parentMenus)
        {
            parent.SubMenus = menuDtos.Where(m => m.ParentMenuId == parent.MenuId).OrderBy(m => m.SortOrder).ToList();
        }

        return parentMenus.OrderBy(m => m.SortOrder).ToList();
    }

    private List<MenuWithPermissionsDto> BuildPermissionMenuHierarchy(List<MenuWithPermissionsDto> menus)
    {
        var parentMenus = menus.Where(m => m.ParentMenuId == null).ToList();
        
        foreach (var parent in parentMenus)
        {
            parent.SubMenus = menus.Where(m => m.ParentMenuId == parent.MenuId).OrderBy(m => m.SortOrder).ToList();
        }

        return parentMenus.OrderBy(m => m.SortOrder).ToList();
    }
}
