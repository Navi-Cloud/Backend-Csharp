using System.Threading.Tasks;
using Navi_Server.Exchange;
using Navi_Server.Models;
using Navi_Server.Models.DTO;

namespace Navi_Server.Services
{
    public interface IUserService
    {
        /// <summary>
        /// Register user to server!
        /// </summary>
        /// <param name="userRequest">User information to register.</param>
        /// <returns>
        /// <para>ExecutionResult of User Entity.</para>
        /// <para>Returns ExecutionResult.ResultType = DuplicatedId when duplicated indexes are found.</para>
        /// <para>Returns ExecutionResult.ResultType = Unknown when un-handled exception occurred.</para>
        /// <para>Returns ExecutionResult.ResultType = SUCCESS with corresponding saved user entity when Registering succeeds.</para>
        /// </returns>
        public Task<ExecutionResult<User>> RegisterUserAsync(User userRequest);
        
        /// <summary>
        /// Login User and user can have freshly fresh token.
        /// </summary>
        /// <param name="loginRequest">Login Request containing User Email, User Password.</param>
        /// <returns>
        /// <para>Execution Result of Token itself</para>
        /// <para>Returns ExecutionResult.ResultType = LoginFailed when userId or userPassword is wrong.</para>
        /// <para>Returns ExecutionResult.ResultType = Success with jwt token.</para>
        /// </returns>
        public Task<ExecutionResult<string>> LoginUserAsync(UserLoginRequest loginRequest);
    }
}