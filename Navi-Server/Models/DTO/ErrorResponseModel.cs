using System.Diagnostics.CodeAnalysis;

namespace Navi_Server.Models.DTO
{
    [ExcludeFromCodeCoverage]
    public class ErrorResponseModel
    {
        public string TraceId { get; set; }
        public string Message { get; set; }
    }
}