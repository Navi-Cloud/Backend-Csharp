using System.Net.Http;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Navi_Server;
using Navi_Server.Repositories;

namespace Navi_Server_Test.Helper
{
    public abstract class TestServerInitializer : MongoHelper
    {
        protected readonly HttpClient _httpClient;
        protected readonly TestServer _testServer;
        
        protected TestServerInitializer()
        {
            _testServer = new TestServer(WebHost.CreateDefaultBuilder()
                .UseStartup<Startup>().UseConfiguration(_configuration));
            _httpClient = _testServer.CreateClient();
        }
    }
}