using DataAccess.DataAccessObjects;
using DataAccess.Options;
using DataAccess.Repositories.Interfaces;
using Domain.DataTransferObjects;
using Domain.Enums;
using Domain.Models;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class HeroesRepository : IHeroesRepository
    {
        private readonly TableStorageOptions _options;

        public HeroesRepository(IOptions<TableStorageOptions> options)
        {
            _options = options.Value;
        }

        private CloudTable GetCloudTable()
        {
            // PartitionKey: Id.ToString()
            // RowKey: Name
            var connectionString = _options.ConnectionString;
            var tableName = _options.HeroesTable;
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            CloudTable table = tableClient.GetTableReference(tableName);
            return table;
        }

        public async Task<Result<List<Hero>>> GetAll()
        {
            try
            {
                var table = GetCloudTable();
                var heroes = new List<Hero>(0);
                var query = new TableQuery<HeroTableEntity>();
                TableContinuationToken continuationToken = default;
                do
                {
                    var queryResult = await table.ExecuteQuerySegmentedAsync(query, continuationToken);
                    heroes.Capacity += queryResult.Results.Count;
                    foreach (var queryItem in queryResult)
                    {
                        heroes.Add(queryItem.ConvertToHero());
                    }
                    continuationToken = queryResult.ContinuationToken;
                } while (continuationToken != null);
                return new Result<List<Hero>>(heroes, true);
            }
            catch (Exception e)
            {
                return new Result<List<Hero>>(default, false, e.Message);
            }
        }

        public Result<Hero> Get(Guid id)
        {
            try
            {
                var table = GetCloudTable();
                var filter = TableQuery.GenerateFilterCondition(nameof(HeroTableEntity.PartitionKey), QueryComparisons.Equal, id.ToString());
                var query = new TableQuery<HeroTableEntity>().Where(filter);
                var heroes = table.ExecuteQuery(query).ToList();
                if (heroes is null || heroes.Count != 1)
                {
                    return new Result<Hero>(default, false, errorType: ErrorType.NotFound);
                }

                return new Result<Hero>(heroes.First().ConvertToHero(), true);
            }
            catch (Exception e)
            {
                return new Result<Hero>(default, false, e.Message);
            }
        }

        public async Task<Result<Hero>> Create(Hero hero)
        {
            try
            {
                var table = GetCloudTable();
                var insertOperation = TableOperation.InsertOrMerge(new HeroTableEntity(hero));
                var result = await table.ExecuteAsync(insertOperation);
                if (result != null && result.Result is HeroTableEntity heroTableEntity)
                {
                    return new Result<Hero>(heroTableEntity.ConvertToHero(), true);
                }

                return new Result<Hero>(default, false, "Failed to create hero.");
            }
            catch (Exception e)
            {
                return new Result<Hero>(default, false, e.Message);
            }
        }

        public async Task<Result<Hero>> Update (Hero hero)
        {
            try
            {
                var heroResult = Get(hero.Id);
                if (heroResult.Success == false || heroResult.ResultObject is null)
                {
                    return new Result<Hero>(default, false, errorType: ErrorType.NotFound);
                }

                if (hero.Name != heroResult.ResultObject.Name)
                {
                    // RowKey will be changed, delete the old one to not end up with two entries.
                    var deleteResult = await Delete(hero.Id);
                    if (deleteResult.Success == false)
                    {
                        return new Result<Hero>(default, false, "Failed to update hero.");
                    }
                }

                var table = GetCloudTable();
                var mergeOperation = TableOperation.InsertOrMerge(new HeroTableEntity(hero));
                var result = await table.ExecuteAsync(mergeOperation);
                if (result != null && result.Result is HeroTableEntity heroTableEntity)
                {
                    return new Result<Hero>(heroTableEntity.ConvertToHero(), true);
                }

                return new Result<Hero>(default, false, "Failed to update hero.");
            }
            catch (Exception e)
            {
                return new Result<Hero>(default, false, e.Message);
            }
        }

        public async Task<Result<bool>> Delete(Guid id)
        {
            try
            {
                var heroResult = Get(id);
                if (heroResult.Success == false || heroResult.ResultObject is null)
                {
                    return new Result<bool>(false, false, errorType: ErrorType.NotFound);
                }

                var table = GetCloudTable();
                var deleteOperation = TableOperation.Delete(new HeroTableEntity(heroResult.ResultObject) { ETag = "*" });
                await table.ExecuteAsync(deleteOperation);
                return new Result<bool>(true, true);
            }
            catch (Exception e)
            {
                return new Result<bool>(false, false, e.Message);
            }
        }

        public Result<bool> IsHeroNameTaken(string name, Guid? id)
        {
            try
            {
                var table = GetCloudTable();
                if (id != null)
                {
                    var partitionFilter = TableQuery.GenerateFilterCondition(nameof(HeroTableEntity.PartitionKey), QueryComparisons.NotEqual, id.ToString());
                    var rowKeyFilter = TableQuery.GenerateFilterCondition(nameof(HeroTableEntity.RowKey), QueryComparisons.Equal, name);
                    var combinedFilter = TableQuery.CombineFilters(partitionFilter, TableOperators.And, rowKeyFilter);
                    var checkNameTakenByOtherHeroQuery = new TableQuery<HeroTableEntity>().Where(combinedFilter);
                    var nameTakenByOtherHeroResult = table.ExecuteQuery(checkNameTakenByOtherHeroQuery).ToList();
                    var nameTakenByOtherHero = nameTakenByOtherHeroResult != null && nameTakenByOtherHeroResult.Any();
                    return new Result<bool>(nameTakenByOtherHero, true);
                }

                var filter = TableQuery.GenerateFilterCondition(nameof(HeroTableEntity.RowKey), QueryComparisons.Equal, name);
                var nameTakenQuery = new TableQuery<HeroTableEntity>().Where(filter);
                var nameTakenResult = table.ExecuteQuery(nameTakenQuery).ToList();
                var nameIsTaken = nameTakenResult != null && nameTakenResult.Any();
                return new Result<bool>(nameIsTaken, true);
            }
            catch (Exception e)
            {
                return new Result<bool>(false, false, e.Message);
            }
        }
    }
}
