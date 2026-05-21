using Serilog.Context;
using System.Security.Claims;

namespace Trim.Middleware
{
    public class UserLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public UserLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "UnknownUser";
            var userName = context.User.Identity?.Name ?? "UnknownUser";
            using (LogContext.PushProperty("UserId", userId))
            using (LogContext.PushProperty("UserName", userName))
            {
                await _next(context);
            }
        }
    } 
}
