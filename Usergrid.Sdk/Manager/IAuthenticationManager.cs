using System.Threading.Tasks;
using Usergrid.Sdk.Model;

namespace Usergrid.Sdk.Manager
{
    internal interface IAuthenticationManager
    {
        Task ChangePassword(string userName, string oldPassword, string newPassword);
        Task Login(string loginId, string secret, AuthType authType);
    }
}