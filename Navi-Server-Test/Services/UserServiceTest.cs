using System;
using System.Reflection;
using System.Runtime.Serialization;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using Navi_Server_Test.Helper;
using Navi_Server.Exchange;
using Navi_Server.Models;
using Navi_Server.Models.DTO;
using Navi_Server.Repositories;
using Navi_Server.Services;
using Xunit;

namespace Navi_Server_Test.Services
{
    public class UserServiceTest
    {
        private readonly IUserService _userService;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IJwtService> _mockJwtService;
        private readonly Mock<IFileRepository> _mockFileRepository;
        private readonly User _mockUser = new() {UserEmail = "kangdroid@testmail.com", UserPassword = ""};
        
        public UserServiceTest()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockJwtService = new Mock<IJwtService>();
            _mockFileRepository = new Mock<IFileRepository>();
            _userService = new UserService(_mockUserRepository.Object, _mockJwtService.Object, _mockFileRepository.Object);
        }

        private MongoWriteException CreateMongoException(ServerErrorCategory category)
        {
            var writeError = (WriteError) FormatterServices.GetUninitializedObject(typeof(WriteError));
            var writeErrorCategory =
                typeof(WriteError).GetField("_category", BindingFlags.NonPublic | BindingFlags.Instance);
            writeErrorCategory?.SetValue(writeError, category);
            
            var exceptionInfo = (MongoWriteException)FormatterServices.GetUninitializedObject(typeof(MongoWriteException));
            var toSet = typeof(MongoWriteException).GetField("_writeError", BindingFlags.NonPublic | BindingFlags.Instance);
            toSet.SetValue(exceptionInfo, writeError);

            return exceptionInfo;
        }

        [Fact(DisplayName = "RegisterUserAsync: RegisterUserAsync should return SUCCESS when everything is ok.")]
        public async void Is_RegisterUserAsync_Should_Return_Success_When_All_Ok()
        {
            // Let
            _mockUserRepository.Setup(a => a.RegisterUser(It.IsAny<User>()))
                .ReturnsAsync(_mockUser);
            
            // Do
            var result = await _userService.RegisterUserAsync(_mockUser);
            
            // Check
            Assert.NotNull(result);
            Assert.Equal(ExecutionResultType.SUCCESS, result.ResultType);
            Assert.Equal(_mockUser, result.Value);
        }

        [Fact(DisplayName =
            "RegisterUserAsync: RegisterUserAsync should return DuplicatedId when duplicated id found.")]
        public async void Is_RegisterUserAsync_Returns_DuplicatedId_When_DuplicatedInsert()
        {
            // Let
            _mockUserRepository.Setup(a => a.RegisterUser(It.IsAny<User>()))
                .Throws(CreateMongoException(ServerErrorCategory.DuplicateKey));
            
            // Do
            var result = await _userService.RegisterUserAsync(_mockUser);
            
            // Check
            Assert.Equal(ExecutionResultType.DuplicatedID, result.ResultType);
        }

        [Theory(DisplayName =
            "RegisterUserAsync: RegisterUserAsync should return unknown when other mongo write exception occurred.")]
        [InlineData(ServerErrorCategory.Uncategorized)]
        [InlineData(ServerErrorCategory.ExecutionTimeout)]
        public async void Is_RegisterUserAsync_Returns_Unknown_When_Other_MongoWriteException(ServerErrorCategory category)
        {
            // Let
            _mockUserRepository.Setup(a => a.RegisterUser(It.IsAny<User>()))
                .Throws(CreateMongoException(category));
            
            // Do
            var result = await _userService.RegisterUserAsync(_mockUser);
            
            // Check
            Assert.Equal(ExecutionResultType.Unknown, result.ResultType);
        }

        [Fact(DisplayName = "RegisterUserAsync: RegisterUserAsync should return unknown when other error occurred.")]
        public async void Is_RegisterUserAsync_Returns_Unknown_When_Other_Error_Occurred()
        {
            // Let
            _mockUserRepository.Setup(a => a.RegisterUser(It.IsAny<User>()))
                .Throws(new Exception());
            
            // Do
            var result = await _userService.RegisterUserAsync(_mockUser);
            
            // Check
            Assert.Equal(ExecutionResultType.Unknown, result.ResultType);
        }

        [Fact(DisplayName =
            "LoginUserAsync: LoginUserAsync should return Result type loginFailed when id is non-existent id.")]
        public async void Is_LoginUserAsync_Returns_LoginFailed_When_Id_Not_Exists()
        {
            // Let
            _mockUserRepository.Setup(a => a.FindUserByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(value: null);
            
            // Do
            var result = await _userService.LoginUserAsync(new UserLoginRequest
                {UserEmail = "_mockUser.UserEmail", UserPassword = ""});
            
            // Check
            Assert.Equal(ExecutionResultType.LoginFailed, result.ResultType);
        }
        
        [Fact(DisplayName =
            "LoginUserAsync: LoginUserAsync should return Result type loginFailed when pw is wrong.")]
        public async void Is_LoginUserAsync_Returns_LoginFailed_When_Pw_Is_Wrong()
        {
            // Let
            _mockUserRepository.Setup(a => a.FindUserByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(_mockUser);
            
            // Do
            var result = await _userService.LoginUserAsync(new UserLoginRequest
                {UserEmail = _mockUser.UserEmail, UserPassword = "somewhat_wrong"});
            
            // Check
            Assert.Equal(ExecutionResultType.LoginFailed, result.ResultType);
        }

        [Fact(DisplayName = "LoginUserAsync: LoginUserAsync should return jwt token well.")]
        public async void Is_LoginUserAsync_Returns_Well()
        {
            // Let
            _mockUserRepository.Setup(a => a.FindUserByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(_mockUser);
            _mockJwtService.Setup(a => a.GenerateJwtToken(_mockUser))
                .Returns("TestToken");
            
            // Do
            var result = await _userService.LoginUserAsync(new UserLoginRequest
                {UserEmail = _mockUser.UserEmail, UserPassword = ""});
            
            // Check
            Assert.Equal(ExecutionResultType.SUCCESS, result.ResultType);
            Assert.Equal("TestToken", result.Value);
        }
    }
}