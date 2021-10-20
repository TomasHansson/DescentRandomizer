using Domain.DataTransferObjects;
using Domain.Models;

namespace DataAccess.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Result<User> TryAuthorize(string username, string password);
    }
}