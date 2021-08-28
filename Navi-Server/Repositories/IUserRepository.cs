using System.Threading.Tasks;
using Navi_Server.Exchange;
using Navi_Server.Models;

namespace Navi_Server.Repositories
{
    public interface IUserRepository
    {
        /// <summary>
        /// Register User to Repository.
        /// </summary>
        /// <param name="toRegister">To-be-registered User Entity</param>
        /// <returns>
        /// <para>ExecutionResult of User Entity.</para>
        /// <para>Returns ExecutionResult.ResultType = DuplicatedId when duplicated indexes are found.</para>
        /// <para>Returns ExecutionResult.ResultType = Unknown when un-handled exception occurred.</para>
        /// <para>Returns ExecutionResult.ResultType = SUCCESS with corresponding saved user entity when Registering succeeds.</para>
        /// </returns>
        public Task<ExecutionResult<User>> RegisterUser(User toRegister);
    }
}