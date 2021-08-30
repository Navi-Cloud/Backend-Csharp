using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Navi_Server_Test.Helper;
using Navi_Server.Models;
using Navi_Server.Models.DTO;
using Navi_Server.Repositories;
using Xunit;

namespace Navi_Server_Test.Controllers
{
    public class FolderControllerTest : TestServerInitializer
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

        private async Task<string> GetUserToken()
        {
            // Let
            _testServer.Services.GetService<IUserRepository>().RegisterUser(_mockUser);
            
            var response =
                await _httpClient.PostForEntity<Dictionary<string, string>, UserLoginRequest>("/api/auth/login", _mockUserLoginRequest,
                    null);

            return response.Body["userToken"];
        }
        
        [Fact(DisplayName = "GET /api/folder should return list of files well.")]
        public async void Is_ExploreFolder_Works_Well()
        {
            // Let
            var userToken = await GetUserToken();
            var fileRepository = _testServer.Services.GetService<IFileRepository>();
            await fileRepository.CreateFolderAsync(new FileMetadata
            {
                FileOwnerEmail = _mockUser.UserEmail,
                IsFolder = true,
                VirtualDirectory = "/",
                VirtualParentDirectory = ""
            });
            await fileRepository.UploadFileAsync(new FileMetadata
            {
                FileOwnerEmail = _mockUser.UserEmail,
                IsFolder = false,
                VirtualDirectory = "/test.txt",
                VirtualParentDirectory = "/"
            }, new MemoryStream(Encoding.UTF8.GetBytes("")));
            
            // Do
            var result = await _httpClient.GetForEntity<List<FileMetadata>>("/api/folder?targetFolder=/", userToken);
            
            // Assert
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }
        
        [Fact(DisplayName = "POST /api/folder should works well.")]
        public async void Is_CreateFolder_Works_Well()
        {
            // Let
            var userToken = await GetUserToken();
            var parentFolderMock = new FileMetadata
            {
                VirtualDirectory = "/"
            };
            var uploadRequest = new CreateFolderRequest
            {
                NewFolderName = "testFolder",
                ParentFolderMetadata = parentFolderMock
            };
            
            // Do
            var result =
                await _httpClient.PostForEntity<FileMetadata, CreateFolderRequest>("/api/folder", uploadRequest,
                    userToken);
            
            // Check
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
            
            // Duplicate Check
            result = await _httpClient.PostForEntity<FileMetadata, CreateFolderRequest>("/api/folder", uploadRequest,
                userToken);
            
            Assert.Equal(StatusCodes.Status409Conflict, result.StatusCode);
        }
    }
}