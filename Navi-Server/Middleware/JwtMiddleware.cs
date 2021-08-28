using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Navi_Server.Services;

namespace Navi_Server.Middleware
{
    [ExcludeFromCodeCoverage]
    public class JwtMiddleware
    {
        private readonly RequestDelegate _requestDelegate;
        private readonly IJwtService _jwtService;

        public JwtMiddleware(RequestDelegate requestDelegate, IJwtService jwtService)
        {
            _jwtService = jwtService;
            _requestDelegate = requestDelegate;
        }
        
        /// <summary>
        /// Get User Token from Http Header, validate, save it to http context.
        /// </summary>
        /// <param name="context">Per-Request-Wide shared HttpContext</param>
        public async Task Invoke(HttpContext context)
        {
            var userToken = context.Request.Headers["X-API-AUTH"].FirstOrDefault();
            if (userToken != null)
            {
                // Validate Token
                var validatedToken = _jwtService.GetValidatedToken(userToken);
                
                // Get UserID
                var userId = _jwtService.GetUserEmailFromToken(validatedToken);
                
                // Put to context
                context.Items["userId"] = userId;
            }

            await _requestDelegate(context);
        }
    }
}