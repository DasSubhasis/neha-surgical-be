using Npgsql;
using Dapper;

namespace NehaSurgicalAPI.Middleware;

public class ApiKeyAuthMiddleware
{
    private readonly RequestDelegate _next;

    public ApiKeyAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, NpgsqlConnection connection)
    {
        // Skip authentication for Swagger UI, auth endpoints, and public APIs
        if (context.Request.Path.StartsWithSegments("/swagger") || 
            context.Request.Path.StartsWithSegments("/swagger-login") ||
            context.Request.Path.StartsWithSegments("/api/auth") ||
            context.Request.Path.StartsWithSegments("/api/OtpAuth") ||
            context.Request.Path.StartsWithSegments("/api/Doctors") ||
            context.Request.Path.StartsWithSegments("/api/Hospitals") ||
            context.Request.Path.StartsWithSegments("/api/ItemGroups") ||
            context.Request.Path.StartsWithSegments("/api/SystemUsers") ||
            context.Request.Path.StartsWithSegments("/api/Roles") ||
            context.Request.Path.StartsWithSegments("/api/Menus"))
        {
            await _next(context);
            return;
        }

        // Check for API key in header
        if (!context.Request.Headers.TryGetValue("X-API-Key", out var extractedApiKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { message = "API Key is missing. Frontend application must provide X-API-Key header." });
            return;
        }

        // Validate API key from database
        var sql = "SELECT user_id, username, role FROM api_users WHERE api_key = @ApiKey AND is_active = 'Y'";
        var apiUser = await connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { ApiKey = extractedApiKey.ToString() });

        if (apiUser == null)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { message = "Invalid or inactive API Key" });
            return;
        }

        // Store API user info in context
        context.Items["ApiUserId"] = apiUser.user_id;
        context.Items["ApiUsername"] = apiUser.username;
        context.Items["ApiUserRole"] = apiUser.role;

        await _next(context);
    }
}
