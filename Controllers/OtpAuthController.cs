using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Dapper;
using NehaSurgicalAPI.Models;
using NehaSurgicalAPI.DTOs;
using NehaSurgicalAPI.Services;
using Microsoft.AspNetCore.Authorization;

namespace NehaSurgicalAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OtpAuthController : ControllerBase
{
    private readonly NpgsqlConnection _connection;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly ILogger<OtpAuthController> _logger;

    public OtpAuthController(NpgsqlConnection connection, IJwtService jwtService, IEmailService emailService, ILogger<OtpAuthController> logger)
    {
        _connection = connection;
        _jwtService = jwtService;
        _emailService = emailService;
        _logger = logger;
    }

    // POST: api/OtpAuth/send-otp
    [HttpPost("send-otp")]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if user exists and is active
            var userSql = @"SELECT system_user_id as SystemUserId, email as Email, full_name as FullName, 
                           role_id as RoleId, is_active as IsActive 
                           FROM SystemUsers WHERE email = @Email";
            
            var user = await _connection.QueryFirstOrDefaultAsync<SystemUser>(userSql, new { request.Email });

            if (user == null)
            {
                return NotFound(new { message = "User not found with this email" });
            }

            if (user.IsActive != "Y")
            {
                return Unauthorized(new { message = "User account is inactive" });
            }

            // Generate 6-digit OTP
            var otp = new Random().Next(100000, 999999).ToString();
            var expiresAt = DateTime.UtcNow.AddMinutes(10); // OTP valid for 10 minutes

            // Save OTP to database
            var otpSql = @"INSERT INTO UserOtps (system_user_id, otp_code, expires_at, is_used, created_at) 
                          VALUES (@SystemUserId, @OtpCode, @ExpiresAt, 'N', NOW())";
            
            await _connection.ExecuteAsync(otpSql, new
            {
                SystemUserId = user.SystemUserId,
                OtpCode = otp,
                ExpiresAt = expiresAt
            });

            // Send OTP via email service
            var emailSent = await _emailService.SendOtpEmailAsync(request.Email, user.FullName, otp);
            
            if (!emailSent)
            {
                _logger.LogWarning("Failed to send OTP email to {Email}, but OTP was saved", request.Email);
            }

            _logger.LogInformation("OTP generated for {Email}", request.Email);

            var response = new SendOtpResponseDto
            {
                Email = request.Email,
                ExpiresIn = "10 minutes"
            };

            return Ok(new { message = "OTP sent successfully to your email", data = response });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending OTP");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // POST: api/OtpAuth/verify-otp
    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get user
            var userSql = @"SELECT system_user_id as SystemUserId, email as Email, full_name as FullName, 
                           role_id as RoleId, phone_no as PhoneNo, is_active as IsActive 
                           FROM SystemUsers WHERE email = @Email";
            
            var user = await _connection.QueryFirstOrDefaultAsync<dynamic>(userSql, new { request.Email });

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid email or OTP" });
            }

            // PostgreSQL returns lowercase column names, so access as lowercase
            var isActive = user.isactive?.ToString() ?? "N";
            
            if (isActive != "Y")
            {
                return Unauthorized(new { message = "User account is inactive" });
            }

            // Verify OTP
            var otpSql = @"SELECT otp_id as OtpId, system_user_id as SystemUserId, otp_code as OtpCode, 
                          expires_at as ExpiresAt, is_used as IsUsed 
                          FROM UserOtps 
                          WHERE system_user_id = @SystemUserId 
                          AND otp_code = @OtpCode 
                          AND is_used = 'N' 
                          AND expires_at > @Now
                          ORDER BY created_at DESC 
                          LIMIT 1";
            
            var otp = await _connection.QueryFirstOrDefaultAsync<UserOtp>(otpSql, new
            {
                SystemUserId = (int)user.systemuserid,
                OtpCode = request.Otp,
                Now = DateTime.UtcNow
            });

            if (otp == null)
            {
                return Unauthorized(new { message = "Invalid or expired OTP" });
            }

            // Mark OTP as used
            await _connection.ExecuteAsync(
                "UPDATE UserOtps SET is_used = 'Y' WHERE otp_id = @OtpId",
                new { otp.OtpId });

            // Get role name for JWT
            var roleSql = "SELECT role_name FROM Roles WHERE role_id = @RoleId";
            var roleName = await _connection.QueryFirstOrDefaultAsync<string>(roleSql, new { RoleId = (int)user.roleid }) ?? "User";

            // Generate JWT token
            var token = _jwtService.GenerateToken((int)user.systemuserid, user.email, roleName, user.fullname);
            var refreshToken = _jwtService.GenerateRefreshToken();

            var response = new LoginResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                User = new SystemUserDto
                {
                    SystemUserId = (int)user.systemuserid,
                    Email = user.email,
                    FullName = user.fullname,
                    RoleId = (int)user.roleid,
                    RoleName = roleName,
                    PhoneNo = user.phoneno,
                    IsActive = isActive
                }
            };

            return Ok(new { message = "Login successful", data = response });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying OTP");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // GET: api/OtpAuth/me
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

            var sql = @"SELECT su.system_user_id as SystemUserId, su.email as Email, su.full_name as FullName, 
                       su.role_id as RoleId, r.role_name as RoleName, su.phone_no as PhoneNo, su.is_active as IsActive 
                       FROM SystemUsers su
                       LEFT JOIN Roles r ON su.role_id = r.role_id
                       WHERE su.system_user_id = @SystemUserId";
            
            var user = await _connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { SystemUserId = userId });

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var response = new SystemUserDto
            {
                SystemUserId = user.SystemUserId,
                Email = user.Email,
                FullName = user.FullName,
                RoleId = user.RoleId,
                RoleName = user.RoleName,
                PhoneNo = user.PhoneNo,
                IsActive = user.IsActive
            };

            return Ok(new { message = "User retrieved successfully", data = response });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
