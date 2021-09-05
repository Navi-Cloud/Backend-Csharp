using System.Security.Authentication;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Navi_Server.Repositories
{
    /// <summary>
    /// Mongo Context. Should be registered with 'Singleton' Object.
    /// </summary>
    public class MongoContext
    {
        /// <summary>
        /// Mongo Client object, can access whole db or cluster itself.
        /// </summary>
        public readonly MongoClient _MongoClient;
        
        /// <summary>
        /// Mongo Database Object, responsible for database itself.
        /// </summary>
        public readonly IMongoDatabase _MongoDatabase;
        
        public MongoContext(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("MongoConnection");
            var settings = MongoClientSettings.FromConnectionString(connectionString);

            if (configuration.GetConnectionString("MongoProduction") == "YES")
            {
                settings.SslSettings = new SslSettings {EnabledSslProtocols = SslProtocols.Tls12};
            }
            _MongoClient = new MongoClient(settings);
            _MongoDatabase = _MongoClient.GetDatabase(configuration.GetConnectionString("MongoDbName"));
        }
    }
}