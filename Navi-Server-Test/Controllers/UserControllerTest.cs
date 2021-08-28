using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Navi_Server_Test.Helper;
using Navi_Server.Models;
using Navi_Server.Models.DTO;
using Navi_Server.Repositories;
using Xunit;

namespace Navi_Server_Test.Controllers
{
    // Integration Test
    public class UserControllerTest : TestServerInitializer
    {
        private readonly User _mockUser = new()
        {
            UserEmail = "kangdroid@alkkldsafkljdsfa.com",
            UserPassword = "testPassword"
        };

        private readonly UserLoginRequest _mockUserLoginRequest = new ()
        {
            UserEmail = "kangdroid@alkkldsafkljdsfa.com",
            UserPassword = "testPassword"
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

        [Fact(DisplayName = "POST /api/auth/login should return ok with token when user logged-in.")]
        public async void Is_LoginUser_Works_Well()
        {
            // Let
            _testServer.Services.GetService<IUserRepository>().RegisterUser(_mockUser);

            // Do
            var response =
                await _httpClient.PostForEntity<Dictionary<string, string>, UserLoginRequest>("/api/auth/login", _mockUserLoginRequest,
                    null);
            
            // Assert
            Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
            Assert.NotNull(response.Body);
            Assert.True(response.Body.ContainsKey("userToken"));
            Assert.NotEmpty(response.Body["userToken"]);
        }
        
        [Fact(DisplayName = "POST /api/auth/login should return 409 when either id or password is wrong.")]
        public async void Is_LoginUser_Returns_409_When_Id_Or_Password_Wrong()
        {
            // Do
            var response =
                await _httpClient.PostForEntity<Dictionary<string, string>, UserLoginRequest>("/api/auth/login", _mockUserLoginRequest,
                    null);
            
            // Assert
            Assert.Equal(StatusCodes.Status403Forbidden, response.StatusCode);
        }
    }
}