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
            _MongoClient = new MongoClient(configuration.GetConnectionString("MongoConnection"));
            _MongoDatabase = _MongoClient.GetDatabase(configuration.GetConnectionString("MongoDbName"));
        }
    }
}