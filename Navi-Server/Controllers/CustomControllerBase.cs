using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using Navi_Server.Exchange;
using Navi_Server.Models.DTO;

namespace Navi_Server.Controllers
{
    [ExcludeFromCodeCoverage]
    public class CustomControllerBase : ControllerBase
    {
        protected delegate IActionResult LazyLoadAction();
        [ExcludeFromCodeCoverage]
        protected IActionResult ResultCaseHandler(Dictionary<ExecutionResultType, LazyLoadAction> handledCase, ExecutionResultType actualResult)
        {
            return handledCase.ContainsKey(actualResult) switch
            {
                true => handledCase[actualResult].Invoke(),
                false => new ObjectResult(new ErrorResponseModel
                    {Message = "Unknown Error occurred!", TraceId = HttpContext.TraceIdentifier})
            };
        }
        
        protected ErrorResponseModel GetErrorResponseModel(string message)
        {
            return new ErrorResponseModel
            {
                Message = message,
                TraceId = HttpContext.TraceIdentifier
            };
        }
    }
}