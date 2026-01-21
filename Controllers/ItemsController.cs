using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Dapper;
using NehaSurgicalAPI.Models;
using NehaSurgicalAPI.DTOs;

namespace NehaSurgicalAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly NpgsqlConnection _connection;

    public ItemsController(NpgsqlConnection connection)
    {
        _connection = connection;
    }

    // GET: api/Items?isActive=Y
    [HttpGet]
    public async Task<IActionResult> GetAllItems([FromQuery] string? isActive = null)
    {
        try
        {
            var sql = @"SELECT 
                i.item_id as ItemId,
                i.name as Name,
                i.shortname as Shortname,
                i.brand_id as BrandId,
                b.name as BrandName,
                i.category_id as CategoryId,
                c.name as CategoryName,
                i.item_group_id as ItemGroupId,
                ig.name as ItemGroupName,
                i.specification_id as SpecificationId,
                s.name as SpecificationName,
                i.size_id as SizeId,
                sz.name as SizeName,
                i.material as Material,
                i.model as Model,
                i.description as Description,
                i.price as Price,
                i.status as Status,
                i.is_active as IsActive,
                i.created_at as CreatedAt,
                i.updated_at as UpdatedAt
                FROM Items i
                INNER JOIN Brands b ON i.brand_id = b.brand_id
                INNER JOIN Categories c ON i.category_id = c.category_id
                LEFT JOIN ItemGroups ig ON i.item_group_id = ig.item_group_id
                LEFT JOIN Specifications s ON i.specification_id = s.specification_id
                LEFT JOIN Sizes sz ON i.size_id = sz.size_id";

            if (!string.IsNullOrEmpty(isActive))
            {
                sql += " WHERE i.is_active = @IsActive";
            }

            sql += " ORDER BY i.item_id DESC";

            var items = await _connection.QueryAsync<dynamic>(sql, new { IsActive = isActive });
            var itemDtos = items.Select(i => MapToDto(i)).ToList();

            return Ok(new { message = "Items retrieved successfully", data = itemDtos });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // GET: api/Items/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetItemById(int id)
    {
        try
        {
            var sql = @"SELECT 
                i.item_id as ItemId,
                i.name as Name,
                i.shortname as Shortname,
                i.brand_id as BrandId,
                b.name as BrandName,
                i.category_id as CategoryId,
                c.name as CategoryName,
                i.item_group_id as ItemGroupId,
                ig.name as ItemGroupName,
                i.specification_id as SpecificationId,
                s.name as SpecificationName,
                i.size_id as SizeId,
                sz.name as SizeName,
                i.material as Material,
                i.model as Model,
                i.description as Description,
                i.price as Price,
                i.status as Status,
                i.is_active as IsActive,
                i.created_at as CreatedAt,
                i.updated_at as UpdatedAt
                FROM Items i
                INNER JOIN Brands b ON i.brand_id = b.brand_id
                INNER JOIN Categories c ON i.category_id = c.category_id
                LEFT JOIN ItemGroups ig ON i.item_group_id = ig.item_group_id
                LEFT JOIN Specifications s ON i.specification_id = s.specification_id
                LEFT JOIN Sizes sz ON i.size_id = sz.size_id
                WHERE i.item_id = @ItemId";

            var item = await _connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { ItemId = id });

            if (item == null)
            {
                return NotFound(new { message = $"Item with ID {id} not found" });
            }

            return Ok(new { message = "Item retrieved successfully", data = MapToDto(item) });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // GET: api/Items/brand/{brandId}
    [HttpGet("brand/{brandId}")]
    public async Task<IActionResult> GetItemsByBrand(int brandId)
    {
        try
        {
            var sql = @"SELECT 
                i.item_id as ItemId,
                i.name as Name,
                i.shortname as Shortname,
                i.brand_id as BrandId,
                b.name as BrandName,
                i.category_id as CategoryId,
                c.name as CategoryName,
                i.item_group_id as ItemGroupId,
                ig.name as ItemGroupName,
                i.specification_id as SpecificationId,
                s.name as SpecificationName,
                i.size_id as SizeId,
                sz.name as SizeName,
                i.material as Material,
                i.model as Model,
                i.description as Description,
                i.price as Price,
                i.status as Status,
                i.is_active as IsActive,
                i.created_at as CreatedAt,
                i.updated_at as UpdatedAt
                FROM Items i
                INNER JOIN Brands b ON i.brand_id = b.brand_id
                INNER JOIN Categories c ON i.category_id = c.category_id
                LEFT JOIN ItemGroups ig ON i.item_group_id = ig.item_group_id
                LEFT JOIN Specifications s ON i.specification_id = s.specification_id
                LEFT JOIN Sizes sz ON i.size_id = sz.size_id
                WHERE i.brand_id = @BrandId AND i.is_active = 'Y'
                ORDER BY i.item_id DESC";

            var items = await _connection.QueryAsync<dynamic>(sql, new { BrandId = brandId });
            var itemDtos = items.Select(i => MapToDto(i)).ToList();

            return Ok(new { message = "Items retrieved successfully", data = itemDtos });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // GET: api/Items/itemgroup/{itemGroupId}
    [HttpGet("itemgroup/{itemGroupId}")]
    public async Task<IActionResult> GetItemsByItemGroup(int itemGroupId)
    {
        try
        {
            var sql = @"SELECT 
                i.item_id as ItemId,
                i.name as Name,
                i.shortname as Shortname,
                i.brand_id as BrandId,
                b.name as BrandName,
                i.category_id as CategoryId,
                c.name as CategoryName,
                i.item_group_id as ItemGroupId,
                ig.name as ItemGroupName,
                i.specification_id as SpecificationId,
                s.name as SpecificationName,
                i.size_id as SizeId,
                sz.name as SizeName,
                i.material as Material,
                i.model as Model,
                i.description as Description,
                i.price as Price,
                i.status as Status,
                i.is_active as IsActive,
                i.created_at as CreatedAt,
                i.updated_at as UpdatedAt
                FROM Items i
                INNER JOIN Brands b ON i.brand_id = b.brand_id
                INNER JOIN Categories c ON i.category_id = c.category_id
                LEFT JOIN ItemGroups ig ON i.item_group_id = ig.item_group_id
                LEFT JOIN Specifications s ON i.specification_id = s.specification_id
                LEFT JOIN Sizes sz ON i.size_id = sz.size_id
                WHERE i.item_group_id = @ItemGroupId AND i.is_active = 'Y'
                ORDER BY i.item_id DESC";

            var items = await _connection.QueryAsync<dynamic>(sql, new { ItemGroupId = itemGroupId });
            var itemDtos = items.Select(i => MapToDto(i)).ToList();

            return Ok(new { message = "Items retrieved successfully", data = itemDtos });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // GET: api/Items/filter?itemGroupId=1&specificationId=2&sizeId=3
    [HttpGet("filter")]
    public async Task<IActionResult> GetItemsByFilters(
        [FromQuery] int? itemGroupId = null,
        [FromQuery] int? specificationId = null,
        [FromQuery] int? sizeId = null)
    {
        try
        {
            var sql = @"SELECT 
                i.item_id as ItemId,
                i.name as Name,
                i.shortname as Shortname,
                i.brand_id as BrandId,
                b.name as BrandName,
                i.category_id as CategoryId,
                c.name as CategoryName,
                i.item_group_id as ItemGroupId,
                ig.name as ItemGroupName,
                i.specification_id as SpecificationId,
                s.name as SpecificationName,
                i.size_id as SizeId,
                sz.name as SizeName,
                i.material as Material,
                i.model as Model,
                i.description as Description,
                i.price as Price,
                i.status as Status,
                i.is_active as IsActive,
                i.created_at as CreatedAt,
                i.updated_at as UpdatedAt
                FROM Items i
                INNER JOIN Brands b ON i.brand_id = b.brand_id
                INNER JOIN Categories c ON i.category_id = c.category_id
                LEFT JOIN ItemGroups ig ON i.item_group_id = ig.item_group_id
                LEFT JOIN Specifications s ON i.specification_id = s.specification_id
                LEFT JOIN Sizes sz ON i.size_id = sz.size_id
                WHERE i.is_active = 'Y'";

            var parameters = new DynamicParameters();

            if (itemGroupId.HasValue)
            {
                sql += " AND i.item_group_id = @ItemGroupId";
                parameters.Add("ItemGroupId", itemGroupId.Value);
            }

            if (specificationId.HasValue)
            {
                sql += " AND i.specification_id = @SpecificationId";
                parameters.Add("SpecificationId", specificationId.Value);
            }

            if (sizeId.HasValue)
            {
                sql += " AND i.size_id = @SizeId";
                parameters.Add("SizeId", sizeId.Value);
            }

            sql += " ORDER BY i.item_id DESC";

            var items = await _connection.QueryAsync<dynamic>(sql, parameters);
            var itemDtos = items.Select(i => MapToDto(i)).ToList();

            return Ok(new { 
                message = "Items retrieved successfully", 
                filters = new {
                    itemGroupId,
                    specificationId,
                    sizeId
                },
                count = itemDtos.Count,
                data = itemDtos 
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // POST: api/Items
    [HttpPost]
    public async Task<IActionResult> CreateItem([FromBody] CreateItemDto itemDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check for duplicate item
            var duplicateCheck = await CheckDuplicateItem(null, itemDto.Name, itemDto.BrandId, itemDto.CategoryId, 
                itemDto.SpecificationId, itemDto.SizeId);
            if (duplicateCheck != null)
            {
                return Conflict(new { message = duplicateCheck });
            }

            var sql = @"INSERT INTO Items (name, shortname, brand_id, category_id, item_group_id, specification_id, size_id, material, model, description, price, status, is_active, created_at, updated_at) 
                        VALUES (@Name, @Shortname, @BrandId, @CategoryId, @ItemGroupId, @SpecificationId, @SizeId, @Material, @Model, @Description, @Price, @Status, @IsActive, NOW(), NOW())
                        RETURNING item_id";

            var itemId = await _connection.ExecuteScalarAsync<int>(sql, itemDto);

            // Fetch the created item with related data
            var createdItem = await GetItemByIdInternal(itemId);

            return CreatedAtAction(nameof(GetItemById), new { id = itemId }, 
                new { message = "Item created successfully", data = createdItem });
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "23505")
        {
            return Conflict(new { message = "An item with this name already exists" });
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "23503")
        {
            return BadRequest(new { message = "Invalid brand, category, specification, or size ID" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // PUT: api/Items/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateItem(int id, [FromBody] UpdateItemDto itemDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check for duplicate item (excluding current item)
            var duplicateCheck = await CheckDuplicateItem(id, itemDto.Name, itemDto.BrandId, itemDto.CategoryId, 
                itemDto.SpecificationId, itemDto.SizeId);
            if (duplicateCheck != null)
            {
                return Conflict(new { message = duplicateCheck });
            }

            var sql = @"UPDATE Items 
                        SET name = @Name, 
                            shortname = @Shortname, 
                            brand_id = @BrandId, 
                            category_id = @CategoryId, 
                            item_group_id = @ItemGroupId, 
                            specification_id = @SpecificationId, 
                            size_id = @SizeId, 
                            material = @Material, 
                            model = @Model, 
                            description = @Description, 
                            price = @Price, 
                            status = @Status, 
                            is_active = @IsActive, 
                            updated_at = NOW()
                        WHERE item_id = @ItemId
                        RETURNING item_id";

            var rowsAffected = await _connection.ExecuteScalarAsync<int?>(sql, 
                new { 
                    ItemId = id, 
                    itemDto.Name, 
                    itemDto.Shortname, 
                    itemDto.BrandId, 
                    itemDto.CategoryId, 
                    itemDto.ItemGroupId, 
                    itemDto.SpecificationId, 
                    itemDto.SizeId, 
                    itemDto.Material, 
                    itemDto.Model, 
                    itemDto.Description, 
                    itemDto.Price, 
                    itemDto.Status, 
                    itemDto.IsActive 
                });

            if (rowsAffected == null)
            {
                return NotFound(new { message = $"Item with ID {id} not found" });
            }

            // Fetch the updated item with related data
            var updatedItem = await GetItemByIdInternal(id);

            return Ok(new { message = "Item updated successfully", data = updatedItem });
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "23505")
        {
            return Conflict(new { message = "An item with this name already exists" });
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "23503")
        {
            return BadRequest(new { message = "Invalid brand, category, specification, or size ID" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // DELETE: api/Items/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteItem(int id)
    {
        try
        {
            var sql = "DELETE FROM Items WHERE item_id = @ItemId";
            var rowsAffected = await _connection.ExecuteAsync(sql, new { ItemId = id });

            if (rowsAffected == 0)
            {
                return NotFound(new { message = $"Item with ID {id} not found" });
            }

            return Ok(new { message = "Item deleted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    private ItemDto MapToDto(dynamic item)
    {
        return new ItemDto
        {
            ItemId = (int)item.itemid,
            Name = (string)item.name,
            Shortname = (string)item.shortname,
            DisplayName = $"{item.shortname} - {item.name} - {item.brandname}",
            BrandId = (int)item.brandid,
            BrandName = (string)item.brandname,
            CategoryId = (int)item.categoryid,
            CategoryName = (string)item.categoryname,
            ItemGroupId = item.itemgroupid != null ? (int?)item.itemgroupid : null,
            ItemGroupName = item.itemgroupname != null ? (string)item.itemgroupname : null,
            SpecificationId = item.specificationid != null ? (int?)item.specificationid : null,
            SpecificationName = item.specificationname != null ? (string)item.specificationname : null,
            SizeId = item.sizeid != null ? (int?)item.sizeid : null,
            SizeName = item.sizename != null ? (string)item.sizename : null,
            Material = item.material != null ? (string)item.material : null,
            Model = item.model != null ? (string)item.model : null,
            Description = item.description != null ? (string)item.description : null,
            Price = (decimal)item.price,
            Status = (string)item.status,
            IsActive = (string)item.isactive,
            CreatedAt = (DateTime)item.createdat
        };
    }

    private async Task<ItemDto> GetItemByIdInternal(int id)
    {
        var sql = @"SELECT 
            i.item_id as ItemId,
            i.name as Name,
            i.shortname as Shortname,
            i.brand_id as BrandId,
            b.name as BrandName,
            i.category_id as CategoryId,
            c.name as CategoryName,
            i.item_group_id as ItemGroupId,
            ig.name as ItemGroupName,
            i.specification_id as SpecificationId,
            s.name as SpecificationName,
            i.size_id as SizeId,
            sz.name as SizeName,
            i.material as Material,
            i.model as Model,
            i.description as Description,
            i.price as Price,
            i.status as Status,
            i.is_active as IsActive,
            i.created_at as CreatedAt
            FROM Items i
            INNER JOIN Brands b ON i.brand_id = b.brand_id
            INNER JOIN Categories c ON i.category_id = c.category_id
            LEFT JOIN ItemGroups ig ON i.item_group_id = ig.item_group_id
            LEFT JOIN Specifications s ON i.specification_id = s.specification_id
            LEFT JOIN Sizes sz ON i.size_id = sz.size_id
            WHERE i.item_id = @ItemId";

        var item = await _connection.QueryFirstAsync<dynamic>(sql, new { ItemId = id });
        return MapToDto(item);
    }

    private async Task<string?> CheckDuplicateItem(int? currentItemId, string name, int brandId, int categoryId, 
        int? specificationId, int? sizeId)
    {
        var sql = @"SELECT i.item_id, i.name, b.name as brand_name, c.name as category_name, 
                           s.name as specification_name, sz.name as size_name
                    FROM Items i
                    INNER JOIN Brands b ON i.brand_id = b.brand_id
                    INNER JOIN Categories c ON i.category_id = c.category_id
                    LEFT JOIN Specifications s ON i.specification_id = s.specification_id
                    LEFT JOIN Sizes sz ON i.size_id = sz.size_id
                    WHERE LOWER(i.name) = LOWER(@Name)
                    AND i.brand_id = @BrandId
                    AND i.category_id = @CategoryId
                    AND (i.specification_id = @SpecificationId OR (i.specification_id IS NULL AND @SpecificationId IS NULL))
                    AND (i.size_id = @SizeId OR (i.size_id IS NULL AND @SizeId IS NULL))";

        if (currentItemId.HasValue)
        {
            sql += " AND i.item_id != @CurrentItemId";
        }

        var duplicate = await _connection.QueryFirstOrDefaultAsync<dynamic>(sql, new 
        { 
            Name = name,
            BrandId = brandId,
            CategoryId = categoryId,
            SpecificationId = specificationId,
            SizeId = sizeId,
            CurrentItemId = currentItemId
        });

        if (duplicate != null)
        {
            var specName = duplicate.specification_name != null ? $", Specification: {duplicate.specification_name}" : "";
            var sizeName = duplicate.size_name != null ? $", Size: {duplicate.size_name}" : "";
            return $"Item '{duplicate.name}' already exists with Brand: {duplicate.brand_name}, Category: {duplicate.category_name}{specName}{sizeName}";
        }

        return null;
    }
}
