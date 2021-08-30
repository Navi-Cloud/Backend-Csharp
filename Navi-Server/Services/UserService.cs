using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using MongoDB.Driver;
using Navi_Server.Exchange;
using Navi_Server.Models;
using Navi_Server.Models.DTO;
using Navi_Server.Repositories;

namespace Navi_Server.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly IFileRepository _fileRepository;

        public UserService(IUserRepository userRepository, IJwtService jwtService, IFileRepository fileRepository)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _fileRepository = fileRepository;
        }
        
        /// <summary>
        /// See <see cref="IUserService.RegisterUserAsync"/> for more information.
        /// </summary>
        /// <param name="userRequest"></param>
        /// <returns></returns>
        public async Task<ExecutionResult<User>> RegisterUserAsync(User userRequest)
        {
            try
            {
                await _userRepository.RegisterUser(userRequest);
            }
            catch (Exception superException)
            {
                return HandleRegisterError(superException, userRequest);
            }
            
            // Create Folder
            var newFolderMetadata = new FileMetadata
            {
                FileOwnerEmail = userRequest.UserEmail,
                IsFolder = true,
                VirtualParentDirectory = "",
                VirtualDirectory = "/"
            };
            await _fileRepository.CreateFolderAsync(newFolderMetadata);

            return new ExecutionResult<User>
            {
                ResultType = ExecutionResultType.SUCCESS,
                Value = userRequest
            };
        }
        
        /// <summary>
        /// See <see cref="IUserService.LoginUserAsync"/> for more information.
        /// </summary>
        /// <param name="loginRequest"></param>
        /// <returns></returns>
        public async Task<ExecutionResult<string>> LoginUserAsync(UserLoginRequest loginRequest)
        {
            var user = await _userRepository.FindUserByEmailAsync(loginRequest.UserEmail);
            var passwordVerified = user?.CheckPassword(loginRequest.UserPassword);
            
            if (passwordVerified is null or false)
            {
                return new ExecutionResult<string>
                {
                    ResultType = ExecutionResultType.LoginFailed,
                    Message = "Login Failed! User Email or Password is wrong!"
                };
            }

            return new ExecutionResult<string>
            {
                ResultType = ExecutionResultType.SUCCESS,
                Value = _jwtService.GenerateJwtToken(user)
            };
        }

        /// <summary>
        /// Handle <see cref="RegisterUserAsync"/>'s exception if required. 
        /// </summary>
        /// <param name="superException">Master Exception[Supertype Exception]</param>
        /// <param name="toRegister">User entity tried to register.</param>
        /// <returns>See <see cref="IUserService.RegisterUserAsync"/> for more details.</returns>
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