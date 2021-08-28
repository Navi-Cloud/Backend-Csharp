using Microsoft.AspNetCore.Http;
using Navi_Server_Test.Helper;
using Navi_Server.Models;
using Xunit;

namespace Navi_Server_Test.Controllers
{
    // Integration Test
    public class UserControllerTest : TestServerInitializer
    {
        private readonly User _mockUser = new()
        {
            UserEmail = "kangdroid@alkkldsafkljdsfa.com"
        };

        [Fact(DisplayName = "POST /api/auth/join should return ok when new user tries to join.")]
        public async void Is_RegisterUser_Works_Well()
        {
            // Do
            var response = await _httpClient.PostForEntity<object, User>("/api/auth/join", _mockUser, null);
            
            // Check
            Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
        }

        [Fact(DisplayName = "POST /api/auth/join should return 409 conflict when existing tries to join.")]
        public async void Is_RegisterUser_Returns_409_Duplicate_User()
        {
            // Let
            await _httpClient.PostForEntity<object, User>("/api/auth/join", _mockUser, null);
            
            // Do
            var response = await _httpClient.PostForEntity<object, User>("/api/auth/join", _mockUser, null);
            
            // Check
            Assert.Equal(StatusCodes.Status409Conflict, response.StatusCode);
        }
    }
}