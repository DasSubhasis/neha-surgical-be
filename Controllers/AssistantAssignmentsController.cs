using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Dapper;
using NehaSurgicalAPI.Models;
using NehaSurgicalAPI.DTOs;
using NehaSurgicalAPI.Data;

namespace NehaSurgicalAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssistantAssignmentsController : ControllerBase
{
    private readonly NpgsqlConnection _connection;
    private readonly ILogger<AssistantAssignmentsController> _logger;

    public AssistantAssignmentsController(NpgsqlConnection connection, ILogger<AssistantAssignmentsController> logger)
    {
        _connection = connection;
        _logger = logger;
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
        SqlMapper.AddTypeHandler(new TimeOnlyTypeHandler());
    }

    // GET: api/AssistantAssignments
    [HttpGet]
    public async Task<IActionResult> GetAllAssignments([FromQuery] string? status = null)
    {
        try
        {
            if (_connection.State != System.Data.ConnectionState.Open)
                await _connection.OpenAsync();

            var sql = @"
                SELECT 
                    o.order_id as Id,
                    o.order_no as OrderNo,
                    d.doctor_name as Patient,
                    o.operation_date as OperationDate,
                    o.operation_time as OperationTime,
                    o.doctor_id as DoctorId,
                    d.doctor_name as DoctorName,
                    o.hospital_id as HospitalId,
                    h.name as HospitalName,
                    aa.assistant_id as AssistantId,
                    su.full_name as AssistantName,
                    aa.reporting_date as ReportingDate,
                    aa.reporting_time as ReportingTime,
                    aa.remarks as Remarks,
                    CASE WHEN aa.assignment_id IS NOT NULL THEN 'Assigned' ELSE 'Pending' END as Status
                FROM Orders o
                INNER JOIN Doctors d ON o.doctor_id = d.doctor_id
                INNER JOIN Hospitals h ON o.hospital_id = h.hospital_id
                LEFT JOIN AssistantAssignments aa ON o.order_id = aa.order_id AND aa.is_active = 'Y'
                LEFT JOIN SystemUsers su ON aa.assistant_id = su.system_user_id
                WHERE o.is_active = 'Y'";

            if (!string.IsNullOrEmpty(status))
            {
                if (status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
                {
                    sql += " AND aa.assignment_id IS NULL";
                }
                else if (status.Equals("Assigned", StringComparison.OrdinalIgnoreCase))
                {
                    sql += " AND aa.assignment_id IS NOT NULL";
                }
            }

            sql += " ORDER BY o.operation_date ASC, o.operation_time ASC";

            var assignments = await _connection.QueryAsync<dynamic>(sql);

            var assignmentDtos = assignments.Select(a => new AssistantAssignmentDto
            {
                Id = a.id,
                OrderNo = a.orderno,
                Patient = a.patient,
                OperationDate = a.operationdate is DateTime dt1 ? DateOnly.FromDateTime(dt1).ToString("yyyy-MM-dd") : ((DateOnly)a.operationdate).ToString("yyyy-MM-dd"),
                OperationTime = a.operationtime is TimeSpan ts ? TimeOnly.FromTimeSpan(ts).ToString("HH:mm") : ((TimeOnly)a.operationtime).ToString("HH:mm"),
                DoctorId = a.doctorid,
                DoctorName = a.doctorname,
                HospitalId = a.hospitalid,
                HospitalName = a.hospitalname,
                AssistantId = a.assistantid,
                AssistantName = a.assistantname,
                ReportingDate = a.reportingdate != null ? (a.reportingdate is DateTime dt2 ? DateOnly.FromDateTime(dt2).ToString("yyyy-MM-dd") : ((DateOnly)a.reportingdate).ToString("yyyy-MM-dd")) : null,
                ReportingTime = a.reportingtime != null ? (a.reportingtime is TimeSpan ts2 ? TimeOnly.FromTimeSpan(ts2).ToString("HH:mm") : ((TimeOnly)a.reportingtime).ToString("HH:mm")) : null,
                Remarks = a.remarks,
                Status = a.status
            }).ToList();

            return Ok(new { message = "Assignments retrieved successfully", data = assignmentDtos });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assignments");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // GET: api/AssistantAssignments/assistants
    [HttpGet("assistants")]
    public async Task<IActionResult> GetAssistants()
    {
        try
        {
            if (_connection.State != System.Data.ConnectionState.Open)
                await _connection.OpenAsync();

            // Get users with 'Assistant' role
            var sql = @"
                SELECT 
                    su.system_user_id as Id,
                    su.full_name as Name,
                    su.phone_no as Phone,
                    su.email as Email
                FROM SystemUsers su
                INNER JOIN Roles r ON su.role_id = r.role_id
                WHERE r.role_name = 'Assistant' 
                AND su.is_active = 'Y'
                ORDER BY su.full_name";

            var assistants = await _connection.QueryAsync<AssistantDto>(sql);

            return Ok(new { message = "Assistants retrieved successfully", data = assistants });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assistants");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // GET: api/AssistantAssignments/existing/{assistantId}
    [HttpGet("existing/{assistantId}")]
    public async Task<IActionResult> GetExistingAssignments(int assistantId)
    {
        try
        {
            if (_connection.State != System.Data.ConnectionState.Open)
                await _connection.OpenAsync();

            var sql = @"
                SELECT 
                    aa.assignment_id as Id,
                    aa.assistant_id as AssistantId,
                    o.order_no as OrderNo,
                    o.operation_date as Date,
                    o.operation_time as OperationTime,
                    aa.reporting_date as ReportingDate,
                    aa.reporting_time as ReportingTime
                FROM AssistantAssignments aa
                INNER JOIN Orders o ON aa.order_id = o.order_id
                WHERE aa.assistant_id = @AssistantId 
                AND aa.is_active = 'Y'
                AND o.is_active = 'Y'
                ORDER BY o.operation_date DESC, o.operation_time DESC";

            var assignments = await _connection.QueryAsync<dynamic>(sql, new { AssistantId = assistantId });

            var existingAssignments = assignments.Select(a => new ExistingAssignmentDto
            {
                Id = a.id,
                AssistantId = a.assistantid,
                OrderNo = a.orderno,
                Date = a.date is DateTime dt ? DateOnly.FromDateTime(dt).ToString("yyyy-MM-dd") : ((DateOnly)a.date).ToString("yyyy-MM-dd"),
                OperationTime = a.operationtime is TimeSpan ts1 ? TimeOnly.FromTimeSpan(ts1).ToString("HH:mm") : ((TimeOnly)a.operationtime).ToString("HH:mm"),
                ReportingDate = a.reportingdate is DateTime dt2 ? DateOnly.FromDateTime(dt2).ToString("yyyy-MM-dd") : ((DateOnly)a.reportingdate).ToString("yyyy-MM-dd"),
                ReportingTime = a.reportingtime is TimeSpan ts2 ? TimeOnly.FromTimeSpan(ts2).ToString("HH:mm") : ((TimeOnly)a.reportingtime).ToString("HH:mm")
            }).ToList();

            return Ok(new { message = "Existing assignments retrieved successfully", data = existingAssignments });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving existing assignments");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // GET: api/AssistantAssignments/existing-by-date?assistantId=1&date=2025-12-10
    [HttpGet("existing-by-date")]
    public async Task<IActionResult> GetExistingAssignmentsByDate([FromQuery] int? assistantId = null, [FromQuery] string? date = null)
    {
        try
        {
            if (_connection.State != System.Data.ConnectionState.Open)
                await _connection.OpenAsync();

            var sql = @"
                SELECT 
                    aa.assignment_id as Id,
                    o.order_id as OrderId,
                    o.order_no as OrderNo,
                    aa.assistant_id as AssistantId,
                    su.full_name as AssistantName,
                    o.operation_date as OperationDate,
                    o.operation_time as OperationTime,
                    aa.reporting_date as ReportingDate,
                    aa.reporting_time as ReportingTime,
                    d.doctor_name as DoctorName,
                    h.name as HospitalName,
                    aa.remarks as Remarks
                FROM AssistantAssignments aa
                INNER JOIN Orders o ON aa.order_id = o.order_id
                INNER JOIN SystemUsers su ON aa.assistant_id = su.system_user_id
                INNER JOIN Doctors d ON o.doctor_id = d.doctor_id
                INNER JOIN Hospitals h ON o.hospital_id = h.hospital_id
                WHERE aa.is_active = 'Y'
                AND o.is_active = 'Y'";

            var parameters = new DynamicParameters();

            if (assistantId.HasValue)
            {
                sql += " AND aa.assistant_id = @AssistantId";
                parameters.Add("AssistantId", assistantId.Value);
            }

            if (!string.IsNullOrEmpty(date))
            {
                if (DateOnly.TryParse(date, out var reportingDate))
                {
                    sql += " AND aa.reporting_date = @ReportingDate";
                    parameters.Add("ReportingDate", reportingDate);
                }
                else
                {
                    return BadRequest(new { message = "Invalid date format. Use yyyy-MM-dd" });
                }
            }

            sql += " ORDER BY o.operation_date DESC, o.operation_time DESC, aa.reporting_time ASC";

            var assignments = await _connection.QueryAsync<dynamic>(sql, parameters);

            var assignmentList = assignments.Select(a => new
            {
                Id = (int)a.id,
                OrderId = (int)a.orderid,
                OrderNo = (string)a.orderno,
                AssistantId = (int)a.assistantid,
                AssistantName = (string)a.assistantname,
                OperationDate = a.operationdate is DateTime dt1 ? DateOnly.FromDateTime(dt1).ToString("yyyy-MM-dd") : ((DateOnly)a.operationdate).ToString("yyyy-MM-dd"),
                OperationTime = a.operationtime is TimeSpan ts1 ? TimeOnly.FromTimeSpan(ts1).ToString("HH:mm") : ((TimeOnly)a.operationtime).ToString("HH:mm"),
                ReportingDate = a.reportingdate is DateTime dt2 ? DateOnly.FromDateTime(dt2).ToString("yyyy-MM-dd") : ((DateOnly)a.reportingdate).ToString("yyyy-MM-dd"),
                ReportingTime = a.reportingtime is TimeSpan ts2 ? TimeOnly.FromTimeSpan(ts2).ToString("HH:mm") : ((TimeOnly)a.reportingtime).ToString("HH:mm"),
                DoctorName = (string)a.doctorname,
                HospitalName = (string)a.hospitalname,
                Remarks = a.remarks != null ? (string)a.remarks : null
            }).ToList();

            return Ok(new { 
                message = "Existing assignments retrieved successfully", 
                count = assignmentList.Count,
                data = assignmentList 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving existing assignments by date");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // POST: api/AssistantAssignments/assign
    [HttpPost("assign")]
    public async Task<IActionResult> AssignAssistant([FromBody] AssignAssistantDto assignDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_connection.State != System.Data.ConnectionState.Open)
                await _connection.OpenAsync();

            // Parse reporting date and time
            if (!DateOnly.TryParse(assignDto.ReportingDate, out var reportingDate))
            {
                return BadRequest(new { message = "Invalid reporting date format" });
            }

            if (!TimeOnly.TryParse(assignDto.ReportingTime, out var reportingTime))
            {
                return BadRequest(new { message = "Invalid reporting time format" });
            }

            using var transaction = await _connection.BeginTransactionAsync();
            try
            {
                // Check if order exists
                var orderSql = @"SELECT order_id, order_no, operation_date, operation_time, doctor_id, hospital_id 
                                FROM Orders WHERE order_id = @OrderId AND is_active = 'Y'";
                var order = await _connection.QueryFirstOrDefaultAsync<dynamic>(orderSql, 
                    new { OrderId = assignDto.OrderId }, transaction);

                if (order == null)
                {
                    return NotFound(new { message = $"Order with ID {assignDto.OrderId} not found" });
                }

                // Check if assistant exists and has 'Assistant' role
                var assistantSql = @"SELECT su.system_user_id, su.full_name, su.phone_no, su.email, r.role_name
                                    FROM SystemUsers su
                                    INNER JOIN Roles r ON su.role_id = r.role_id
                                    WHERE su.system_user_id = @AssistantId AND su.is_active = 'Y'";
                var assistant = await _connection.QueryFirstOrDefaultAsync<dynamic>(assistantSql, 
                    new { AssistantId = assignDto.AssistantId }, transaction);

                if (assistant == null)
                {
                    return NotFound(new { message = $"Assistant with ID {assignDto.AssistantId} not found" });
                }

                if (assistant.role_name != "Assistant")
                {
                    return BadRequest(new { message = "Selected user is not an assistant" });
                }

                // Validate assigned_by user if provided
                int? validatedAssignedBy = null;
                if (assignDto.AssignedBy.HasValue && assignDto.AssignedBy.Value > 0)
                {
                    var assignedByExists = await _connection.ExecuteScalarAsync<int?>(
                        "SELECT system_user_id FROM SystemUsers WHERE system_user_id = @UserId AND is_active = 'Y'",
                        new { UserId = assignDto.AssignedBy.Value }, transaction);
                    
                    if (assignedByExists.HasValue)
                    {
                        validatedAssignedBy = assignedByExists.Value;
                    }
                }

                // Check if assignment already exists
                var existingSql = "SELECT assignment_id FROM AssistantAssignments WHERE order_id = @OrderId AND is_active = 'Y'";
                var existingAssignment = await _connection.QueryFirstOrDefaultAsync<int?>(existingSql, 
                    new { OrderId = assignDto.OrderId }, transaction);

                int assignmentId;
                if (existingAssignment.HasValue)
                {
                    // Update existing assignment
                    var updateSql = @"UPDATE AssistantAssignments 
                                     SET assistant_id = @AssistantId,
                                         reporting_date = @ReportingDate,
                                         reporting_time = @ReportingTime,
                                         remarks = @Remarks,
                                         assigned_by = @AssignedBy,
                                         assigned_at = NOW(),
                                         updated_at = NOW()
                                     WHERE assignment_id = @AssignmentId
                                     RETURNING assignment_id";

                    assignmentId = await _connection.ExecuteScalarAsync<int>(updateSql, new
                    {
                        AssignmentId = existingAssignment.Value,
                        assignDto.AssistantId,
                        ReportingDate = reportingDate,
                        ReportingTime = reportingTime,
                        assignDto.Remarks,
                        AssignedBy = validatedAssignedBy
                    }, transaction);
                }
                else
                {
                    // Create new assignment
                    var insertSql = @"INSERT INTO AssistantAssignments 
                                     (order_id, assistant_id, reporting_date, reporting_time, remarks, assigned_by, assigned_at, is_active, created_at, updated_at)
                                     VALUES (@OrderId, @AssistantId, @ReportingDate, @ReportingTime, @Remarks, @AssignedBy, NOW(), 'Y', NOW(), NOW())
                                     RETURNING assignment_id";

                    assignmentId = await _connection.ExecuteScalarAsync<int>(insertSql, new
                    {
                        assignDto.OrderId,
                        assignDto.AssistantId,
                        ReportingDate = reportingDate,
                        ReportingTime = reportingTime,
                        assignDto.Remarks,
                        AssignedBy = validatedAssignedBy
                    }, transaction);
                }

                // Update order status to Assigned
                var updateOrderStatusSql = @"UPDATE Orders 
                                             SET status = 'Assigned', updated_at = NOW() 
                                             WHERE order_id = @OrderId";
                await _connection.ExecuteAsync(updateOrderStatusSql, new { OrderId = assignDto.OrderId }, transaction);

                await transaction.CommitAsync();

                // Log WhatsApp notifications (to be implemented with actual WhatsApp service)
                var operationDate = order.operation_date is DateTime dt1 ? DateOnly.FromDateTime(dt1).ToString("yyyy-MM-dd") : ((DateOnly)order.operation_date).ToString("yyyy-MM-dd");
                var operationTime = order.operation_time is TimeSpan ts ? TimeOnly.FromTimeSpan(ts).ToString("HH:mm") : ((TimeOnly)order.operation_time).ToString("HH:mm");
                
                _logger.LogInformation("WA -> {AssistantName} ({Phone}): Assigned to {OrderNo} on {Date} at {Time} — reporting {ReportingTime}",
                    (string)assistant.full_name, (string)assistant.phone_no, (string)order.order_no, operationDate, operationTime, assignDto.ReportingTime);

                // Return the assignment details
                var resultDto = new AssistantAssignmentDto
                {
                    Id = assignDto.OrderId,
                    OrderNo = order.order_no,
                    OperationDate = operationDate,
                    OperationTime = operationTime,
                    AssistantId = assignDto.AssistantId,
                    AssistantName = assistant.full_name,
                    ReportingTime = assignDto.ReportingTime,
                    Remarks = assignDto.Remarks,
                    Status = "Assigned"
                };

                return Ok(new { 
                    success = true,
                    message = "Assistant assigned successfully", 
                    data = resultDto 
                });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "23505")
        {
            return Conflict(new { message = "Assignment already exists for this order" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning assistant");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // DELETE: api/AssistantAssignments/{orderId}
    [HttpDelete("{orderId}")]
    public async Task<IActionResult> UnassignAssistant(int orderId)
    {
        try
        {
            if (_connection.State != System.Data.ConnectionState.Open)
                await _connection.OpenAsync();

            var sql = "UPDATE AssistantAssignments SET is_active = 'N', updated_at = NOW() WHERE order_id = @OrderId AND is_active = 'Y'";
            var rowsAffected = await _connection.ExecuteAsync(sql, new { OrderId = orderId });

            if (rowsAffected == 0)
            {
                return NotFound(new { message = $"Assignment for order ID {orderId} not found" });
            }

            return Ok(new { message = "Assistant unassigned successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unassigning assistant");
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
