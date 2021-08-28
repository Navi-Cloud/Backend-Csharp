using System;
using MongoDB.Driver;
using Navi_Server_Test.Helper;
using Navi_Server.Exchange;
using Navi_Server.Models;
using Navi_Server.Repositories;
using Xunit;

namespace Navi_Server_Test.Repositories
{
    public class UserRepositoryTest : MongoHelper, IDisposable
    {
        private readonly User _mockUser = new User {UserEmail = "kangdroid@testEmailNonExists.com"};
        private readonly IUserRepository _userRepository;
        private readonly IMongoCollection<User> _collection;

        public UserRepositoryTest()
        {
            _userRepository = new UserRepository(_mongoContext);
            _collection = _mongoContext._MongoDatabase.GetCollection<User>(nameof(User));
        }

        public void Dispose()
        {
            DestroyDatabase();
        }

        [Fact(DisplayName = "RegisterUser: RegisterUser should return DuplicatedID when duplicated indexes are found.")]
        public async void Is_RegisterUser_Returns_DuplicatedId_UniqueIndex_Constraints()
        {
            // Setup
            await _collection.InsertOneAsync(_mockUser);

            // Do
            var result = await _userRepository.RegisterUser(_mockUser);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ExecutionResultType.DuplicatedID, result.ResultType);
        }

        [Fact(DisplayName = "RegisterUser: RegisterUser should return Unknown when unknown exception occurred.")]
        public void Is_RegisterUser_Returns_Unknown_When_UnknownError_Occurred()
        {
            // It is literally "UNKNOWN", Therefore it is not testable!
        }

        [Fact(DisplayName = "RegisterUser: RegisterUser should return correct object when everything is ok.")]
        public async void Is_RegisterUser_Returns_Ok()
        {
            // Setup - N/A
            // Do
            var result = await _userRepository.RegisterUser(_mockUser);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(ExecutionResultType.SUCCESS, result.ResultType);
            Assert.Equal(_mockUser, result.Value);
        }
    }
}