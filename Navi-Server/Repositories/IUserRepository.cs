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
        /// <para>Registered User Entity when succeeds.</para>
        /// <para>May throw MongoWriteException when write operation fails.</para>
        /// </returns>
        public Task<User> RegisterUser(User toRegister);
    }
}