using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Dapper;
using NehaSurgicalAPI.Models;
using NehaSurgicalAPI.DTOs;
using NehaSurgicalAPI.Data;

namespace NehaSurgicalAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly NpgsqlConnection _connection;

    public OrdersController(NpgsqlConnection connection)
    {
        _connection = connection;
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
        SqlMapper.AddTypeHandler(new TimeOnlyTypeHandler());
    }

    // GET: api/Orders?status=Pending&isDelivered=Pending&assignedId=4
    [HttpGet]
    public async Task<IActionResult> GetAllOrders([FromQuery] string? status = null, [FromQuery] string? isDelivered = null, [FromQuery] int? assignedId = null)
    {
        try
        {
            if (_connection.State != System.Data.ConnectionState.Open)
                await _connection.OpenAsync();

            var sql = @"
                SELECT 
                    o.order_id as OrderId,
                    o.order_no as OrderNo,
                    o.order_date as OrderDate,
                    o.doctor_id as DoctorId,
                    d.doctor_name as DoctorName,
                    o.hospital_id as HospitalId,
                    h.name as HospitalName,
                    o.operation_date as OperationDate,
                    o.operation_time as OperationTime,
                    o.material_send_date as MaterialSendDate,
                    o.remarks as Remarks,
                    o.created_by as CreatedBy,
                    o.status as Status,
                    o.is_delivered as IsDelivered,
                    o.created_at as CreatedAt,
                    o.updated_at as UpdatedAt
                FROM Orders o
                INNER JOIN Doctors d ON o.doctor_id = d.doctor_id
                INNER JOIN Hospitals h ON o.hospital_id = h.hospital_id";

            if (assignedId.HasValue)
            {
                sql += @"
                LEFT JOIN AssistantAssignments aa ON o.order_id = aa.order_id AND aa.is_active = 'Y'
                LEFT JOIN SubAssistantAssignments sa ON aa.assignment_id = sa.assignment_id AND sa.is_active = 'Y'
                WHERE o.is_active = 'Y' AND (aa.assistant_id = @AssignedId OR sa.sub_assistant_id = @AssignedId)";
            }
            else
            {
                sql += " WHERE o.is_active = 'Y'";
            }

            if (!string.IsNullOrEmpty(status))
            {
                sql += " AND o.status = @Status";
            }

            if (!string.IsNullOrEmpty(isDelivered))
            {
                sql += " AND o.is_delivered = @IsDelivered";
            }

            sql += " ORDER BY o.order_id DESC";

            var orders = (await _connection.QueryAsync<dynamic>(sql, new { Status = status, IsDelivered = isDelivered, AssignedId = assignedId })).ToList();
            
            var orderDtos = new List<OrderDto>();
            foreach (var order in orders)
            {
                var orderDto = await MapToOrderDto(order);
                orderDtos.Add(orderDto);
            }

            return Ok(new { message = "Orders retrieved successfully", data = orderDtos });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // GET: api/Orders/not-delivered
    [HttpGet("not-delivered")]
    public async Task<IActionResult> GetNotDeliveredOrders()
    {
        try
        {
            if (_connection.State != System.Data.ConnectionState.Open)
                await _connection.OpenAsync();

            var sql = @"
                SELECT 
                    o.order_id as OrderId,
                    o.order_no as OrderNo,
                    o.order_date as OrderDate,
                    o.doctor_id as DoctorId,
                    d.doctor_name as DoctorName,
                    o.hospital_id as HospitalId,
                    h.name as HospitalName,
                    o.operation_date as OperationDate,
                    o.operation_time as OperationTime,
                    o.material_send_date as MaterialSendDate,
                    o.remarks as Remarks,
                    o.created_by as CreatedBy,
                    o.status as Status,
                    o.is_delivered as IsDelivered,
                    o.created_at as CreatedAt,
                    o.updated_at as UpdatedAt
                FROM Orders o
                INNER JOIN Doctors d ON o.doctor_id = d.doctor_id
                INNER JOIN Hospitals h ON o.hospital_id = h.hospital_id
                WHERE o.is_active = 'Y' AND o.is_delivered != 'Delivered'
                ORDER BY o.order_id DESC";

            var orders = (await _connection.QueryAsync<dynamic>(sql)).ToList();
            
            var orderDtos = new List<OrderDto>();
            foreach (var order in orders)
            {
                var orderDto = await MapToOrderDto(order);
                orderDtos.Add(orderDto);
            }

            return Ok(new { message = "Not delivered orders retrieved successfully", data = orderDtos });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // GET: api/Orders/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderById(int id)
    {
        try
        {
            if (_connection.State != System.Data.ConnectionState.Open)
                await _connection.OpenAsync();

            var sql = @"
                SELECT 
                    o.order_id as OrderId,
                    o.order_no as OrderNo,
                    o.order_date as OrderDate,
                    o.doctor_id as DoctorId,
                    d.doctor_name as DoctorName,
                    o.hospital_id as HospitalId,
                    h.name as HospitalName,
                    o.operation_date as OperationDate,
                    o.operation_time as OperationTime,
                    o.material_send_date as MaterialSendDate,
                    o.remarks as Remarks,
                    o.created_by as CreatedBy,
                    o.status as Status,
                    o.is_delivered as IsDelivered,
                    o.created_at as CreatedAt,
                    o.updated_at as UpdatedAt
                FROM Orders o
                INNER JOIN Doctors d ON o.doctor_id = d.doctor_id
                INNER JOIN Hospitals h ON o.hospital_id = h.hospital_id
                WHERE o.order_id = @OrderId AND o.is_active = 'Y'";

            var order = await _connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { OrderId = id });
            
            if (order == null)
            {
                return NotFound(new { message = $"Order with ID {id} not found" });
            }

            var orderDto = await MapToOrderDto(order);
            return Ok(new { message = "Order retrieved successfully", data = orderDto });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // POST: api/Orders
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto orderDto)
    {
        try
        {
            if (_connection.State != System.Data.ConnectionState.Open)
                await _connection.OpenAsync();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate dates
            if (!DateOnly.TryParse(orderDto.OrderDate, out var orderDate))
                return BadRequest(new { message = "Invalid order date format" });
            
            if (!DateOnly.TryParse(orderDto.OperationDate, out var operationDate))
                return BadRequest(new { message = "Invalid operation date format" });
            
            if (!DateOnly.TryParse(orderDto.MaterialSendDate, out var materialSendDate))
                return BadRequest(new { message = "Invalid material send date format" });
            
            if (!TimeOnly.TryParse(orderDto.OperationTime, out var operationTime))
                return BadRequest(new { message = "Invalid operation time format" });

            using var transaction = await _connection.BeginTransactionAsync();
            try
            {
                // Insert order with temporary order number
                var insertOrderSql = @"
                    INSERT INTO Orders (order_no, order_date, doctor_id, hospital_id, operation_date, 
                                      operation_time, material_send_date, remarks, created_by, status, 
                                      is_active, created_at, updated_at)
                    VALUES ('TEMP', @OrderDate, @DoctorId, @HospitalId, @OperationDate, 
                           @OperationTime, @MaterialSendDate, @Remarks, @CreatedBy, 'Pending', 
                           'Y', NOW(), NOW())
                    RETURNING order_id";

                var orderId = await _connection.ExecuteScalarAsync<int>(insertOrderSql, new
                {
                    OrderDate = orderDate,
                    orderDto.DoctorId,
                    orderDto.HospitalId,
                    OperationDate = operationDate,
                    OperationTime = operationTime,
                    MaterialSendDate = materialSendDate,
                    orderDto.Remarks,
                    orderDto.CreatedBy
                }, transaction);

                // Generate and update order number based on order_id
                var orderNo = $"ORD-{orderId:D6}";
                var updateOrderNoSql = "UPDATE Orders SET order_no = @OrderNo WHERE order_id = @OrderId";
                await _connection.ExecuteAsync(updateOrderNoSql, new { OrderNo = orderNo, OrderId = orderId }, transaction);

                // Insert item groups
                var itemGroupsNotFound = new List<string>();
                if (orderDto.ItemGroups != null && orderDto.ItemGroups.Count > 0)
                {
                    foreach (var itemGroupValue in orderDto.ItemGroups)
                    {
                        dynamic? itemGroup = null;
                        
                        // Try to parse as ID first, otherwise treat as name
                        if (int.TryParse(itemGroupValue, out var itemGroupId))
                        {
                            var itemGroupSql = "SELECT item_group_id, name FROM ItemGroups WHERE item_group_id = @Id AND is_active = 'Y'";
                            itemGroup = await _connection.QueryFirstOrDefaultAsync<dynamic>(itemGroupSql, 
                                new { Id = itemGroupId }, transaction);
                            
                            if (itemGroup == null)
                            {
                                itemGroupsNotFound.Add($"ID: {itemGroupId}");
                            }
                        }
                        else
                        {
                            var itemGroupSql = "SELECT item_group_id, name FROM ItemGroups WHERE name = @Name AND is_active = 'Y'";
                            itemGroup = await _connection.QueryFirstOrDefaultAsync<dynamic>(itemGroupSql, 
                                new { Name = itemGroupValue }, transaction);
                            
                            if (itemGroup == null)
                            {
                                itemGroupsNotFound.Add($"Name: {itemGroupValue}");
                            }
                        }

                        if (itemGroup != null)
                        {
                            var insertItemGroupSql = @"
                                INSERT INTO OrderItemGroups (order_id, item_group_id, item_group_name, created_at)
                                VALUES (@OrderId, @ItemGroupId, @ItemGroupName, NOW())";
                            
                            await _connection.ExecuteAsync(insertItemGroupSql, new
                            {
                                OrderId = orderId,
                                ItemGroupId = (int)itemGroup.item_group_id,
                                ItemGroupName = (string)itemGroup.name
                            }, transaction);
                        }
                    }
                }
                
                // Log warning if some item groups were not found (but don't fail the transaction)
                if (itemGroupsNotFound.Count > 0)
                {
                    Console.WriteLine($"Warning: Item groups not found: {string.Join(", ", itemGroupsNotFound)}");
                }

                // Insert items
                if (orderDto.Items != null && orderDto.Items.Count > 0)
                {
                    foreach (var item in orderDto.Items)
                    {
                        int? itemId = null;
                        int? itemGroupId = null;
                        var itemName = item.Name;

                        // If it's a group item, get the item group ID
                        if (item.IsGroup == true)
                        {
                            var itemGroupSql = "SELECT item_group_id FROM ItemGroups WHERE name = @Name AND is_active = 'Y'";
                            itemGroupId = await _connection.QueryFirstOrDefaultAsync<int?>(itemGroupSql, 
                                new { Name = item.Name }, transaction);
                        }
                        // If it's not manual, try to get the item ID
                        else if (item.Manual != true)
                        {
                            var itemSql = "SELECT item_id FROM Items WHERE name = @Name AND is_active = 'Y'";
                            itemId = await _connection.QueryFirstOrDefaultAsync<int?>(itemSql, 
                                new { Name = item.Name }, transaction);
                        }

                        var insertItemSql = @"
                            INSERT INTO OrderItems (order_id, item_id, item_group_id, item_name, is_manual, is_group, quantity, created_at)
                            VALUES (@OrderId, @ItemId, @ItemGroupId, @ItemName, @IsManual, @IsGroup, @Quantity, NOW())";
                        
                        await _connection.ExecuteAsync(insertItemSql, new
                        {
                            OrderId = orderId,
                            ItemId = itemId,
                            ItemGroupId = itemGroupId,
                            ItemName = itemName,
                            IsManual = item.Manual == true ? "Y" : "N",
                            IsGroup = item.IsGroup == true ? "Y" : "N",
                            Quantity = item.Quantity
                        }, transaction);
                    }
                }

                // Insert audit record
                var insertAuditSql = @"
                    INSERT INTO OrderAudits (order_id, action, performed_by, performed_at)
                    VALUES (@OrderId, 'Created', @PerformedBy, NOW())";
                
                await _connection.ExecuteAsync(insertAuditSql, new
                {
                    OrderId = orderId,
                    PerformedBy = orderDto.CreatedBy
                }, transaction);

                await transaction.CommitAsync();

                // Retrieve the created order
                var createdOrder = await GetOrderByIdInternal(orderId);
                return CreatedAtAction(nameof(GetOrderById), new { id = orderId },
                    new { message = "Order created successfully", data = createdOrder });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "23505")
        {
            return Conflict(new { message = "An order with this order number already exists" });
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "23503")
        {
            return BadRequest(new { message = "Invalid doctor or hospital ID" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // PUT: api/Orders/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrder(int id, [FromBody] UpdateOrderDto orderDto)
    {
        try
        {
            if (_connection.State != System.Data.ConnectionState.Open)
                await _connection.OpenAsync();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if order exists
            var checkSql = "SELECT COUNT(*) FROM Orders WHERE order_id = @OrderId AND is_active = 'Y'";
            var exists = await _connection.ExecuteScalarAsync<int>(checkSql, new { OrderId = id });
            
            if (exists == 0)
            {
                return NotFound(new { message = $"Order with ID {id} not found" });
            }

            using var transaction = await _connection.BeginTransactionAsync();
            try
            {
                var updateFields = new List<string>();
                var parameters = new DynamicParameters();
                parameters.Add("OrderId", id);

                if (!string.IsNullOrEmpty(orderDto.OrderNo))
                {
                    updateFields.Add("order_no = @OrderNo");
                    parameters.Add("OrderNo", orderDto.OrderNo);
                }

                if (!string.IsNullOrEmpty(orderDto.OrderDate))
                {
                    if (DateOnly.TryParse(orderDto.OrderDate, out var orderDate))
                    {
                        updateFields.Add("order_date = @OrderDate");
                        parameters.Add("OrderDate", orderDate);
                    }
                }

                if (orderDto.DoctorId.HasValue)
                {
                    updateFields.Add("doctor_id = @DoctorId");
                    parameters.Add("DoctorId", orderDto.DoctorId.Value);
                }

                if (orderDto.HospitalId.HasValue)
                {
                    updateFields.Add("hospital_id = @HospitalId");
                    parameters.Add("HospitalId", orderDto.HospitalId.Value);
                }

                if (!string.IsNullOrEmpty(orderDto.OperationDate))
                {
                    if (DateOnly.TryParse(orderDto.OperationDate, out var operationDate))
                    {
                        updateFields.Add("operation_date = @OperationDate");
                        parameters.Add("OperationDate", operationDate);
                    }
                }

                if (!string.IsNullOrEmpty(orderDto.OperationTime))
                {
                    if (TimeOnly.TryParse(orderDto.OperationTime, out var operationTime))
                    {
                        updateFields.Add("operation_time = @OperationTime");
                        parameters.Add("OperationTime", operationTime);
                    }
                }

                if (!string.IsNullOrEmpty(orderDto.MaterialSendDate))
                {
                    if (DateOnly.TryParse(orderDto.MaterialSendDate, out var materialSendDate))
                    {
                        updateFields.Add("material_send_date = @MaterialSendDate");
                        parameters.Add("MaterialSendDate", materialSendDate);
                    }
                }

                if (orderDto.Remarks != null)
                {
                    updateFields.Add("remarks = @Remarks");
                    parameters.Add("Remarks", orderDto.Remarks);
                }

                if (!string.IsNullOrEmpty(orderDto.Status))
                {
                    updateFields.Add("status = @Status");
                    parameters.Add("Status", orderDto.Status);
                }

                updateFields.Add("updated_at = NOW()");

                if (updateFields.Count > 1) // More than just updated_at
                {
                    var updateSql = $"UPDATE Orders SET {string.Join(", ", updateFields)} WHERE order_id = @OrderId";
                    await _connection.ExecuteAsync(updateSql, parameters, transaction);
                }

                // Update item groups if provided
                if (orderDto.ItemGroups != null)
                {
                    // Delete existing item groups
                    await _connection.ExecuteAsync("DELETE FROM OrderItemGroups WHERE order_id = @OrderId", 
                        new { OrderId = id }, transaction);

                    // Insert new item groups
                    foreach (var itemGroupValue in orderDto.ItemGroups)
                    {
                        dynamic? itemGroup = null;
                        
                        // Try to parse as ID first, otherwise treat as name
                        if (int.TryParse(itemGroupValue, out var itemGroupId))
                        {
                            var itemGroupSql = "SELECT item_group_id, name FROM ItemGroups WHERE item_group_id = @Id AND is_active = 'Y'";
                            itemGroup = await _connection.QueryFirstOrDefaultAsync<dynamic>(itemGroupSql, 
                                new { Id = itemGroupId }, transaction);
                        }
                        else
                        {
                            var itemGroupSql = "SELECT item_group_id, name FROM ItemGroups WHERE name = @Name AND is_active = 'Y'";
                            itemGroup = await _connection.QueryFirstOrDefaultAsync<dynamic>(itemGroupSql, 
                                new { Name = itemGroupValue }, transaction);
                        }

                        if (itemGroup != null)
                        {
                            var insertItemGroupSql = @"
                                INSERT INTO OrderItemGroups (order_id, item_group_id, item_group_name, created_at)
                                VALUES (@OrderId, @ItemGroupId, @ItemGroupName, NOW())";
                            
                            await _connection.ExecuteAsync(insertItemGroupSql, new
                            {
                                OrderId = id,
                                ItemGroupId = (int)itemGroup.item_group_id,
                                ItemGroupName = (string)itemGroup.name
                            }, transaction);
                        }
                    }
                }

                // Update items if provided
                if (orderDto.Items != null)
                {
                    // Delete existing items
                    await _connection.ExecuteAsync("DELETE FROM OrderItems WHERE order_id = @OrderId", 
                        new { OrderId = id }, transaction);

                    // Insert new items
                    foreach (var item in orderDto.Items)
                    {
                        int? itemId = null;
                        int? itemGroupId = null;
                        var itemName = item.Name;

                        if (item.IsGroup == true)
                        {
                            var itemGroupSql = "SELECT item_group_id FROM ItemGroups WHERE name = @Name AND is_active = 'Y'";
                            itemGroupId = await _connection.QueryFirstOrDefaultAsync<int?>(itemGroupSql, 
                                new { Name = item.Name }, transaction);
                        }
                        else if (item.Manual != true)
                        {
                            var itemSql = "SELECT item_id FROM Items WHERE name = @Name AND is_active = 'Y'";
                            itemId = await _connection.QueryFirstOrDefaultAsync<int?>(itemSql, 
                                new { Name = item.Name }, transaction);
                        }

                        var insertItemSql = @"
                            INSERT INTO OrderItems (order_id, item_id, item_group_id, item_name, is_manual, is_group, quantity, created_at)
                            VALUES (@OrderId, @ItemId, @ItemGroupId, @ItemName, @IsManual, @IsGroup, @Quantity, NOW())";
                        
                        await _connection.ExecuteAsync(insertItemSql, new
                        {
                            OrderId = id,
                            ItemId = itemId,
                            ItemGroupId = itemGroupId,
                            ItemName = itemName,
                            IsManual = item.Manual == true ? "Y" : "N",
                            IsGroup = item.IsGroup == true ? "Y" : "N",
                            Quantity = item.Quantity
                        }, transaction);
                    }
                }

                // Insert audit record
                var auditAction = !string.IsNullOrEmpty(orderDto.Status) ? $"Updated - Status changed to {orderDto.Status}" : "Updated";
                var insertAuditSql = @"
                    INSERT INTO OrderAudits (order_id, action, performed_by, performed_at)
                    VALUES (@OrderId, @Action, @PerformedBy, NOW())";
                
                await _connection.ExecuteAsync(insertAuditSql, new
                {
                    OrderId = id,
                    Action = auditAction,
                    PerformedBy = orderDto.UpdatedBy ?? "System"
                }, transaction);

                await transaction.CommitAsync();

                // Retrieve the updated order
                var updatedOrder = await GetOrderByIdInternal(id);
                return Ok(new { message = "Order updated successfully", data = updatedOrder });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "23505")
        {
            return Conflict(new { message = "An order with this order number already exists" });
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "23503")
        {
            return BadRequest(new { message = "Invalid doctor or hospital ID" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // DELETE: api/Orders/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        try
        {
            if (_connection.State != System.Data.ConnectionState.Open)
                await _connection.OpenAsync();

            var checkSql = "SELECT COUNT(*) FROM Orders WHERE order_id = @OrderId AND is_active = 'Y'";
            var exists = await _connection.ExecuteScalarAsync<int>(checkSql, new { OrderId = id });
            
            if (exists == 0)
            {
                return NotFound(new { message = $"Order with ID {id} not found" });
            }

            // Soft delete
            var deleteSql = "UPDATE Orders SET is_active = 'N', updated_at = NOW() WHERE order_id = @OrderId";
            await _connection.ExecuteAsync(deleteSql, new { OrderId = id });

            return Ok(new { message = "Order deleted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // Helper method to map order data to DTO
    private async Task<OrderDto> MapToOrderDto(dynamic order)
    {
        if (_connection.State != System.Data.ConnectionState.Open)
            await _connection.OpenAsync();

        var orderId = (int)order.orderid;
        
        // Get item groups
        var itemGroupsSql = @"
            SELECT item_group_name
            FROM OrderItemGroups
            WHERE order_id = @OrderId
            ORDER BY order_item_group_id";
        
        var itemGroups = (await _connection.QueryAsync<string>(itemGroupsSql, new { OrderId = orderId })).ToList();

        // Get items
        var itemsSql = @"
            SELECT 
                COALESCE(item_id::text, item_group_id::text, order_item_id::text) as Id,
                item_name as Name,
                is_manual as IsManual,
                is_group as IsGroup,
                quantity as Quantity
            FROM OrderItems
            WHERE order_id = @OrderId
            ORDER BY order_item_id";
        
        var items = (await _connection.QueryAsync<dynamic>(itemsSql, new { OrderId = orderId }))
            .Select(i => new OrderItemDto
            {
                Id = (string)i.id,
                Name = (string)i.name,
                Manual = ((string)i.ismanual) == "Y" ? true : null,
                IsGroup = ((string)i.isgroup) == "Y" ? true : null,
                Quantity = (int)i.quantity
            })
            .ToList();

        // Get audits
        var auditsSql = @"
            SELECT 
                performed_at as PerformedAt,
                performed_by as PerformedBy,
                action as Action
            FROM OrderAudits
            WHERE order_id = @OrderId
            ORDER BY performed_at";
        
        var audits = (await _connection.QueryAsync<dynamic>(auditsSql, new { OrderId = orderId }))
            .Select(a => new OrderAuditDto
            {
                When = ((DateTime)a.performedat).ToString("yyyy-MM-dd HH:mm:ss"),
                By = (string)a.performedby,
                Action = (string)a.action
            })
            .ToList();

        return new OrderDto
        {
            OrderId = orderId,
            OrderNo = (string)order.orderno,
            OrderDate = order.orderdate is DateTime dt1 ? DateOnly.FromDateTime(dt1).ToString("yyyy-MM-dd") : ((DateOnly)order.orderdate).ToString("yyyy-MM-dd"),
            DoctorId = (int)order.doctorid,
            DoctorName = (string)order.doctorname,
            HospitalId = (int)order.hospitalid,
            HospitalName = (string)order.hospitalname,
            OperationDate = order.operationdate is DateTime dt2 ? DateOnly.FromDateTime(dt2).ToString("yyyy-MM-dd") : ((DateOnly)order.operationdate).ToString("yyyy-MM-dd"),
            OperationTime = order.operationtime is TimeSpan ts ? TimeOnly.FromTimeSpan(ts).ToString("HH:mm") : ((TimeOnly)order.operationtime).ToString("HH:mm"),
            MaterialSendDate = order.materialsenddate is DateTime dt3 ? DateOnly.FromDateTime(dt3).ToString("yyyy-MM-dd") : ((DateOnly)order.materialsenddate).ToString("yyyy-MM-dd"),
            ItemGroups = itemGroups,
            Items = items,
            Remarks = order.remarks != null ? (string)order.remarks : null,
            CreatedBy = (string)order.createdby,
            Status = (string)order.status,
            IsDelivered = (string)order.isdelivered,
            Audits = audits
        };
    }

    // Helper method to get order by ID (internal use)
    private async Task<OrderDto> GetOrderByIdInternal(int id)
    {
        if (_connection.State != System.Data.ConnectionState.Open)
            await _connection.OpenAsync();

        var sql = @"
            SELECT 
                o.order_id as OrderId,
                o.order_no as OrderNo,
                o.order_date as OrderDate,
                o.doctor_id as DoctorId,
                d.doctor_name as DoctorName,
                o.hospital_id as HospitalId,
                h.name as HospitalName,
                o.operation_date as OperationDate,
                o.operation_time as OperationTime,
                o.material_send_date as MaterialSendDate,
                o.remarks as Remarks,
                o.created_by as CreatedBy,
                o.status as Status,
                o.is_delivered as IsDelivered
            FROM Orders o
            INNER JOIN Doctors d ON o.doctor_id = d.doctor_id
            INNER JOIN Hospitals h ON o.hospital_id = h.hospital_id
            WHERE o.order_id = @OrderId";

        var order = await _connection.QueryFirstAsync<dynamic>(sql, new { OrderId = id });
        return await MapToOrderDto(order);
    }
}
