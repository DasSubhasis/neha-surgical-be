using System.Security.Claims;

namespace NehaSurgicalAPI.Middleware;

public class JwtAuthMiddleware
{
    private readonly RequestDelegate _next;

    public JwtAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip JWT validation for login endpoint
        if (context.Request.Path.Equals("/api/auth/login", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        // Check if JWT token exists in Authorization header
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (!string.IsNullOrEmpty(token))
        {
            // Token validation is handled by ASP.NET Core Authentication middleware
            // This middleware just extracts user info from validated token
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = context.User.FindFirst(ClaimTypes.Email)?.Value;
            var role = context.User.FindFirst(ClaimTypes.Role)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                context.Items["SystemUserId"] = int.Parse(userId);
                context.Items["SystemUserEmail"] = email;
                context.Items["SystemUserRole"] = role;
            }
        }

        await _next(context);
    }
}
