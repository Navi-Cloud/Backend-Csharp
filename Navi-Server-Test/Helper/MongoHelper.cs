using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using Navi_Server.Repositories;
using Newtonsoft.Json;

namespace Navi_Server_Test.Helper
{
    public abstract class MongoHelper
    {
        protected readonly IConfiguration _configuration;
        protected readonly MongoContext _mongoContext;
        
        protected MongoHelper()
        {
            using (var configurationStream = TestConfiguration.GetTestConfigurationStream())
            {
                _configuration = new ConfigurationBuilder()
                    .AddJsonStream(configurationStream)
                    .Build();
            }

            _mongoContext = new MongoContext(_configuration);
        }

        // Destroy Database => It will completely drop database, so if you need to call this, make sure you have done your operation
        // with db before calling this.
        protected void DestroyDatabase()
        {
            _mongoContext._MongoClient.DropDatabase(_configuration.GetConnectionString("MongoDbName"));
        }
    }
}