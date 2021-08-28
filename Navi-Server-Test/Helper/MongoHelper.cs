using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using Navi_Server.Repositories;
using Newtonsoft.Json;

namespace Navi_Server_Test.Helper
{
    public class ConnectionStrings
    {
        public string MongoConnection { get; set; }
        public string MongoDbName { get; set; }
    }
    
    public abstract class MongoHelper
    {
        private readonly ConnectionStrings _connectionStrings = new()
        {
            MongoConnection = "mongodb://root:testPassword@localhost:27017",
            MongoDbName = Guid.NewGuid().ToString()
        };
        
        protected readonly IConfiguration _configuration;
        protected readonly MongoContext _mongoContext;
        
        protected MongoHelper()
        {
            using (var configurationStream = GetConnectionStream())
            {
                _configuration = new ConfigurationBuilder()
                    .AddJsonStream(configurationStream)
                    .Build();
            }

            _mongoContext = new MongoContext(_configuration);
        }

        private MemoryStream GetConnectionStream()
        {
            var jsonString = JsonConvert.SerializeObject(new { ConnectionStrings = _connectionStrings });
            var byteArray = Encoding.UTF8.GetBytes(jsonString);
            return new MemoryStream(byteArray);
        }
        
        // Destroy Database => It will completely drop database, so if you need to call this, make sure you have done your operation
        // with db before calling this.
        protected void DestroyDatabase()
        {
            _mongoContext._MongoClient.DropDatabase(_connectionStrings.MongoDbName);
        }
    }
}