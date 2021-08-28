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
        public async Task<ExecutionResult<User>> RegisterUser(User toRegister)
        {
            try
            {
                await _mongoCollection.InsertOneAsync(toRegister);
            }
            catch (Exception superException)
            {
                // Handle error if required.
                return HandleRegisterError(superException, toRegister);
            }

            return new ExecutionResult<User>
            {
                ResultType = ExecutionResultType.SUCCESS,
                Value = toRegister
            };
        }
        
        /// <summary>
        /// Handle <see cref="RegisterUser"/>'s exception if required. 
        /// </summary>
        /// <param name="superException">Master Exception[Supertype Exception]</param>
        /// <param name="toRegister">User entity tried to register.</param>
        /// <returns>See <see cref="IUserRepository.RegisterUser"/> for more details.</returns>
        [ExcludeFromCodeCoverage]
        private ExecutionResult<User> HandleRegisterError(Exception superException, User toRegister)
        {
            // When Error type is MongoWriteException
            if (superException is MongoWriteException mongoWriteException)
            {
                // When Error Type is 'Duplicate Key'
                if (mongoWriteException.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    return new ExecutionResult<User>
                    {
                        ResultType = ExecutionResultType.DuplicatedID,
                        Message = $"Duplicated Email found: {toRegister.UserEmail}"
                    };
                } // Else -> goto Unknown Error.
            }

            // Unknown if exception is not MongoWriteException.
            return new ExecutionResult<User>
            {
                ResultType = ExecutionResultType.Unknown,
                Message = $"Unknown Error Occurred! : {superException.Message}"
            };
        }
    }
}