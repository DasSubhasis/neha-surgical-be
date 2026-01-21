using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using NehaSurgicalAPI.DTOs;
using NehaSurgicalAPI.Models;

namespace NehaSurgicalAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubAssistantAssignmentsController : ControllerBase
{
    private readonly NpgsqlConnection _connection;
    private readonly ILogger<SubAssistantAssignmentsController> _logger;

    public SubAssistantAssignmentsController(NpgsqlConnection connection, ILogger<SubAssistantAssignmentsController> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    // GET: api/SubAssistantAssignments
    [HttpGet]
    public async Task<IActionResult> GetAllSubAssignments([FromQuery] int? assignmentId = null, [FromQuery] int? orderId = null)
    {
        try
        {
            if (_connection.State != System.Data.ConnectionState.Open)
                await _connection.OpenAsync();

            var sql = @"SELECT sa.sub_assignment_id as SubAssignmentId, sa.assignment_id as AssignmentId,
                        aa.order_id as OrderId, o.order_no as OrderNo,
                        aa.assistant_id as MainAssistantId, main_su.full_name as MainAssistantName,
                        sa.sub_assistant_id as SubAssistantId, sub_su.full_name as SubAssistantName,
                        sub_su.phone_no as SubAssistantPhone, sa.remarks as Remarks,
                        sa.assigned_at as AssignedAt
                        FROM SubAssistantAssignments sa
                        INNER JOIN AssistantAssignments aa ON sa.assignment_id = aa.assignment_id
                        INNER JOIN Orders o ON aa.order_id = o.order_id
                        INNER JOIN SystemUsers main_su ON aa.assistant_id = main_su.system_user_id
                        INNER JOIN SystemUsers sub_su ON sa.sub_assistant_id = sub_su.system_user_id
                        WHERE sa.is_active = 'Y' AND aa.is_active = 'Y' AND o.is_active = 'Y'";

            if (assignmentId.HasValue)
            {
                sql += " AND sa.assignment_id = @AssignmentId";
            }

            if (orderId.HasValue)
            {
                sql += " AND aa.order_id = @OrderId";
            }

            sql += " ORDER BY sa.assigned_at DESC";

            var subAssignments = await _connection.QueryAsync<dynamic>(sql, new { AssignmentId = assignmentId, OrderId = orderId });

            var subAssignmentDtos = subAssignments.Select(sa => new SubAssistantAssignmentDto
            {
                SubAssignmentId = (int)sa.subassignmentid,
                AssignmentId = (int)sa.assignmentid,
                OrderId = (int)sa.orderid,
                OrderNo = (string)sa.orderno,
                MainAssistantId = (int)sa.mainassistantid,
                MainAssistantName = (string)sa.mainassistantname,
                SubAssistantId = (int)sa.subassistantid,
                SubAssistantName = (string)sa.subassistantname,
                SubAssistantPhone = sa.subassistantphone != null ? (string)sa.subassistantphone : null,
                Remarks = sa.remarks != null ? (string)sa.remarks : null,
                AssignedAt = ((DateTime)sa.assignedat).ToString("yyyy-MM-dd HH:mm:ss")
            }).ToList();

            return Ok(new { message = "Sub-assistant assignments retrieved successfully", data = subAssignmentDtos });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sub-assistant assignments");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // GET: api/SubAssistantAssignments/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetSubAssignmentById(int id)
    {
        try
        {
            if (_connection.State != System.Data.ConnectionState.Open)
                await _connection.OpenAsync();

            var sql = @"SELECT sa.sub_assignment_id as SubAssignmentId, sa.assignment_id as AssignmentId,
                        aa.order_id as OrderId, o.order_no as OrderNo,
                        aa.assistant_id as MainAssistantId, main_su.full_name as MainAssistantName,
                        sa.sub_assistant_id as SubAssistantId, sub_su.full_name as SubAssistantName,
                        sub_su.phone_no as SubAssistantPhone, sa.remarks as Remarks,
                        sa.assigned_at as AssignedAt
                        FROM SubAssistantAssignments sa
                        INNER JOIN AssistantAssignments aa ON sa.assignment_id = aa.assignment_id
                        INNER JOIN Orders o ON aa.order_id = o.order_id
                        INNER JOIN SystemUsers main_su ON aa.assistant_id = main_su.system_user_id
                        INNER JOIN SystemUsers sub_su ON sa.sub_assistant_id = sub_su.system_user_id
                        WHERE sa.sub_assignment_id = @Id AND sa.is_active = 'Y'";

            var subAssignment = await _connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { Id = id });

            if (subAssignment == null)
            {
                return NotFound(new { message = $"Sub-assistant assignment with ID {id} not found" });
            }

            var subAssignmentDto = new SubAssistantAssignmentDto
            {
                SubAssignmentId = (int)subAssignment.subassignmentid,
                AssignmentId = (int)subAssignment.assignmentid,
                OrderId = (int)subAssignment.orderid,
                OrderNo = (string)subAssignment.orderno,
                MainAssistantId = (int)subAssignment.mainassistantid,
                MainAssistantName = (string)subAssignment.mainassistantname,
                SubAssistantId = (int)subAssignment.subassistantid,
                SubAssistantName = (string)subAssignment.subassistantname,
                SubAssistantPhone = subAssignment.subassistantphone != null ? (string)subAssignment.subassistantphone : null,
                Remarks = subAssignment.remarks != null ? (string)subAssignment.remarks : null,
                AssignedAt = ((DateTime)subAssignment.assignedat).ToString("yyyy-MM-dd HH:mm:ss")
            };

            return Ok(new { message = "Sub-assistant assignment retrieved successfully", data = subAssignmentDto });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sub-assistant assignment with ID {SubAssignmentId}", id);
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // POST: api/SubAssistantAssignments
    [HttpPost]
    public async Task<IActionResult> AssignSubAssistant([FromBody] AssignSubAssistantDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_connection.State != System.Data.ConnectionState.Open)
                await _connection.OpenAsync();

            // Validate that either AssignmentId or OrderId is provided
            if (!dto.AssignmentId.HasValue && !dto.OrderId.HasValue)
            {
                return BadRequest(new { message = "Either AssignmentId or OrderId must be provided" });
            }

            // Get assignment - either by AssignmentId or OrderId
            dynamic? assignment = null;
            int assignmentId = 0;

            if (dto.AssignmentId.HasValue && dto.AssignmentId.Value > 0)
            {
                var assignmentSql = @"SELECT aa.assignment_id, aa.order_id, aa.assistant_id, 
                                     o.order_no, su.full_name as main_assistant_name
                                     FROM AssistantAssignments aa
                                     INNER JOIN Orders o ON aa.order_id = o.order_id
                                     INNER JOIN SystemUsers su ON aa.assistant_id = su.system_user_id
                                     WHERE aa.assignment_id = @AssignmentId AND aa.is_active = 'Y'";
                assignment = await _connection.QueryFirstOrDefaultAsync<dynamic>(assignmentSql, new { dto.AssignmentId });
                
                if (assignment == null)
                {
                    return NotFound(new { message = $"Main assistant assignment with ID {dto.AssignmentId} not found" });
                }
                
                assignmentId = assignment.assignment_id;
            }
            else if (dto.OrderId.HasValue && dto.OrderId.Value > 0)
            {
                var assignmentSql = @"SELECT aa.assignment_id, aa.order_id, aa.assistant_id, 
                                     o.order_no, su.full_name as main_assistant_name
                                     FROM AssistantAssignments aa
                                     INNER JOIN Orders o ON aa.order_id = o.order_id
                                     INNER JOIN SystemUsers su ON aa.assistant_id = su.system_user_id
                                     WHERE aa.order_id = @OrderId AND aa.is_active = 'Y'";
                assignment = await _connection.QueryFirstOrDefaultAsync<dynamic>(assignmentSql, new { dto.OrderId });
                
                if (assignment == null)
                {
                    return NotFound(new { message = $"No main assistant assigned to order ID {dto.OrderId}. Please assign a main assistant first." });
                }
                
                assignmentId = assignment.assignment_id;
            }
            else
            {
                return BadRequest(new { message = "Invalid AssignmentId or OrderId" });
            }

            // Validate sub-assistant exists and has 'Assistant' role
            var subAssistantSql = @"SELECT su.system_user_id, su.full_name, su.phone_no, r.role_name
                                   FROM SystemUsers su
                                   INNER JOIN Roles r ON su.role_id = r.role_id
                                   WHERE su.system_user_id = @SubAssistantId AND su.is_active = 'Y'";
            var subAssistant = await _connection.QueryFirstOrDefaultAsync<dynamic>(subAssistantSql, new { dto.SubAssistantId });

            if (subAssistant == null)
            {
                return NotFound(new { message = $"Assistant with ID {dto.SubAssistantId} not found" });
            }

            // if (subAssistant.role_name != "Assistant")
            // {
            //     return BadRequest(new { message = "Selected user is not an assistant" });
            // }

            // Check if sub-assistant is the same as main assistant
            if (assignment.assistant_id == dto.SubAssistantId)
            {
                return BadRequest(new { message = "Assistant cannot be the same as main assistant" });
            }

            // Check if sub-assistant already assigned to this assignment
            var existingSql = @"SELECT sub_assignment_id FROM SubAssistantAssignments 
                               WHERE assignment_id = @AssignmentId AND sub_assistant_id = @SubAssistantId AND is_active = 'Y'";
            var existing = await _connection.QueryFirstOrDefaultAsync<int?>(existingSql, 
                new { AssignmentId = assignmentId, dto.SubAssistantId });

            if (existing.HasValue)
            {
                return Conflict(new { message = "This assistant is already assigned to this order" });
            }

            // Validate assigned_by user if provided
            int? validatedAssignedBy = null;
            if (dto.AssignedBy.HasValue && dto.AssignedBy.Value > 0)
            {
                var assignedByExists = await _connection.ExecuteScalarAsync<int?>(
                    "SELECT system_user_id FROM SystemUsers WHERE system_user_id = @UserId AND is_active = 'Y'",
                    new { UserId = dto.AssignedBy.Value });
                
                if (assignedByExists.HasValue)
                {
                    validatedAssignedBy = assignedByExists.Value;
                }
            }

            // Insert sub-assistant assignment
            var insertSql = @"INSERT INTO SubAssistantAssignments 
                             (assignment_id, sub_assistant_id, remarks, assigned_by, assigned_at, is_active, created_at, updated_at)
                             VALUES (@AssignmentId, @SubAssistantId, @Remarks, @AssignedBy, NOW(), 'Y', NOW(), NOW())
                             RETURNING sub_assignment_id";

            var subAssignmentId = await _connection.ExecuteScalarAsync<int>(insertSql, new
            {
                AssignmentId = assignmentId,
                dto.SubAssistantId,
                dto.Remarks,
                AssignedBy = validatedAssignedBy
            });

            _logger.LogInformation("Sub-assistant {SubAssistantName} ({Phone}) assigned under main assistant {MainAssistantName} for order {OrderNo}",
                (string)subAssistant.full_name, (string)subAssistant.phone_no, (string)assignment.main_assistant_name, (string)assignment.order_no);

            return CreatedAtAction(nameof(GetSubAssignmentById), new { id = subAssignmentId }, 
                new { message = "Assistant assigned successfully", subAssignmentId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning assistant");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // DELETE: api/SubAssistantAssignments/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> UnassignSubAssistant(int id)
    {
        try
        {
            if (_connection.State != System.Data.ConnectionState.Open)
                await _connection.OpenAsync();

            // Check if sub-assignment exists
            var exists = await _connection.ExecuteScalarAsync<bool>(
                "SELECT EXISTS(SELECT 1 FROM SubAssistantAssignments WHERE sub_assignment_id = @Id AND is_active = 'Y')",
                new { Id = id });

            if (!exists)
            {
                return NotFound(new { message = $"Sub-assistant assignment with ID {id} not found" });
            }

            // Soft delete
            var sql = "UPDATE SubAssistantAssignments SET is_active = 'N', updated_at = NOW() WHERE sub_assignment_id = @Id";
            await _connection.ExecuteAsync(sql, new { Id = id });

            return Ok(new { message = "Sub-assistant unassigned successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unassigning sub-assistant with ID {SubAssignmentId}", id);
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
