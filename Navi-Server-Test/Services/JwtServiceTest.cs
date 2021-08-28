using Microsoft.Extensions.Configuration;
using Navi_Server_Test.Helper;
using Navi_Server.Models;
using Navi_Server.Services;
using Xunit;

namespace Navi_Server_Test.Services
{
    public class JwtServiceTest
    {
        private readonly JwtService _jwtService;
        private readonly User _mockUser = new User()
        {
            UserEmail = "kangdroid@test.com",
            UserPassword = "testPassword"
        };
        public JwtServiceTest()
        {
            using (var configuration = TestConfiguration.GetTestConfigurationStream())
            {
                _jwtService =
                    new JwtService(new ConfigurationBuilder().AddJsonStream(configuration).Build());
            }
        }

        [Fact]
        public void Is_GenerateJwtToken_Works_Well()
        {

            // Do
            var token = _jwtService.GenerateJwtToken(_mockUser);
            var jwtSecurityToken = _jwtService.GetValidatedToken(token);
            var gotUserId = _jwtService.GetUserEmailFromToken(jwtSecurityToken);
            
            Assert.NotNull(token);
            Assert.Equal(_mockUser.UserEmail, gotUserId);
        }

        [Fact]
        public void Is_GetValidatedToken_Works_Well()
        {
            // Do
            var token = _jwtService.GenerateJwtToken(_mockUser);
            var jwtSecurityToken = _jwtService.GetValidatedToken(token);
            
            Assert.NotNull(jwtSecurityToken);
        }

        [Fact]
        public void Is_GetValidatedToken_Returns_Null()
        {
            var jwtSecurityToken = _jwtService.GetValidatedToken("testToken");
            
            Assert.Null(jwtSecurityToken);
        }

        [Fact] // Normal test is on generatejwttoken test
        public void Is_GetUserIdFromToken_Returns_Null()
        {
            var userId = _jwtService.GetUserEmailFromToken(null);
            
            // Check
            Assert.Null(userId);
        }
    }
}