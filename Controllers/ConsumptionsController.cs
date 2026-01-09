using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using NehaSurgicalAPI.DTOs;
using NehaSurgicalAPI.Models;
using System.Text.Json;

namespace NehaSurgicalAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConsumptionsController : ControllerBase
{
    private readonly NpgsqlConnection _connection;
    private readonly ILogger<ConsumptionsController> _logger;

    public ConsumptionsController(NpgsqlConnection connection, ILogger<ConsumptionsController> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    // GET: api/Consumptions
    [HttpGet]
    public async Task<IActionResult> GetAllConsumptions([FromQuery] int? orderId = null, [FromQuery] string? type = null)
    {
        try
        {
            if (_connection.State != System.Data.ConnectionState.Open)
                await _connection.OpenAsync();

            var sql = @"SELECT c.consumption_id as ConsumptionId, c.order_id as OrderId,
                        o.order_no as OrderNo, c.item_group_id as ItemGroupId,
                        c.item_group_name as ItemGroupName, c.consumed_items as ConsumedItems,
                        c.created_by as CreatedBy,
                        c.created_at as CreatedAt
                        FROM Consumptions c
                        INNER JOIN Orders o ON c.order_id = o.order_id
                        WHERE c.is_active = 'Y'";

            if (orderId.HasValue)
            {
                sql += " AND c.order_id = @OrderId";
            }

            sql += " ORDER BY c.created_at DESC";

            var consumptions = await _connection.QueryAsync<dynamic>(sql, new { OrderId = orderId });

            var consumptionDtos = consumptions.Select(c => new ConsumptionDto
            {
                ConsumptionId = (int)c.consumptionid,
                OrderId = (int)c.orderid,
                OrderNo = (string)c.orderno,
                ItemGroupId = c.itemgroupid != null ? (int?)c.itemgroupid : null,
                ItemGroupName = c.itemgroupname != null ? (string)c.itemgroupname : null,
                ConsumedItems = JsonSerializer.Deserialize<List<ConsumedItemDto>>((string)c.consumeditems) ?? new(),
                CreatedBy = (string)c.createdby,
                CreatedAt = ((DateTime)c.createdat).ToString("yyyy-MM-dd HH:mm:ss")
            }).ToList();

            // Filter by item type if specified
            if (!string.IsNullOrEmpty(type))
            {
                consumptionDtos = consumptionDtos
                    .Select(c => new ConsumptionDto
                    {
                        ConsumptionId = c.ConsumptionId,
                        OrderId = c.OrderId,
                        OrderNo = c.OrderNo,
                        ItemGroupId = c.ItemGroupId,
                        ItemGroupName = c.ItemGroupName,
                        ConsumedItems = c.ConsumedItems.Where(item => item.Type == type).ToList(),
                        CreatedBy = c.CreatedBy,
                        CreatedAt = c.CreatedAt
                    })
                    .Where(c => c.ConsumedItems.Count > 0)
                    .ToList();
            }

            return Ok(new { message = "Consumptions retrieved successfully", data = consumptionDtos });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving consumptions");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // GET: api/Consumptions/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetConsumptionById(int id)
    {
        try
        {
            if (_connection.State != System.Data.ConnectionState.Open)
                await _connection.OpenAsync();

            var sql = @"SELECT c.consumption_id as ConsumptionId, c.order_id as OrderId,
                        o.order_no as OrderNo, c.item_group_id as ItemGroupId,
                        c.item_group_name as ItemGroupName, c.consumed_items as ConsumedItems,
                        c.created_by as CreatedBy,
                        c.created_at as CreatedAt
                        FROM Consumptions c
                        INNER JOIN Orders o ON c.order_id = o.order_id
                        WHERE c.consumption_id = @Id AND c.is_active = 'Y'";

            var consumption = await _connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { Id = id });

            if (consumption == null)
            {
                return NotFound(new { message = $"Consumption with ID {id} not found" });
            }

            var consumptionDto = new ConsumptionDto
            {
                ConsumptionId = (int)consumption.consumptionid,
                OrderId = (int)consumption.orderid,
                OrderNo = (string)consumption.orderno,
                ItemGroupId = consumption.itemgroupid != null ? (int?)consumption.itemgroupid : null,
                ItemGroupName = consumption.itemgroupname != null ? (string)consumption.itemgroupname : null,
                ConsumedItems = JsonSerializer.Deserialize<List<ConsumedItemDto>>((string)consumption.consumeditems) ?? new(),
                CreatedBy = (string)consumption.createdby,
                CreatedAt = ((DateTime)consumption.createdat).ToString("yyyy-MM-dd HH:mm:ss")
            };

            return Ok(new { message = "Consumption retrieved successfully", data = consumptionDto });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving consumption with ID {ConsumptionId}", id);
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // POST: api/Consumptions
    [HttpPost]
    public async Task<IActionResult> CreateConsumption([FromBody] CreateConsumptionDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (dto.ConsumedItems == null || dto.ConsumedItems.Count == 0)
            {
                return BadRequest(new { message = "At least one consumed item is required" });
            }

            if (_connection.State != System.Data.ConnectionState.Open)
                await _connection.OpenAsync();

            // Validate order exists
            var orderExists = await _connection.ExecuteScalarAsync<bool>(
                "SELECT EXISTS(SELECT 1 FROM Orders WHERE order_id = @OrderId AND is_active = 'Y')",
                new { dto.OrderId });

            if (!orderExists)
            {
                return NotFound(new { message = $"Order with ID {dto.OrderId} not found" });
            }

            // Validate item group if provided (treat 0 as null)
            if (dto.ItemGroupId.HasValue && dto.ItemGroupId.Value > 0)
            {
                var itemGroupExists = await _connection.ExecuteScalarAsync<bool>(
                    "SELECT EXISTS(SELECT 1 FROM ItemGroups WHERE item_group_id = @ItemGroupId AND is_active = 'Y')",
                    new { dto.ItemGroupId });

                if (!itemGroupExists)
                {
                    return NotFound(new { message = $"Item group with ID {dto.ItemGroupId} not found" });
                }
            }
            else if (dto.ItemGroupId == 0)
            {
                dto.ItemGroupId = null; // Treat 0 as null
            }

            // Serialize consumed items to JSON
            var consumedItemsJson = JsonSerializer.Serialize(dto.ConsumedItems);

            var sql = @"INSERT INTO Consumptions 
                        (order_id, item_group_id, item_group_name, consumed_items, created_by, created_at, updated_at, is_active)
                        VALUES (@OrderId, @ItemGroupId, @ItemGroupName, @ConsumedItems::jsonb, @CreatedBy, NOW(), NOW(), 'Y')
                        RETURNING consumption_id";

            var consumptionId = await _connection.ExecuteScalarAsync<int>(sql, new
            {
                dto.OrderId,
                dto.ItemGroupId,
                dto.ItemGroupName,
                ConsumedItems = consumedItemsJson,
                dto.CreatedBy
            });

            // Update order status to Completed
            try
            {
                var updateOrderSql = @"UPDATE Orders 
                                      SET status = 'Completed', updated_at = NOW() 
                                      WHERE order_id = @OrderId";
                await _connection.ExecuteAsync(updateOrderSql, new { dto.OrderId });

                // Create audit entry
                var auditSql = @"INSERT INTO OrderAudits 
                                (order_id, previous_status, new_status, changed_by, changed_at, remarks)
                                SELECT order_id, status, 'Completed', @ChangedBy, NOW(), @Remarks
                                FROM Orders WHERE order_id = @OrderId";
                
                await _connection.ExecuteAsync(auditSql, new 
                { 
                    dto.OrderId, 
                    ChangedBy = dto.CreatedBy, 
                    Remarks = $"Status changed to Completed after consumption entry (ID: {consumptionId})" 
                });

                _logger.LogInformation("Order {OrderId} status updated to Completed after consumption", dto.OrderId);
            }
            catch (Exception auditEx)
            {
                _logger.LogError(auditEx, "Error updating order status for order {OrderId}", dto.OrderId);
                // Continue - consumption was created successfully
            }

            return CreatedAtAction(nameof(GetConsumptionById), new { id = consumptionId },
                new { message = "Consumption created successfully and order marked as Completed", consumptionId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating consumption");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // PUT: api/Consumptions/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateConsumption(int id, [FromBody] UpdateConsumptionDto dto)
    {
        try
        {
            if (_connection.State != System.Data.ConnectionState.Open)
                await _connection.OpenAsync();

            // Check if consumption exists
            var exists = await _connection.ExecuteScalarAsync<bool>(
                "SELECT EXISTS(SELECT 1 FROM Consumptions WHERE consumption_id = @Id AND is_active = 'Y')",
                new { Id = id });

            if (!exists)
            {
                return NotFound(new { message = $"Consumption with ID {id} not found" });
            }

            // Validate item group if provided (treat 0 as null)
            if (dto.ItemGroupId.HasValue && dto.ItemGroupId.Value > 0)
            {
                var itemGroupExists = await _connection.ExecuteScalarAsync<bool>(
                    "SELECT EXISTS(SELECT 1 FROM ItemGroups WHERE item_group_id = @ItemGroupId AND is_active = 'Y')",
                    new { dto.ItemGroupId });

                if (!itemGroupExists)
                {
                    return NotFound(new { message = $"Item group with ID {dto.ItemGroupId} not found" });
                }
            }
            else if (dto.ItemGroupId == 0)
            {
                dto.ItemGroupId = null; // Treat 0 as null
            }

            // Build update SQL dynamically
            var updateFields = new List<string>();
            var parameters = new DynamicParameters();
            parameters.Add("Id", id);

            if (dto.ItemGroupId.HasValue)
            {
                updateFields.Add("item_group_id = @ItemGroupId");
                parameters.Add("ItemGroupId", dto.ItemGroupId.Value);
            }

            if (!string.IsNullOrEmpty(dto.ItemGroupName))
            {
                updateFields.Add("item_group_name = @ItemGroupName");
                parameters.Add("ItemGroupName", dto.ItemGroupName);
            }

            if (dto.ConsumedItems != null && dto.ConsumedItems.Count > 0)
            {
                var consumedItemsJson = JsonSerializer.Serialize(dto.ConsumedItems);
                updateFields.Add("consumed_items = @ConsumedItems::jsonb");
                parameters.Add("ConsumedItems", consumedItemsJson);
            }

            if (updateFields.Count == 0)
            {
                return BadRequest(new { message = "No fields to update" });
            }

            updateFields.Add("updated_at = NOW()");

            var sql = $"UPDATE Consumptions SET {string.Join(", ", updateFields)} WHERE consumption_id = @Id";

            await _connection.ExecuteAsync(sql, parameters);

            return Ok(new { message = "Consumption updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating consumption with ID {ConsumptionId}", id);
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // DELETE: api/Consumptions/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteConsumption(int id)
    {
        try
        {
            if (_connection.State != System.Data.ConnectionState.Open)
                await _connection.OpenAsync();

            // Check if consumption exists
            var exists = await _connection.ExecuteScalarAsync<bool>(
                "SELECT EXISTS(SELECT 1 FROM Consumptions WHERE consumption_id = @Id AND is_active = 'Y')",
                new { Id = id });

            if (!exists)
            {
                return NotFound(new { message = $"Consumption with ID {id} not found" });
            }

            // Soft delete
            var sql = "UPDATE Consumptions SET is_active = 'N', updated_at = NOW() WHERE consumption_id = @Id";
            await _connection.ExecuteAsync(sql, new { Id = id });

            return Ok(new { message = "Consumption deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting consumption with ID {ConsumptionId}", id);
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
