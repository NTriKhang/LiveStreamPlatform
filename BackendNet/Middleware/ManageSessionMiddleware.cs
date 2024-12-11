using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BackendNet.Middleware
{
    public class ManageSessionMiddleware
    {
        private readonly RequestDelegate _next;
        public ManageSessionMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        [AllowAnonymous]
        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User.Identity != null && context.User.Identity.IsAuthenticated)
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!string.IsNullOrEmpty(userId))
                {
  
                }
                else
                {
                    Console.WriteLine("User ID not found in claims.");
                }
            }
            else
            {
                Console.WriteLine("User is not authenticated.");
            }

            await _next(context);
        }
    }
}
