using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Navi_Server.Models.DTO;

namespace Navi_Server.Middleware
{
    [ExcludeFromCodeCoverage]
    public class CustomUserValidator : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;
            if (!httpContext.Items.ContainsKey("userId"))
                context.Result = new UnauthorizedObjectResult(
                    new ErrorResponseModel
                    {
                        TraceId = httpContext.TraceIdentifier,
                        Message = "Unauthorized User! Please login."
                    });
            base.OnActionExecuting(context);
        }
    }
}