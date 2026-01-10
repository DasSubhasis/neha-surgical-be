using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using NehaSurgicalAPI.DTOs;
using NehaSurgicalAPI.Models;

namespace NehaSurgicalAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssistantOperationsController : ControllerBase
{
    private readonly NpgsqlConnection _connection;
    private readonly ILogger<AssistantOperationsController> _logger;

    public AssistantOperationsController(NpgsqlConnection connection, ILogger<AssistantOperationsController> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    // GET: api/AssistantOperations
    [HttpGet]
    public async Task<IActionResult> GetAllOperations([FromQuery] int? orderId = null, [FromQuery] int? assistantId = null)
    {
        try
        {
            if (_connection.State != System.Data.ConnectionState.Open)
                await _connection.OpenAsync();

            var sql = @"SELECT ao.operation_record_id as OperationRecordId, ao.order_id as OrderId, 
                        o.order_no as OrderNo, ao.assistant_id as AssistantId, 
                        su.full_name as AssistantName, ao.gps_latitude as GpsLatitude, 
                        ao.gps_longitude as GpsLongitude, ao.gps_location as GpsLocation, 
                        ao.checkin_time as CheckinTime, ao.checkout_time as CheckoutTime, ao.notes as Notes,
                        ao.created_at as CreatedAt, ao.updated_at as UpdatedAt
                        FROM AssistantOperations ao
                        INNER JOIN Orders o ON ao.order_id = o.order_id
                        INNER JOIN SystemUsers su ON ao.assistant_id = su.system_user_id
                        WHERE ao.is_active = 'Y'";

            if (orderId.HasValue)
            {
                sql += " AND ao.order_id = @OrderId";
            }

            if (assistantId.HasValue)
            {
                sql += " AND ao.assistant_id = @AssistantId";
            }

            sql += " ORDER BY ao.checkin_time DESC";

            var operations = await _connection.QueryAsync<dynamic>(sql, new { OrderId = orderId, AssistantId = assistantId });

            var operationDtos = operations.Select(op => new AssistantOperationDto
            {
                OperationRecordId = (int)op.operationrecordid,
                OrderId = (int)op.orderid,
                OrderNo = (string)op.orderno,
                AssistantId = (int)op.assistantid,
                AssistantName = (string)op.assistantname,
                GpsLatitude = op.gpslatitude != null ? (decimal?)op.gpslatitude : null,
                GpsLongitude = op.gpslongitude != null ? (decimal?)op.gpslongitude : null,
                GpsLocation = op.gpslocation != null ? (string)op.gpslocation : null,
                CheckinTime = op.checkintime != null ? ((DateTime)op.checkintime).ToString("yyyy-MM-dd HH:mm:ss") : null,
                CheckoutTime = op.checkouttime != null ? ((DateTime)op.checkouttime).ToString("yyyy-MM-dd HH:mm:ss") : null,
                Notes = op.notes != null ? (string)op.notes : null,
                CreatedAt = ((DateTime)op.createdat).ToString("yyyy-MM-dd HH:mm:ss"),
                UpdatedAt = ((DateTime)op.updatedat).ToString("yyyy-MM-dd HH:mm:ss")
            }).ToList();

            return Ok(new { message = "Assistant operations retrieved successfully", data = operationDtos });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assistant operations");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // GET: api/AssistantOperations/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOperationById(int id)
    {
        try
        {
            if (_connection.State != System.Data.ConnectionState.Open)
                await _connection.OpenAsync();

            var sql = @"SELECT ao.operation_record_id as OperationRecordId, ao.order_id as OrderId, 
                        o.order_no as OrderNo, ao.assistant_id as AssistantId, 
                        su.full_name as AssistantName, ao.gps_latitude as GpsLatitude, 
                        ao.gps_longitude as GpsLongitude, ao.gps_location as GpsLocation, ao.checkin_time as CheckinTime, 
                        ao.checkout_time as CheckoutTime, ao.notes as Notes,
                        ao.created_at as CreatedAt, ao.updated_at as UpdatedAt
                        FROM AssistantOperations ao
                        INNER JOIN Orders o ON ao.order_id = o.order_id
                        INNER JOIN SystemUsers su ON ao.assistant_id = su.system_user_id
                        WHERE ao.operation_record_id = @Id AND ao.is_active = 'Y'";

            var operation = await _connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { Id = id });

            if (operation == null)
            {
                return NotFound(new { message = $"Assistant operation with ID {id} not found" });
            }

            var operationDto = new AssistantOperationDto
            {
                OperationRecordId = (int)operation.operationrecordid,
                OrderId = (int)operation.orderid,
                OrderNo = (string)operation.orderno,
                AssistantId = (int)operation.assistantid,
                AssistantName = (string)operation.assistantname,
                GpsLatitude = operation.gpslatitude != null ? (decimal?)operation.gpslatitude : null,
                GpsLongitude = operation.gpslongitude != null ? (decimal?)operation.gpslongitude : null,
                GpsLocation = operation.gpslocation != null ? (string)operation.gpslocation : null,
                CheckinTime = operation.checkintime != null ? ((DateTime)operation.checkintime).ToString("yyyy-MM-dd HH:mm:ss") : null,
                CheckoutTime = operation.checkouttime != null ? ((DateTime)operation.checkouttime).ToString("yyyy-MM-dd HH:mm:ss") : null,
                Notes = operation.notes != null ? (string)operation.notes : null,
                CreatedAt = ((DateTime)operation.createdat).ToString("yyyy-MM-dd HH:mm:ss"),
                UpdatedAt = ((DateTime)operation.updatedat).ToString("yyyy-MM-dd HH:mm:ss")
            };

            return Ok(new { message = "Assistant operation retrieved successfully", data = operationDto });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assistant operation with ID {OperationRecordId}", id);
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // POST: api/AssistantOperations
    [HttpPost]
    public async Task<IActionResult> CreateOperation([FromBody] CreateAssistantOperationDto dto)
    {
        try
        {
            if (_connection.State != System.Data.ConnectionState.Open)
                await _connection.OpenAsync();

            // Validate order exists
            var orderExists = await _connection.ExecuteScalarAsync<bool>(
                "SELECT EXISTS(SELECT 1 FROM Orders WHERE order_id = @OrderId AND is_active = 'Y')",
                new { dto.OrderId });

            if (!orderExists)
            {
                return BadRequest(new { message = "Invalid order ID" });
            }

            // Validate assistant exists
            var assistantExists = await _connection.ExecuteScalarAsync<bool>(
                "SELECT EXISTS(SELECT 1 FROM SystemUsers WHERE system_user_id = @AssistantId AND is_active = 'Y')",
                new { dto.AssistantId });

            if (!assistantExists)
            {
                return BadRequest(new { message = "Invalid assistant ID" });
            }

            var sql = @"INSERT INTO AssistantOperations 
                        (order_id, assistant_id, gps_latitude, gps_longitude, gps_location, checkin_time, checkout_time, notes, created_at, updated_at)
                        VALUES (@OrderId, @AssistantId, @GpsLatitude, @GpsLongitude, @GpsLocation, @CheckinTime, @CheckoutTime, @Notes, NOW(), NOW())
                        RETURNING operation_record_id";

            var operationRecordId = await _connection.ExecuteScalarAsync<int>(sql, dto);

            // If checkout time is provided, update order status to Completed (Pre-Billing) only if not already set
            if (dto.CheckoutTime.HasValue)
            {
                try
                {
                    // Check current order status
                    var currentStatus = await _connection.QueryFirstOrDefaultAsync<string>(
                        "SELECT status FROM Orders WHERE order_id = @OrderId",
                        new { OrderId = dto.OrderId });

                    // Only update if status is not already 'Completed (Pre-Billing)'
                    if (currentStatus != "Completed (Pre-Billing)")
                    {
                        var updateOrderSql = @"UPDATE Orders 
                                               SET status = 'Completed (Pre-Billing)',
                                                   updated_at = NOW()
                                               WHERE order_id = @OrderId";

                        await _connection.ExecuteAsync(updateOrderSql, new { OrderId = dto.OrderId });

                        // Create audit entry for order status change
                        var auditSql = @"INSERT INTO OrderAudits (order_id, action, action_by, action_at)
                                        VALUES (@OrderId, 'Status changed to Completed (Pre-Billing) (Operation completed)', 'System', NOW())";

                        await _connection.ExecuteAsync(auditSql, new { OrderId = dto.OrderId });

                        _logger.LogInformation("Order {OrderId} status updated to Completed (Pre-Billing)", dto.OrderId);
                    }
                    else
                    {
                        _logger.LogInformation("Order {OrderId} status already set to Completed (Pre-Billing), skipping update", dto.OrderId);
                    }
                }
                catch (Exception orderEx)
                {
                    _logger.LogError(orderEx, "Failed to update order status to Completed (Pre-Billing) for order {OrderId}. Check if status is allowed in constraint.", dto.OrderId);
                }
            }
            // If only checkin time is provided, update order status to In-operation only if not already set
            else if (dto.CheckinTime.HasValue)
            {
                try
                {
                    // Check current order status
                    var currentStatus = await _connection.QueryFirstOrDefaultAsync<string>(
                        "SELECT status FROM Orders WHERE order_id = @OrderId",
                        new { OrderId = dto.OrderId });

                    // Only update if status is not already 'In-operation' or beyond
                    if (currentStatus != "In-operation" && currentStatus != "Completed (Pre-Billing)")
                    {
                        var updateOrderSql = @"UPDATE Orders 
                                               SET status = 'In-operation',
                                                   updated_at = NOW()
                                               WHERE order_id = @OrderId";

                        await _connection.ExecuteAsync(updateOrderSql, new { OrderId = dto.OrderId });

                        // Create audit entry for order status change
                        var auditSql = @"INSERT INTO OrderAudits (order_id, action, action_by, action_at)
                                        VALUES (@OrderId, 'Status changed to In-operation (Operation started)', 'System', NOW())";

                        await _connection.ExecuteAsync(auditSql, new { OrderId = dto.OrderId });

                        _logger.LogInformation("Order {OrderId} status updated to In-operation", dto.OrderId);
                    }
                    else
                    {
                        _logger.LogInformation("Order {OrderId} status already set to {Status}, skipping update", dto.OrderId, currentStatus);
                    }
                }
                catch (Exception orderEx)
                {
                    _logger.LogError(orderEx, "Failed to update order status to In-operation for order {OrderId}. Check if 'In-operation' is allowed in status constraint.", dto.OrderId);
                }
            }

            return CreatedAtAction(nameof(GetOperationById), new { id = operationRecordId }, 
                new { message = "Assistant operation created successfully", operationRecordId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating assistant operation");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // PUT: api/AssistantOperations/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOperation(int id, [FromBody] UpdateAssistantOperationDto dto)
    {
        try
        {
            if (_connection.State != System.Data.ConnectionState.Open)
                await _connection.OpenAsync();

            // Check if operation exists
            var exists = await _connection.ExecuteScalarAsync<bool>(
                "SELECT EXISTS(SELECT 1 FROM AssistantOperations WHERE operation_record_id = @Id AND is_active = 'Y')",
                new { Id = id });

            if (!exists)
            {
                return NotFound(new { message = $"Assistant operation with ID {id} not found" });
            }

            var sql = @"UPDATE AssistantOperations 
                        SET gps_latitude = COALESCE(@GpsLatitude, gps_latitude),
                            gps_longitude = COALESCE(@GpsLongitude, gps_longitude),
                            gps_location = COALESCE(@GpsLocation, gps_location),
                            checkin_time = COALESCE(@CheckinTime, checkin_time),
                            checkout_time = COALESCE(@CheckoutTime, checkout_time),
                            notes = COALESCE(@Notes, notes),
                            updated_at = NOW()
                        WHERE operation_record_id = @Id";

            await _connection.ExecuteAsync(sql, new
            {
                Id = id,
                dto.GpsLatitude,
                dto.GpsLongitude,
                dto.GpsLocation,
                dto.CheckinTime,
                dto.CheckoutTime,
                dto.Notes
            });

            return Ok(new { message = "Assistant operation updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating assistant operation with ID {OperationRecordId}", id);
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // POST: api/AssistantOperations/5/checkin
    [HttpPost("{id}/checkin")]
    public async Task<IActionResult> CheckIn(int id, [FromBody] CheckInDto dto)
    {
        try
        {
            if (_connection.State != System.Data.ConnectionState.Open)
                await _connection.OpenAsync();

            // Check if operation exists and get order_id
            var orderIdResult = await _connection.QueryFirstOrDefaultAsync<int?>(
                "SELECT order_id FROM AssistantOperations WHERE operation_record_id = @Id AND is_active = 'Y'",
                new { Id = id });

            if (!orderIdResult.HasValue)
            {
                return NotFound(new { message = $"Assistant operation with ID {id} not found" });
            }

            var orderId = orderIdResult.Value;

            // Update assistant operation check-in
            var sql = @"UPDATE AssistantOperations 
                        SET checkin_time = @CheckinTime,
                            gps_latitude = @GpsLatitude,
                            gps_longitude = @GpsLongitude,
                            gps_location = @GpsLocation,
                            notes = COALESCE(@Notes, notes),
                            updated_at = NOW()
                        WHERE operation_record_id = @Id";

            await _connection.ExecuteAsync(sql, new
            {
                Id = id,
                CheckinTime = dto.CheckinTime ?? DateTime.Now,
                dto.GpsLatitude,
                dto.GpsLongitude,
                dto.GpsLocation,
                dto.Notes
            });

            // Update order status to In-operation only if not already set
            try
            {
                // Check current order status
                var currentStatus = await _connection.QueryFirstOrDefaultAsync<string>(
                    "SELECT status FROM Orders WHERE order_id = @OrderId",
                    new { OrderId = orderId });

                // Only update if status is not already 'In-operation' or beyond
                if (currentStatus != "In-operation" && currentStatus != "Completed (Pre-Billing)")
                {
                    var updateOrderSql = @"UPDATE Orders 
                                           SET status = 'In-operation',
                                               updated_at = NOW()
                                           WHERE order_id = @OrderId";

                    await _connection.ExecuteAsync(updateOrderSql, new { OrderId = orderId });

                    // Create audit entry for order status change
                    var auditSql = @"INSERT INTO OrderAudits (order_id, action, action_by, action_at)
                                    VALUES (@OrderId, 'Status changed to In-operation (Assistant checked in)', 'System', NOW())";

                    await _connection.ExecuteAsync(auditSql, new { OrderId = orderId });

                    _logger.LogInformation("Order {OrderId} status updated to In-operation", orderId);
                }
                else
                {
                    _logger.LogInformation("Order {OrderId} status already set to {Status}, skipping update on check-in", orderId, currentStatus);
                }
            }
            catch (Exception orderEx)
            {
                _logger.LogError(orderEx, "Failed to update order status to In-operation for order {OrderId}. Check if 'In-operation' is allowed in status constraint.", orderId);
                // Still return success for check-in, but log the order update failure
            }

            return Ok(new { message = "Check-in recorded successfully and order status updated to In-operation" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording check-in for operation ID {OperationRecordId}", id);
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // POST: api/AssistantOperations/5/checkout
    [HttpPost("{id}/checkout")]
    public async Task<IActionResult> CheckOut(int id, [FromBody] CheckOutDto dto)
    {
        try
        {
            if (_connection.State != System.Data.ConnectionState.Open)
                await _connection.OpenAsync();

            // Check if operation exists and get order_id
            var orderIdResult = await _connection.QueryFirstOrDefaultAsync<int?>(
                "SELECT order_id FROM AssistantOperations WHERE operation_record_id = @Id AND is_active = 'Y'",
                new { Id = id });

            if (!orderIdResult.HasValue)
            {
                return NotFound(new { message = $"Assistant operation with ID {id} not found" });
            }

            var orderId = orderIdResult.Value;

            // Update assistant operation check-out
            var sql = @"UPDATE AssistantOperations 
                        SET checkout_time = @CheckoutTime,
                            gps_latitude = COALESCE(@GpsLatitude, gps_latitude),
                            gps_longitude = COALESCE(@GpsLongitude, gps_longitude),
                            gps_location = COALESCE(@GpsLocation, gps_location),
                            notes = COALESCE(@Notes, notes),
                            updated_at = NOW()
                        WHERE operation_record_id = @Id";

            await _connection.ExecuteAsync(sql, new
            {
                Id = id,
                CheckoutTime = dto.CheckoutTime ?? DateTime.Now,
                dto.GpsLatitude,
                dto.GpsLongitude,
                dto.GpsLocation,
                dto.Notes
            });

            // Update order status to Completed (Pre-Billing) only if not already set
            try
            {
                // Check current order status
                var currentStatus = await _connection.QueryFirstOrDefaultAsync<string>(
                    "SELECT status FROM Orders WHERE order_id = @OrderId",
                    new { OrderId = orderId });

                // Only update if status is not already 'Completed (Pre-Billing)'
                if (currentStatus != "Completed (Pre-Billing)")
                {
                    var updateOrderSql = @"UPDATE Orders 
                                           SET status = 'Completed (Pre-Billing)',
                                               updated_at = NOW()
                                           WHERE order_id = @OrderId";

                    await _connection.ExecuteAsync(updateOrderSql, new { OrderId = orderId });

                    // Create audit entry for order status change
                    var auditSql = @"INSERT INTO OrderAudits (order_id, action, action_by, action_at)
                                    VALUES (@OrderId, 'Status changed to Completed (Pre-Billing) (Assistant checked out)', 'System', NOW())";

                    await _connection.ExecuteAsync(auditSql, new { OrderId = orderId });

                    _logger.LogInformation("Order {OrderId} status updated to Completed (Pre-Billing)", orderId);
                }
                else
                {
                    _logger.LogInformation("Order {OrderId} status already set to Completed (Pre-Billing), skipping update on check-out", orderId);
                }
            }
            catch (Exception orderEx)
            {
                _logger.LogError(orderEx, "Failed to update order status to Completed (Pre-Billing) for order {OrderId}. Check if status is allowed in constraint.", orderId);
                // Still return success for check-out, but log the order update failure
            }

            return Ok(new { message = "Check-out recorded successfully and order status updated to Completed (Pre-Billing)" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording check-out for operation ID {OperationRecordId}", id);
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // DELETE: api/AssistantOperations/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOperation(int id)
    {
        try
        {
            if (_connection.State != System.Data.ConnectionState.Open)
                await _connection.OpenAsync();

            // Check if operation exists
            var exists = await _connection.ExecuteScalarAsync<bool>(
                "SELECT EXISTS(SELECT 1 FROM AssistantOperations WHERE operation_record_id = @Id AND is_active = 'Y')",
                new { Id = id });

            if (!exists)
            {
                return NotFound(new { message = $"Assistant operation with ID {id} not found" });
            }

            // Soft delete
            var sql = "UPDATE AssistantOperations SET is_active = 'N', updated_at = NOW() WHERE operation_record_id = @Id";
            await _connection.ExecuteAsync(sql, new { Id = id });

            return Ok(new { message = "Assistant operation deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting assistant operation with ID {OperationRecordId}", id);
            return StatusCode(500, new { message = ex.Message });
        }
    }
}

public class CheckInDto
{
    public DateTime? CheckinTime { get; set; }
    public decimal? GpsLatitude { get; set; }
    public decimal? GpsLongitude { get; set; }
    public string? GpsLocation { get; set; }
    public string? Notes { get; set; }
}

public class CheckOutDto
{
    public DateTime? CheckoutTime { get; set; }
    public decimal? GpsLatitude { get; set; }
    public decimal? GpsLongitude { get; set; }
    public string? GpsLocation { get; set; }
    public string? Notes { get; set; }
}
