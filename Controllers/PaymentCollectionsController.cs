using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using NehaSurgicalAPI.Data;
using NehaSurgicalAPI.DTOs;
using NehaSurgicalAPI.Models;

namespace NehaSurgicalAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentCollectionsController : ControllerBase
{
    private readonly NpgsqlConnection _connection;
    private readonly ILogger<PaymentCollectionsController> _logger;

    public PaymentCollectionsController(NpgsqlConnection connection, ILogger<PaymentCollectionsController> logger)
    {
        _connection = connection;
        _logger = logger;
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
    }

    // GET: api/PaymentCollections
    [HttpGet]
    public async Task<IActionResult> GetAllPaymentCollections(
        [FromQuery] int? doctorId = null,
        [FromQuery] int? hospitalId = null,
        [FromQuery] string? fromDate = null,
        [FromQuery] string? toDate = null)
    {
        try
        {
            if (_connection.State != System.Data.ConnectionState.Open)
                await _connection.OpenAsync();

            var sql = @"SELECT 
                        pc.collection_id as CollectionId,
                        pc.collection_date as CollectionDate,
                        pc.collected_by as CollectedBy,
                        pc.doctor_id as DoctorId,
                        d.doctor_name as DoctorName,
                        pc.hospital_id as HospitalId,
                        h.name as HospitalName,
                        pc.amount as Amount,
                        pc.remarks as Remarks,
                        pc.created_by as CreatedBy,
                        pc.created_at as CreatedAt
                        FROM PaymentCollections pc
                        INNER JOIN Doctors d ON pc.doctor_id = d.doctor_id
                        INNER JOIN Hospitals h ON pc.hospital_id = h.hospital_id
                        WHERE pc.is_active = 'Y'";

            var parameters = new DynamicParameters();

            if (doctorId.HasValue)
            {
                sql += " AND pc.doctor_id = @DoctorId";
                parameters.Add("DoctorId", doctorId.Value);
            }

            if (hospitalId.HasValue)
            {
                sql += " AND pc.hospital_id = @HospitalId";
                parameters.Add("HospitalId", hospitalId.Value);
            }

            if (!string.IsNullOrEmpty(fromDate))
            {
                sql += " AND pc.collection_date >= @FromDate";
                parameters.Add("FromDate", DateOnly.Parse(fromDate));
            }

            if (!string.IsNullOrEmpty(toDate))
            {
                sql += " AND pc.collection_date <= @ToDate";
                parameters.Add("ToDate", DateOnly.Parse(toDate));
            }

            sql += " ORDER BY pc.collection_date DESC, pc.collection_id DESC";

            var collections = await _connection.QueryAsync<dynamic>(sql, parameters);

            var collectionDtos = collections.Select(c => new PaymentCollectionDto
            {
                CollectionId = (int)c.collectionid,
                CollectionDate = DateOnly.FromDateTime((DateTime)c.collectiondate).ToString("yyyy-MM-dd"),
                CollectedBy = (string)c.collectedby,
                DoctorId = (int)c.doctorid,
                DoctorName = (string)c.doctorname,
                HospitalId = (int)c.hospitalid,
                HospitalName = (string)c.hospitalname,
                Amount = (decimal)c.amount,
                Remarks = c.remarks != null ? (string)c.remarks : null,
                CreatedBy = c.createdby != null ? (string)c.createdby : null,
                CreatedAt = ((DateTime)c.createdat).ToString("yyyy-MM-dd HH:mm:ss")
            }).ToList();

            return Ok(new { message = "Payment collections retrieved successfully", count = collectionDtos.Count, data = collectionDtos });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment collections");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // GET: api/PaymentCollections/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPaymentCollectionById(int id)
    {
        try
        {
            if (_connection.State != System.Data.ConnectionState.Open)
                await _connection.OpenAsync();

            var sql = @"SELECT 
                        pc.collection_id as CollectionId,
                        pc.collection_date as CollectionDate,
                        pc.collected_by as CollectedBy,
                        pc.doctor_id as DoctorId,
                        d.doctor_name as DoctorName,
                        pc.hospital_id as HospitalId,
                        h.name as HospitalName,
                        pc.amount as Amount,
                        pc.remarks as Remarks,
                        pc.created_by as CreatedBy,
                        pc.created_at as CreatedAt
                        FROM PaymentCollections pc
                        INNER JOIN Doctors d ON pc.doctor_id = d.doctor_id
                        INNER JOIN Hospitals h ON pc.hospital_id = h.hospital_id
                        WHERE pc.collection_id = @Id AND pc.is_active = 'Y'";

            var collection = await _connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { Id = id });

            if (collection == null)
            {
                return NotFound(new { message = $"Payment collection with ID {id} not found" });
            }

            var collectionDto = new PaymentCollectionDto
            {
                CollectionId = (int)collection.collectionid,
                CollectionDate = DateOnly.FromDateTime((DateTime)collection.collectiondate).ToString("yyyy-MM-dd"),
                CollectedBy = (string)collection.collectedby,
                DoctorId = (int)collection.doctorid,
                DoctorName = (string)collection.doctorname,
                HospitalId = (int)collection.hospitalid,
                HospitalName = (string)collection.hospitalname,
                Amount = (decimal)collection.amount,
                Remarks = collection.remarks != null ? (string)collection.remarks : null,
                CreatedBy = collection.createdby != null ? (string)collection.createdby : null,
                CreatedAt = ((DateTime)collection.createdat).ToString("yyyy-MM-dd HH:mm:ss")
            };

            return Ok(new { message = "Payment collection retrieved successfully", data = collectionDto });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment collection with ID {CollectionId}", id);
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // POST: api/PaymentCollections
    [HttpPost]
    public async Task<IActionResult> CreatePaymentCollection([FromBody] CreatePaymentCollectionDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_connection.State != System.Data.ConnectionState.Open)
                await _connection.OpenAsync();

            // Validate doctor exists
            var doctorExists = await _connection.ExecuteScalarAsync<bool>(
                "SELECT EXISTS(SELECT 1 FROM Doctors WHERE doctor_id = @DoctorId AND is_active = 'Y')",
                new { dto.DoctorId });

            if (!doctorExists)
            {
                return NotFound(new { message = $"Doctor with ID {dto.DoctorId} not found" });
            }

            // Validate hospital exists
            var hospitalExists = await _connection.ExecuteScalarAsync<bool>(
                "SELECT EXISTS(SELECT 1 FROM Hospitals WHERE hospital_id = @HospitalId AND is_active = 'Y')",
                new { dto.HospitalId });

            if (!hospitalExists)
            {
                return NotFound(new { message = $"Hospital with ID {dto.HospitalId} not found" });
            }

            var sql = @"INSERT INTO PaymentCollections 
                        (collection_date, collected_by, doctor_id, hospital_id, amount, remarks, created_by, created_at, updated_at, is_active)
                        VALUES (@CollectionDate, @CollectedBy, @DoctorId, @HospitalId, @Amount, @Remarks, @CreatedBy, NOW(), NOW(), 'Y')
                        RETURNING collection_id";

            var collectionId = await _connection.ExecuteScalarAsync<int>(sql, new
            {
                CollectionDate = DateOnly.Parse(dto.CollectionDate),
                dto.CollectedBy,
                dto.DoctorId,
                dto.HospitalId,
                dto.Amount,
                dto.Remarks,
                dto.CreatedBy
            });

            return CreatedAtAction(nameof(GetPaymentCollectionById), new { id = collectionId },
                new { message = "Payment collection created successfully", collectionId });
        }
        catch (FormatException)
        {
            return BadRequest(new { message = "Invalid date format. Use yyyy-MM-dd" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment collection");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // PUT: api/PaymentCollections/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePaymentCollection(int id, [FromBody] UpdatePaymentCollectionDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_connection.State != System.Data.ConnectionState.Open)
                await _connection.OpenAsync();

            // Check if collection exists
            var exists = await _connection.ExecuteScalarAsync<bool>(
                "SELECT EXISTS(SELECT 1 FROM PaymentCollections WHERE collection_id = @Id AND is_active = 'Y')",
                new { Id = id });

            if (!exists)
            {
                return NotFound(new { message = $"Payment collection with ID {id} not found" });
            }

            // Validate doctor if provided
            if (dto.DoctorId.HasValue)
            {
                var doctorExists = await _connection.ExecuteScalarAsync<bool>(
                    "SELECT EXISTS(SELECT 1 FROM Doctors WHERE doctor_id = @DoctorId AND is_active = 'Y')",
                    new { dto.DoctorId });

                if (!doctorExists)
                {
                    return NotFound(new { message = $"Doctor with ID {dto.DoctorId} not found" });
                }
            }

            // Validate hospital if provided
            if (dto.HospitalId.HasValue)
            {
                var hospitalExists = await _connection.ExecuteScalarAsync<bool>(
                    "SELECT EXISTS(SELECT 1 FROM Hospitals WHERE hospital_id = @HospitalId AND is_active = 'Y')",
                    new { dto.HospitalId });

                if (!hospitalExists)
                {
                    return NotFound(new { message = $"Hospital with ID {dto.HospitalId} not found" });
                }
            }

            // Build update SQL dynamically
            var updateFields = new List<string>();
            var parameters = new DynamicParameters();
            parameters.Add("Id", id);

            if (!string.IsNullOrEmpty(dto.CollectionDate))
            {
                updateFields.Add("collection_date = @CollectionDate");
                parameters.Add("CollectionDate", DateOnly.Parse(dto.CollectionDate));
            }

            if (!string.IsNullOrEmpty(dto.CollectedBy))
            {
                updateFields.Add("collected_by = @CollectedBy");
                parameters.Add("CollectedBy", dto.CollectedBy);
            }

            if (dto.DoctorId.HasValue)
            {
                updateFields.Add("doctor_id = @DoctorId");
                parameters.Add("DoctorId", dto.DoctorId.Value);
            }

            if (dto.HospitalId.HasValue)
            {
                updateFields.Add("hospital_id = @HospitalId");
                parameters.Add("HospitalId", dto.HospitalId.Value);
            }

            if (dto.Amount.HasValue)
            {
                updateFields.Add("amount = @Amount");
                parameters.Add("Amount", dto.Amount.Value);
            }

            if (dto.Remarks != null)
            {
                updateFields.Add("remarks = @Remarks");
                parameters.Add("Remarks", dto.Remarks);
            }

            if (updateFields.Count == 0)
            {
                return BadRequest(new { message = "No fields to update" });
            }

            updateFields.Add("updated_at = NOW()");

            var sql = $"UPDATE PaymentCollections SET {string.Join(", ", updateFields)} WHERE collection_id = @Id";

            await _connection.ExecuteAsync(sql, parameters);

            return Ok(new { message = "Payment collection updated successfully" });
        }
        catch (FormatException)
        {
            return BadRequest(new { message = "Invalid date format. Use yyyy-MM-dd" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating payment collection with ID {CollectionId}", id);
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // DELETE: api/PaymentCollections/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePaymentCollection(int id)
    {
        try
        {
            if (_connection.State != System.Data.ConnectionState.Open)
                await _connection.OpenAsync();

            // Check if collection exists
            var exists = await _connection.ExecuteScalarAsync<bool>(
                "SELECT EXISTS(SELECT 1 FROM PaymentCollections WHERE collection_id = @Id AND is_active = 'Y')",
                new { Id = id });

            if (!exists)
            {
                return NotFound(new { message = $"Payment collection with ID {id} not found" });
            }

            // Soft delete
            var sql = "UPDATE PaymentCollections SET is_active = 'N', updated_at = NOW() WHERE collection_id = @Id";
            await _connection.ExecuteAsync(sql, new { Id = id });

            return Ok(new { message = "Payment collection deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting payment collection with ID {CollectionId}", id);
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
