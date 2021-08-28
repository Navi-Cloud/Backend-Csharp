using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using Navi_Server.Exchange;
using Navi_Server.Models;

namespace Navi_Server.Repositories
{
    public class UserRepository: IUserRepository
    {
        /// <summary>
        /// Mongo Collection information about user entity.
        /// </summary>
        private readonly IMongoCollection<User> _mongoCollection;
        
        /// <summary>
        /// <para>Setup User Repository.</para>
        /// <para>Note User.UserEmail is set 'UNIQUE' Index!</para>
        /// </summary>
        /// <param name="mongoContext">
        /// <para>Dependency-Injected Mongo Context, responsible for whole database.</para>
        /// <para>See <see cref="MongoContext"/> for more details.</para>
        /// </param>
        public UserRepository(MongoContext mongoContext)
        {
            _mongoCollection = mongoContext._MongoDatabase.GetCollection<User>(nameof(User));
            
            // Setup Index
            _mongoCollection.Indexes.CreateOne(
                new CreateIndexModel<User>(
                    new BsonDocument { {"userEmail", 1}}, 
                    new CreateIndexOptions { Unique = true })
            );
        }
        
        /// <summary>
        /// <para>Implementation of IUserRepository.RegisterUser.</para>
        /// </summary>
        /// <param name="toRegister">User to register</param>
        /// <returns>See <see cref="IUserRepository.RegisterUser"/> for more details.</returns>
        public async Task<User> RegisterUser(User toRegister)
        {
            await _mongoCollection.InsertOneAsync(toRegister);
            return toRegister;
        }
    }
}