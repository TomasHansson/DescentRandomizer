using Domain.DataTransferObjects;
using Microsoft.Azure.Cosmos.Table;

namespace DataAccess.DataAccessObjects
{
    public class UserTableEntity : TableEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public User ConvertToUser()
        {
            return new User
            {
                FirstName = FirstName,
                LastName = LastName,
                Username = Username
            };
        }
    }
}
