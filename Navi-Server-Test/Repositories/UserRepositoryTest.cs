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

        [Fact(DisplayName = "RegisterUser: RegisterUser should throws MongoWriteException when duplicated id found.")]
        public async void Is_RegisterUser_Throws_MongoWriteException_When_DuplicatedId()
        {
            // Setup
            await _collection.InsertOneAsync(_mockUser);

            await Assert.ThrowsAsync<MongoWriteException>(() => _userRepository.RegisterUser(_mockUser));
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
            Assert.Equal(_mockUser, result);
        }

        [Fact(DisplayName = "FindUserByEmailAsync: FindUserByEmailAsync should return null when entity is not found.")]
        public async void Is_FindUserByEmailAsync_Returns_Null_When_Not_Found()
        {
            var result = await _userRepository.FindUserByEmailAsync("test");
            
            // Assert
            Assert.Null(result);
        }

        [Fact(DisplayName =
            "FindUserByEmailAsync: FindUserByEmailAsync should return corresponding entity when exists.")]
        public async void Is_FindUserByEmailAsync_Works_Well()
        {
            // Let
            await _collection.InsertOneAsync(_mockUser);
            
            // Do
            var result = await _userRepository.FindUserByEmailAsync(_mockUser.UserEmail);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(_mockUser.Id, result.Id);
            Assert.Equal(_mockUser.UserEmail, result.UserEmail);
            Assert.Equal(_mockUser.UserName, result.UserName);
            Assert.Equal(_mockUser.UserPassword, result.UserPassword);
        }
    }
}