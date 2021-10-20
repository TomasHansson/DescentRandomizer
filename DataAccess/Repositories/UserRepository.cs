using DataAccess.DataAccessObjects;
using DataAccess.Options;
using DataAccess.Repositories.Interfaces;
using Domain.DataTransferObjects;
using Domain.Enums;
using Domain.Models;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Options;
using System;
using System.Linq;

namespace DataAccess.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly TableStorageOptions _options;

        public UserRepository(IOptions<TableStorageOptions> options)
        {
            _options = options.Value;
        }

        private CloudTable GetCloudTable()
        {
            // PartitionKey: First Name
            // RowKey: Last Name
            var connectionString = _options.ConnectionString;
            var tableName = _options.UsersTable;
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            CloudTable table = tableClient.GetTableReference(tableName);
            return table;
        }

        public Result<User> TryAuthorize(string username, string password)
        {
            try
            {
                var table = GetCloudTable();
                var usernameFilter = TableQuery.GenerateFilterCondition(nameof(UserTableEntity.Username), QueryComparisons.Equal, username);
                var passwordFilter = TableQuery.GenerateFilterCondition(nameof(UserTableEntity.Password), QueryComparisons.Equal, password);
                var filter = TableQuery.CombineFilters(usernameFilter, TableOperators.And, passwordFilter);
                var query = new TableQuery<UserTableEntity>().Where(filter);
                var users = table.ExecuteQuery(query).ToList();
                if (users is null || users.Count != 1)
                {
                    return new Result<User>(default, false, errorType: ErrorType.NotFound);
                }

                return new Result<User>(users.First().ConvertToUser(), true);
            }
            catch (Exception e)
            {
                return new Result<User>(default, false, e.Message);
            }
        }
    }
}
