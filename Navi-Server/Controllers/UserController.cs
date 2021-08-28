using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Navi_Server.Exchange;
using Navi_Server.Models;
using Navi_Server.Models.DTO;
using Navi_Server.Services;

namespace Navi_Server.Controllers
{
    [ApiController]
    [Route("/api/auth")]
    public class UserController: CustomControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }
        
        /// <summary>
        /// RegisterUser: Register User to server.
        /// </summary>
        /// <param name="fromUser">User Entity to register.</param>
        /// <returns>
        /// <para>Empty[OK] response when user registered correctly.</para>
        /// <para>CONFLICT response when user email exists.</para>
        /// <para>500 response when unknown error occurred.</para>
        /// </returns>
        [HttpPost("join")]
        public async Task<IActionResult> RegisterUser(User fromUser)
        {
            var result = await _userService.RegisterUserAsync(fromUser);
            
            // Handled Case
            var handledCase = new Dictionary<ExecutionResultType, LazyLoadAction>
            {
                {ExecutionResultType.SUCCESS, () => Ok()},
                {ExecutionResultType.DuplicatedID, () => Conflict(new ErrorResponseModel {Message = result.Message, TraceId = HttpContext.TraceIdentifier})}
            };

            return ResultCaseHandler(handledCase, result.ResultType);
        }
        
        /// <summary>
        /// LoginUser: Login user to server!
        /// </summary>
        /// <param name="userLoginRequest">The user-input email and password.</param>
        /// <returns>
        /// <para>200 OK with Jwt Token Response when User logged-in Correctly.</para>
        /// <para>403 FORBIDDEN when email or password is wrong.</para>
        /// </returns>
        [HttpPost("login")]
        public async Task<IActionResult> LoginUser(UserLoginRequest userLoginRequest)
        {
            var result = await _userService.LoginUserAsync(userLoginRequest);
            
            
            // Handled Case
            var handledCase = new Dictionary<ExecutionResultType, LazyLoadAction>
            {
                {ExecutionResultType.SUCCESS, () => Ok(new {userToken = result.Value})},
                {
                    ExecutionResultType.LoginFailed, () =>
                        new ObjectResult(new ErrorResponseModel {Message = "Email or Password is wrong!"})
                        {
                            StatusCode = StatusCodes.Status403Forbidden
                        }
                }
            };

            return ResultCaseHandler(handledCase, result.ResultType);
        }
    }
}