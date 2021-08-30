using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
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
    public class FileControllerTest : TestServerInitializer
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

        [Fact(DisplayName = "POST /api/file should upload file well")]
        public async void Is_UploadFile_Works_Well()
        {
            var userToken = await GetUserToken();
            
            var testFilePath = Path.Combine(Path.GetTempPath(), "test.csv");
            await using var writer = new StreamWriter(testFilePath);
            await writer.WriteLineAsync("testContent");

            var content = new MultipartFormDataContent();
            content.Add(new StreamContent(File.OpenRead(testFilePath))
            {
                Headers =
                {
                    ContentLength = 100,
                    ContentType = new MediaTypeHeaderValue("text/plain")
                }
            }, "File", "test.csv");
            content.Add(new StringContent("/"), "ParentFolder");
            content.Headers.Add("X-API-AUTH", userToken);

            var result = await _httpClient.PostAsync("/api/file", content);
            
            Assert.Equal(StatusCodes.Status200OK, (int)result.StatusCode);
            
            // Check for duplicated check
            result = await _httpClient.PostAsync("/api/file", content);
            Assert.Equal(StatusCodes.Status409Conflict, (int)result.StatusCode);
        }

        [Fact(DisplayName = "GET /api/file should download corresponding file well.")]
        public async void Is_DownloadFile_Works_Well()
        {
            var userToken = await GetUserToken();
            
            // Upload Test File
            var testFilePath = Path.Combine(Path.GetTempPath(), "test.csv");
            await using var writer = new StreamWriter(testFilePath);
            await writer.WriteLineAsync("testContent");

            var content = new MultipartFormDataContent();
            content.Add(new StreamContent(File.OpenRead(testFilePath))
            {
                Headers =
                {
                    ContentLength = 100,
                    ContentType = new MediaTypeHeaderValue("text/plain")
                }
            }, "File", "test.csv");
            content.Add(new StringContent("/"), "ParentFolder");
            content.Headers.Add("X-API-AUTH", userToken);
            await _httpClient.PostAsync("/api/file", content);
            
            // Upload File Info
            var uploadedVpd = "/";
            var uploadedVd = "/test.csv";
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-API-AUTH", userToken);
            
            // Do
            var result = await _httpClient.GetAsync($"/api/file?vd={uploadedVd}&vpd={uploadedVpd}");
            
            // Check
            Assert.Equal(StatusCodes.Status200OK, (int)result.StatusCode);
            
            // Try downloading somewhat non-existence file
            result = await _httpClient.GetAsync($"/api/file?vd=asdfasdf&vpd=asdfs");
            Assert.Equal(StatusCodes.Status404NotFound, (int)result.StatusCode);
        }

        [Fact(DisplayName = "DELETE /api/file should work well.")]
        public async void Is_DeleteFile_Works_Well()
        {
            var userToken = await GetUserToken();
            
            // Upload Test File
            var testFilePath = Path.Combine(Path.GetTempPath(), "test.csv");
            await using var writer = new StreamWriter(testFilePath);
            await writer.WriteLineAsync("testContent");

            var content = new MultipartFormDataContent();
            content.Add(new StreamContent(File.OpenRead(testFilePath))
            {
                Headers =
                {
                    ContentLength = 100,
                    ContentType = new MediaTypeHeaderValue("text/plain")
                }
            }, "File", "test.csv");
            content.Add(new StringContent("/"), "ParentFolder");
            content.Headers.Add("X-API-AUTH", userToken);
            await _httpClient.PostAsync("/api/file", content);
            
            // Upload File Info
            var fileInfo = new FileMetadata
            {
                FileOwnerEmail = _mockUser.UserEmail,
                IsFolder = false,
                VirtualDirectory = "/test.csv",
                VirtualParentDirectory = "/"
            };

            HttpRequestMessage request = new HttpRequestMessage
            {
                Content = JsonContent.Create(fileInfo),
                Method = HttpMethod.Delete,
                RequestUri = new Uri("/api/file", UriKind.Relative),
                Headers =
                {
                    {"X-API-AUTH", userToken}
                }
            };

            var result = await _httpClient.SendAsync(request);
            Assert.Equal(StatusCodes.Status200OK, (int)result.StatusCode);
            
            // Check for not-found
            HttpRequestMessage notFoundRequest = new HttpRequestMessage
            {
                Content = JsonContent.Create(new FileMetadata()),
                Method = HttpMethod.Delete,
                RequestUri = new Uri("/api/file", UriKind.Relative),
                Headers =
                {
                    {"X-API-AUTH", userToken}
                }
            };
            result = await _httpClient.SendAsync(notFoundRequest);
            Assert.Equal(StatusCodes.Status404NotFound, (int)result.StatusCode);
        }
    }
}