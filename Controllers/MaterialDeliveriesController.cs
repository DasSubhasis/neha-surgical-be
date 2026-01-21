using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using NehaSurgicalAPI.DTOs;
using NehaSurgicalAPI.Models;
using NehaSurgicalAPI.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace NehaSurgicalAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MaterialDeliveriesController : ControllerBase
{
    private readonly NpgsqlConnection _connection;
    private readonly ILogger<MaterialDeliveriesController> _logger;

    public MaterialDeliveriesController(NpgsqlConnection connection, ILogger<MaterialDeliveriesController> logger)
    {
        _connection = connection;
        _logger = logger;
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
    }

    // GET: api/MaterialDeliveries
    [HttpGet]
    public async Task<IActionResult> GetAllDeliveries([FromQuery] string? deliveryStatus = null, [FromQuery] int? deliveredById = null)
    {
        try
        {
            if (_connection.State != System.Data.ConnectionState.Open)
                await _connection.OpenAsync();

            var sql = @"SELECT md.delivery_id as DeliveryId, md.order_id as OrderId, o.order_no as OrderNo, 
                        d.doctor_name as DoctorName, h.name as HospitalName,
                        o.operation_date as OperationDate, o.operation_time as OperationTime,
                        md.delivery_date as DeliveryDate, md.delivered_by_user_id as DeliveredById, 
                        COALESCE(u.full_name, md.delivered_by) as DeliveredBy, md.remarks as Remarks, 
                        md.delivery_status as DeliveryStatus, md.created_by as CreatedBy, md.updated_by as UpdatedBy, 
                        md.created_at as CreatedAt, md.updated_at as UpdatedAt 
                        FROM MaterialDeliveries md
                        INNER JOIN Orders o ON md.order_id = o.order_id
                        INNER JOIN Doctors d ON o.doctor_id = d.doctor_id
                        INNER JOIN Hospitals h ON o.hospital_id = h.hospital_id
                        LEFT JOIN SystemUsers u ON md.delivered_by_user_id = u.system_user_id
                        WHERE md.is_active = 'Y'";

            if (!string.IsNullOrEmpty(deliveryStatus))
            {
                sql += " AND md.delivery_status = @DeliveryStatus";
            }

            if (deliveredById.HasValue)
            {
                sql += " AND md.delivered_by_user_id = @DeliveredById";
            }

            sql += " ORDER BY md.delivery_date DESC";

            var deliveries = await _connection.QueryAsync<dynamic>(sql, new { DeliveryStatus = deliveryStatus, DeliveredById = deliveredById });

            var deliveryDtos = deliveries.Select(d => new MaterialDeliveryDto
            {
                DeliveryId = (int)d.deliveryid,
                OrderId = (int)d.orderid,
                OrderNo = (string)d.orderno,
                DoctorName = (string)d.doctorname,
                HospitalName = (string)d.hospitalname,
                OperationDate = d.operationdate is DateTime dt1 ? DateOnly.FromDateTime(dt1).ToString("yyyy-MM-dd") : ((DateOnly)d.operationdate).ToString("yyyy-MM-dd"),
                OperationTime = d.operationtime is TimeSpan ts ? TimeOnly.FromTimeSpan(ts).ToString("HH:mm") : ((TimeOnly)d.operationtime).ToString("HH:mm"),
                DeliveryDate = d.deliverydate is DateTime dt ? DateOnly.FromDateTime(dt).ToString("yyyy-MM-dd") : ((DateOnly)d.deliverydate).ToString("yyyy-MM-dd"),
                DeliveredById = d.deliveredbyid != null ? (int?)d.deliveredbyid : null,
                DeliveredBy = (string)d.deliveredby,
                Remarks = d.remarks != null ? (string)d.remarks : null,
                DeliveryStatus = (string)d.deliverystatus,
                CreatedBy = (string)d.createdby,
                UpdatedBy = d.updatedby != null ? (string)d.updatedby : null,
                CreatedAt = ((DateTime)d.createdat).ToString("yyyy-MM-dd HH:mm:ss"),
                UpdatedAt = ((DateTime)d.updatedat).ToString("yyyy-MM-dd HH:mm:ss")
            }).ToList();

            return Ok(new { message = "Material deliveries retrieved successfully", data = deliveryDtos });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving material deliveries");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // GET: api/MaterialDeliveries/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDeliveryById(int id)
    {
        try
        {
            if (_connection.State != System.Data.ConnectionState.Open)
                await _connection.OpenAsync();

            var sql = @"SELECT md.delivery_id as DeliveryId, md.order_id as OrderId, o.order_no as OrderNo, 
                        d.doctor_name as DoctorName, h.name as HospitalName,
                        o.operation_date as OperationDate, o.operation_time as OperationTime,
                        md.delivery_date as DeliveryDate, md.delivered_by as DeliveredBy, md.remarks as Remarks, 
                        md.delivery_status as DeliveryStatus, md.created_by as CreatedBy, md.updated_by as UpdatedBy, 
                        md.created_at as CreatedAt, md.updated_at as UpdatedAt 
                        FROM MaterialDeliveries md
                        INNER JOIN Orders o ON md.order_id = o.order_id
                        INNER JOIN Doctors d ON o.doctor_id = d.doctor_id
                        INNER JOIN Hospitals h ON o.hospital_id = h.hospital_id
                        WHERE md.delivery_id = @Id AND md.is_active = 'Y'";

            var delivery = await _connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { Id = id });

            if (delivery == null)
            {
                return NotFound(new { message = $"Material delivery with ID {id} not found" });
            }

            var deliveryDto = new MaterialDeliveryDto
            {
                DeliveryId = (int)delivery.deliveryid,
                OrderId = (int)delivery.orderid,
                OrderNo = (string)delivery.orderno,
                DoctorName = (string)delivery.doctorname,
                HospitalName = (string)delivery.hospitalname,
                OperationDate = delivery.operationdate is DateTime dt1 ? DateOnly.FromDateTime(dt1).ToString("yyyy-MM-dd") : ((DateOnly)delivery.operationdate).ToString("yyyy-MM-dd"),
                OperationTime = delivery.operationtime is TimeSpan ts ? TimeOnly.FromTimeSpan(ts).ToString("HH:mm") : ((TimeOnly)delivery.operationtime).ToString("HH:mm"),
                DeliveryDate = delivery.deliverydate is DateTime dt ? DateOnly.FromDateTime(dt).ToString("yyyy-MM-dd") : ((DateOnly)delivery.deliverydate).ToString("yyyy-MM-dd"),
                DeliveredBy = (string)delivery.deliveredby,
                Remarks = delivery.remarks != null ? (string)delivery.remarks : null,
                DeliveryStatus = (string)delivery.deliverystatus,
                CreatedBy = (string)delivery.createdby,
                UpdatedBy = delivery.updatedby != null ? (string)delivery.updatedby : null,
                CreatedAt = ((DateTime)delivery.createdat).ToString("yyyy-MM-dd HH:mm:ss"),
                UpdatedAt = ((DateTime)delivery.updatedat).ToString("yyyy-MM-dd HH:mm:ss")
            };

            return Ok(new { message = "Material delivery retrieved successfully", data = deliveryDto });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving material delivery with ID {DeliveryId}", id);
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // GET: api/MaterialDeliveries/order/5
    [HttpGet("order/{orderId}")]
    public async Task<IActionResult> GetDeliveriesByOrderId(int orderId)
    {
        try
        {
            if (_connection.State != System.Data.ConnectionState.Open)
                await _connection.OpenAsync();

            var sql = @"SELECT md.delivery_id as DeliveryId, md.order_id as OrderId, o.order_no as OrderNo,
                        d.doctor_name as DoctorName, h.hospital_name as HospitalName,
                        o.operation_date as OperationDate, o.operation_time as OperationTime,
                        md.delivery_date as DeliveryDate, md.delivered_by as DeliveredBy, 
                        md.remarks as Remarks, md.delivery_status as DeliveryStatus,
                        md.created_by as CreatedBy, md.updated_by as UpdatedBy,
                        md.created_at as CreatedAt, md.updated_at as UpdatedAt
                        FROM MaterialDeliveries md
                        INNER JOIN Orders o ON md.order_id = o.order_id
                        INNER JOIN Doctors d ON o.doctor_id = d.doctor_id
                        INNER JOIN Hospitals h ON o.hospital_id = h.hospital_id
                        WHERE md.order_id = @OrderId AND md.is_active = 'Y'
                        ORDER BY md.delivery_date DESC";

            var deliveries = await _connection.QueryAsync<dynamic>(sql, new { OrderId = orderId });

            var deliveryDtos = deliveries.Select(d => new MaterialDeliveryDto
            {
                DeliveryId = (int)d.deliveryid,
                OrderId = (int)d.orderid,
                OrderNo = (string)d.orderno,
                DoctorName = (string)d.doctorname,
                HospitalName = (string)d.hospitalname,
                OperationDate = d.operationdate is DateTime dt1 ? DateOnly.FromDateTime(dt1).ToString("yyyy-MM-dd") : ((DateOnly)d.operationdate).ToString("yyyy-MM-dd"),
                OperationTime = d.operationtime is TimeSpan ts ? TimeOnly.FromTimeSpan(ts).ToString("HH:mm") : ((TimeOnly)d.operationtime).ToString("HH:mm"),
                DeliveryDate = d.deliverydate is DateTime dt ? DateOnly.FromDateTime(dt).ToString("yyyy-MM-dd") : ((DateOnly)d.deliverydate).ToString("yyyy-MM-dd"),
                DeliveredBy = (string)d.deliveredby,
                Remarks = d.remarks != null ? (string)d.remarks : null,
                DeliveryStatus = (string)d.deliverystatus,
                CreatedBy = (string)d.createdby,
                UpdatedBy = d.updatedby != null ? (string)d.updatedby : null,
                CreatedAt = ((DateTime)d.createdat).ToString("yyyy-MM-dd HH:mm:ss"),
                UpdatedAt = ((DateTime)d.updatedat).ToString("yyyy-MM-dd HH:mm:ss")
            }).ToList();

            return Ok(new { message = "Material deliveries retrieved successfully", data = deliveryDtos });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving material deliveries for order {OrderId}", orderId);
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // POST: api/MaterialDeliveries
    [HttpPost]
    public async Task<IActionResult> CreateDelivery([FromBody] CreateMaterialDeliveryDto deliveryDto)
    {
        try
        {
            if (_connection.State != System.Data.ConnectionState.Open)
                await _connection.OpenAsync();

            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Validation failed", errors = ModelState });
            }

            // Validate delivery date
            if (!DateOnly.TryParse(deliveryDto.DeliveryDate, out var deliveryDate))
            {
                return BadRequest(new { message = "Invalid delivery date format. Use yyyy-MM-dd" });
            }

            // Check if order exists
            var orderCheckSql = "SELECT COUNT(*) FROM Orders WHERE order_id = @OrderId AND is_active = 'Y'";
            var orderExists = await _connection.ExecuteScalarAsync<int>(orderCheckSql, new { OrderId = deliveryDto.OrderId });

            if (orderExists == 0)
            {
                return NotFound(new { message = $"Order with ID {deliveryDto.OrderId} not found" });
            }

            // Validate that the user exists
            var userCheckSql = "SELECT full_name FROM SystemUsers WHERE system_user_id = @UserId AND is_active = 'Y'";
            var userName = await _connection.QueryFirstOrDefaultAsync<string>(userCheckSql, new { UserId = deliveryDto.DeliveredById });

            if (userName == null)
            {
                return NotFound(new { message = $"User with ID {deliveryDto.DeliveredById} not found" });
            }

            var sql = @"INSERT INTO MaterialDeliveries (order_id, delivery_date, delivered_by, delivered_by_user_id, remarks, delivery_status, created_by, created_at, updated_at)
                        VALUES (@OrderId, @DeliveryDate, @DeliveredBy, @DeliveredById, @Remarks, @DeliveryStatus, @CreatedBy, NOW(), NOW())
                        RETURNING delivery_id";

            var deliveryId = await _connection.ExecuteScalarAsync<int>(sql, new
            {
                OrderId = deliveryDto.OrderId,
                DeliveryDate = deliveryDate,
                DeliveredBy = userName,
                DeliveredById = deliveryDto.DeliveredById,
                Remarks = deliveryDto.Remarks,
                DeliveryStatus = deliveryDto.DeliveryStatus,
                CreatedBy = deliveryDto.CreatedBy
            });

            // Update order's is_delivered status if delivery status is 'Delivered'
            if (deliveryDto.DeliveryStatus == "Delivered")
            {
                var updateOrderSql = "UPDATE Orders SET is_delivered = 'Delivered', updated_at = NOW() WHERE order_id = @OrderId";
                await _connection.ExecuteAsync(updateOrderSql, new { OrderId = deliveryDto.OrderId });
            }
            else if (deliveryDto.DeliveryStatus == "Assigned")
            {
                var updateOrderSql = "UPDATE Orders SET is_delivered = 'Assigned', updated_at = NOW() WHERE order_id = @OrderId";
                await _connection.ExecuteAsync(updateOrderSql, new { OrderId = deliveryDto.OrderId });
            }

            // Get order details for response
            var orderDetailsSql = @"SELECT o.order_no as OrderNo, d.doctor_name as DoctorName, h.name as HospitalName,
                                    o.operation_date as OperationDate, o.operation_time as OperationTime
                                    FROM Orders o
                                    INNER JOIN Doctors d ON o.doctor_id = d.doctor_id
                                    INNER JOIN Hospitals h ON o.hospital_id = h.hospital_id
                                    WHERE o.order_id = @OrderId";
            var orderDetails = await _connection.QueryFirstOrDefaultAsync<dynamic>(orderDetailsSql, new { OrderId = deliveryDto.OrderId });

            var createdDelivery = new MaterialDeliveryDto
            {
                DeliveryId = deliveryId,
                OrderId = deliveryDto.OrderId,
                OrderNo = orderDetails?.orderno ?? string.Empty,
                DoctorName = orderDetails?.doctorname ?? string.Empty,
                HospitalName = orderDetails?.hospitalname ?? string.Empty,
                OperationDate = orderDetails?.operationdate is DateTime dt1 ? DateOnly.FromDateTime(dt1).ToString("yyyy-MM-dd") : (orderDetails?.operationdate != null ? ((DateOnly)orderDetails.operationdate).ToString("yyyy-MM-dd") : string.Empty),
                OperationTime = orderDetails?.operationtime is TimeSpan ts ? TimeOnly.FromTimeSpan(ts).ToString("HH:mm") : (orderDetails?.operationtime != null ? ((TimeOnly)orderDetails.operationtime).ToString("HH:mm") : string.Empty),
                DeliveryDate = deliveryDate.ToString("yyyy-MM-dd"),
                DeliveredById = deliveryDto.DeliveredById,
                DeliveredBy = userName,
                Remarks = deliveryDto.Remarks,
                DeliveryStatus = deliveryDto.DeliveryStatus,
                CreatedBy = deliveryDto.CreatedBy,
                UpdatedBy = null,
                CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                UpdatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            return CreatedAtAction(nameof(GetDeliveryById), new { id = deliveryId },
                new { message = "Material delivery created successfully", data = createdDelivery });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating material delivery");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // PUT: api/MaterialDeliveries/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDelivery(int id, [FromBody] UpdateMaterialDeliveryDto deliveryDto)
    {
        try
        {
            if (_connection.State != System.Data.ConnectionState.Open)
                await _connection.OpenAsync();

            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Validation failed", errors = ModelState });
            }

            // Check if delivery exists
            var checkSql = "SELECT order_id FROM MaterialDeliveries WHERE delivery_id = @Id AND is_active = 'Y'";
            var existingOrderId = await _connection.QueryFirstOrDefaultAsync<int?>(checkSql, new { Id = id });

            if (existingOrderId == null)
            {
                return NotFound(new { message = $"Material delivery with ID {id} not found" });
            }

            var updates = new List<string>();
            var parameters = new DynamicParameters();
            parameters.Add("Id", id);

            if (deliveryDto.OrderId.HasValue)
            {
                // Check if new order exists
                var orderCheckSql = "SELECT COUNT(*) FROM Orders WHERE order_id = @OrderId AND is_active = 'Y'";
                var orderExists = await _connection.ExecuteScalarAsync<int>(orderCheckSql, new { OrderId = deliveryDto.OrderId.Value });

                if (orderExists == 0)
                {
                    return NotFound(new { message = $"Order with ID {deliveryDto.OrderId.Value} not found" });
                }

                updates.Add("order_id = @OrderId");
                parameters.Add("OrderId", deliveryDto.OrderId.Value);
            }

            if (!string.IsNullOrEmpty(deliveryDto.DeliveryDate))
            {
                if (!DateOnly.TryParse(deliveryDto.DeliveryDate, out var deliveryDate))
                {
                    return BadRequest(new { message = "Invalid delivery date format. Use yyyy-MM-dd" });
                }
                updates.Add("delivery_date = @DeliveryDate");
                parameters.Add("DeliveryDate", deliveryDate);
            }

            if (deliveryDto.DeliveredById.HasValue)
            {
                // Validate that the user exists and get their name
                var userCheckSql = "SELECT full_name FROM SystemUsers WHERE system_user_id = @UserId AND is_active = 'Y'";
                var deliveredByUserName = await _connection.QueryFirstOrDefaultAsync<string>(userCheckSql, new { UserId = deliveryDto.DeliveredById.Value });

                if (deliveredByUserName == null)
                {
                    return NotFound(new { message = $"User with ID {deliveryDto.DeliveredById.Value} not found" });
                }

                updates.Add("delivered_by = @DeliveredBy");
                updates.Add("delivered_by_user_id = @DeliveredById");
                parameters.Add("DeliveredBy", deliveredByUserName);
                parameters.Add("DeliveredById", deliveryDto.DeliveredById.Value);
            }

            if (deliveryDto.Remarks != null)
            {
                updates.Add("remarks = @Remarks");
                parameters.Add("Remarks", deliveryDto.Remarks);
            }

            if (!string.IsNullOrEmpty(deliveryDto.DeliveryStatus))
            {
                updates.Add("delivery_status = @DeliveryStatus");
                parameters.Add("DeliveryStatus", deliveryDto.DeliveryStatus);

                // Update order's is_delivered status
                var orderId = deliveryDto.OrderId ?? existingOrderId.Value;
                if (deliveryDto.DeliveryStatus == "Delivered")
                {
                    var updateOrderSql = "UPDATE Orders SET is_delivered = 'Delivered', updated_at = NOW() WHERE order_id = @OrderId";
                    await _connection.ExecuteAsync(updateOrderSql, new { OrderId = orderId });
                }
                else if (deliveryDto.DeliveryStatus == "Assigned")
                {
                    var updateOrderSql = "UPDATE Orders SET is_delivered = 'Assigned', updated_at = NOW() WHERE order_id = @OrderId";
                    await _connection.ExecuteAsync(updateOrderSql, new { OrderId = orderId });
                }
            }

            if (!string.IsNullOrEmpty(deliveryDto.UpdatedBy))
            {
                updates.Add("updated_by = @UpdatedBy");
                parameters.Add("UpdatedBy", deliveryDto.UpdatedBy);
            }

            if (updates.Count == 0)
            {
                return BadRequest(new { message = "No fields to update" });
            }

            updates.Add("updated_at = NOW()");

            var sql = $"UPDATE MaterialDeliveries SET {string.Join(", ", updates)} WHERE delivery_id = @Id";
            await _connection.ExecuteAsync(sql, parameters);

            return Ok(new { message = "Material delivery updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating material delivery with ID {DeliveryId}", id);
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // DELETE: api/MaterialDeliveries/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDelivery(int id)
    {
        try
        {
            if (_connection.State != System.Data.ConnectionState.Open)
                await _connection.OpenAsync();

            var checkSql = "SELECT COUNT(*) FROM MaterialDeliveries WHERE delivery_id = @Id AND is_active = 'Y'";
            var exists = await _connection.ExecuteScalarAsync<int>(checkSql, new { Id = id });

            if (exists == 0)
            {
                return NotFound(new { message = $"Material delivery with ID {id} not found" });
            }

            // Soft delete
            var deleteSql = "UPDATE MaterialDeliveries SET is_active = 'N', updated_at = NOW() WHERE delivery_id = @Id";
            await _connection.ExecuteAsync(deleteSql, new { Id = id });

            return Ok(new { message = "Material delivery deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting material delivery with ID {DeliveryId}", id);
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // POST: api/MaterialDeliveries/5/mark-delivered
    [HttpPost("{id}/mark-delivered")]
    public async Task<IActionResult> MarkAsDelivered(int id, [FromBody] MarkAsDeliveredDto request)
    {
        try
        {
            if (_connection.State != System.Data.ConnectionState.Open)
                await _connection.OpenAsync();

            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Validation failed", errors = ModelState });
            }

            // Check if delivery exists
            var checkSql = @"SELECT order_id, delivery_status 
                            FROM MaterialDeliveries 
                            WHERE delivery_id = @Id AND is_active = 'Y'";
            var delivery = await _connection.QueryFirstOrDefaultAsync<dynamic>(checkSql, new { Id = id });

            if (delivery == null)
            {
                return NotFound(new { message = $"Material delivery with ID {id} not found" });
            }

            // Check if already delivered
            if (delivery.delivery_status == "Delivered")
            {
                return BadRequest(new { message = "This delivery has already been marked as delivered" });
            }

            var currentTimestamp = DateTime.Now;

            // Get user name from database
            var userSql = "SELECT full_name FROM SystemUsers WHERE system_user_id = @UserId AND is_active = 'Y'";
            var userName = await _connection.QueryFirstOrDefaultAsync<string>(userSql, new { UserId = request.DeliveredById });

            if (userName == null)
            {
                return NotFound(new { message = $"User with ID {request.DeliveredById} not found" });
            }

            // Update delivery to mark as delivered
            var updateSql = @"UPDATE MaterialDeliveries 
                             SET delivery_status = 'Delivered',
                                 remarks = COALESCE(@Remarks, remarks),
                                 actual_delivery_by = @ActualDeliveryBy,
                                 actual_delivery_by_userid = @ActualDeliveryByUserId,
                                 actual_delivery_time = @ActualDeliveryTime
                             WHERE delivery_id = @Id";

            await _connection.ExecuteAsync(updateSql, new
            {
                Id = id,
                DeliveryDate = DateOnly.FromDateTime(currentTimestamp),
                DeliveredBy = userName,
                DeliveredById = request.DeliveredById,
                Remarks = request.Remarks,
                ActualDeliveryBy = userName,
                ActualDeliveryByUserId = request.DeliveredById,
                ActualDeliveryTime = currentTimestamp
            });

            // Update order's is_delivered status
            var updateOrderSql = @"UPDATE Orders 
                                  SET is_delivered = 'Delivered', 
                                      updated_at = @UpdatedAt 
                                  WHERE order_id = @OrderId";
            await _connection.ExecuteAsync(updateOrderSql, new
            {
                OrderId = delivery.order_id,
                UpdatedAt = currentTimestamp
            });

            return Ok(new
            {
                message = "Material delivery marked as delivered successfully",
                data = new
                {
                    deliveryId = id,
                    deliveredById = request.DeliveredById,
                    deliveredBy = userName,
                    deliveryDate = DateOnly.FromDateTime(currentTimestamp).ToString("yyyy-MM-dd"),
                    deliveryTime = currentTimestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                    deliveryStatus = "Delivered"
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking material delivery {DeliveryId} as delivered", id);
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
