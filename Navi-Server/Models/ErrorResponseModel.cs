using System.Diagnostics.CodeAnalysis;

namespace Navi_Server.Models
{
    [ExcludeFromCodeCoverage]
    public class ErrorResponseModel
    {
        public string TraceId { get; set; }
        public string Message { get; set; }
    }
}