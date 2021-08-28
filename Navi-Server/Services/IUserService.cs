using System.Threading.Tasks;
using Navi_Server.Exchange;
using Navi_Server.Models;

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
    }
}