using System.Diagnostics.CodeAnalysis;

namespace Navi_Server.Models.DTO
{
    [ExcludeFromCodeCoverage]
    public class UserLoginRequest
    {
        public string UserEmail { get; set; }
        public string UserPassword { get; set; }
    }
}