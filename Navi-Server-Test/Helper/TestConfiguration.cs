using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Navi_Server_Test.Helper
{
    public static class TestConfiguration
    {
        public static MemoryStream GetTestConfigurationStream()
        {
            var testConfiguration = new
            {
                ConnectionStrings = new 
                {
                    MongoConnection = "mongodb://root:testPassword@localhost:27017",
                    MongoDbName = Guid.NewGuid().ToString()
                },
                SecurityInfo = new
                {
                    TokenSecurityKey = "TestButLongEnoughKeyNaviServerTestOneTwoThree",
                    TokenIssuer = "Navi-Server",
                    TokenAudience = "Navi-Server"
                }
            };
            
            var jsonString = JsonConvert.SerializeObject(testConfiguration);
            var byteArray = Encoding.UTF8.GetBytes(jsonString);

            return new MemoryStream(byteArray);
        }
    }
}