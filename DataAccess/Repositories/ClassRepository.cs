using DataAccess.DataAccessObjects;
using DataAccess.Options;
using DataAccess.Repositories.Interfaces;
using Domain.DataTransferObjects;
using Domain.Enums;
using Domain.Models;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class ClassRepository : IClassRepository
    {
        private readonly TableStorageOptions _options;

        public ClassRepository(IOptions<TableStorageOptions> options)
        {
            _options = options.Value;
        }

        private CloudTable GetClassCloudTable()
        {
            // PartitionKey: Id.ToString()
            // RowKey: Name
            var connectionString = _options.ConnectionString;
            var tableName = _options.ClassesTable;
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            CloudTable table = tableClient.GetTableReference(tableName);
            return table;
        }

        private CloudTable GetClassCardCloudTable()
        {
            // PartitionKey: ClassId.ToString()
            // RowKey: Id.ToString()
            var connectionString = _options.ConnectionString;
            var tableName = _options.ClassCardsTable;
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            CloudTable table = tableClient.GetTableReference(tableName);
            return table;
        }

        public async Task<Result<List<Class>>> GetAll(bool includeClassCards = true)
        {
            try
            {
                var table = GetClassCloudTable();
                var classes = new List<Class>(0);
                var query = new TableQuery<ClassTableEntity>();
                TableContinuationToken continuationToken = default;
                do
                {
                    var queryResult = await table.ExecuteQuerySegmentedAsync(query, continuationToken);
                    classes.Capacity += queryResult.Results.Count;
                    foreach (var queryItem in queryResult)
                    {
                        classes.Add(queryItem.ConvertToClass());
                    }
                    continuationToken = queryResult.ContinuationToken;
                } while (continuationToken != null);
                if (includeClassCards)
                {
                    foreach (var currentClass in classes)
                    {
                        var cardsTable = GetClassCardCloudTable();
                        var filter = TableQuery.GenerateFilterCondition(nameof(ClassCardTableEntity.PartitionKey), QueryComparisons.Equal, currentClass.Id.ToString());
                        var cardsQuery = new TableQuery<ClassCardTableEntity>().Where(filter);
                        var classCards = cardsTable.ExecuteQuery(cardsQuery).ToList();
                        currentClass.ClassCards = classCards.Select(x => x.ConvertToClassCard()).ToList();
                    }
                }
                return new Result<List<Class>>(classes, true);
            }
            catch (Exception e)
            {
                return new Result<List<Class>>(default, false, e.Message);
            }
        }

        public Result<Class> Get(Guid id)
        {
            try
            {
                var table = GetClassCloudTable();
                var filter = TableQuery.GenerateFilterCondition(nameof(ClassTableEntity.PartitionKey), QueryComparisons.Equal, id.ToString());
                var query = new TableQuery<ClassTableEntity>().Where(filter);
                var classes = table.ExecuteQuery(query).ToList();
                if (classes is null || classes.Count != 1)
                {
                    return new Result<Class>(default, false, errorType: ErrorType.NotFound);
                }

                return new Result<Class>(classes.First().ConvertToClass(), true);
            }
            catch (Exception e)
            {
                return new Result<Class>(default, false, e.Message);
            }
        }

        public async Task<Result<Class>> Create(Class newClass)
        {
            try
            {
                var table = GetClassCloudTable();
                var insertOperation = TableOperation.InsertOrMerge(new ClassTableEntity(newClass));
                var result = await table.ExecuteAsync(insertOperation);
                if (result != null && result.Result is ClassTableEntity heroTableEntity)
                {
                    return new Result<Class>(heroTableEntity.ConvertToClass(), true);
                }

                return new Result<Class>(default, false, "Failed to create class.");
            }
            catch (Exception e)
            {
                return new Result<Class>(default, false, e.Message);
            }
        }

        public async Task<Result<Class>> Update(Class classToUpdate)
        {
            try
            {
                var classResult = Get(classToUpdate.Id);
                if (classResult.Success == false || classResult.ResultObject is null)
                {
                    return new Result<Class>(default, false, errorType: ErrorType.NotFound);
                }

                if (classToUpdate.Name != classResult.ResultObject.Name)
                {
                    // RowKey will be changed, delete the old one to not end up with two entries.
                    var deleteResult = await Delete(classToUpdate.Id);
                    if (deleteResult.Success == false)
                    {
                        return new Result<Class>(default, false, "Failed to update class.");
                    }
                }

                var table = GetClassCloudTable();
                var mergeOperation = TableOperation.InsertOrMerge(new ClassTableEntity(classToUpdate));
                var result = await table.ExecuteAsync(mergeOperation);
                if (result != null && result.Result is ClassTableEntity heroTableEntity)
                {
                    return new Result<Class>(heroTableEntity.ConvertToClass(), true);
                }

                return new Result<Class>(default, false, "Failed to update class.");
            }
            catch (Exception e)
            {
                return new Result<Class>(default, false, e.Message);
            }
        }

        public async Task<Result<bool>> Delete(Guid id)
        {
            try
            {
                var classResult = Get(id);
                if (classResult.Success == false || classResult.ResultObject is null)
                {
                    return new Result<bool>(false, false, errorType: ErrorType.NotFound);
                }

                var table = GetClassCloudTable();
                var deleteOperation = TableOperation.Delete(new ClassTableEntity(classResult.ResultObject) { ETag = "*" });
                await table.ExecuteAsync(deleteOperation);
                return new Result<bool>(true, true);
            }
            catch (Exception e)
            {
                return new Result<bool>(false, false, e.Message);
            }
        }

        public Result<bool> IsClassNameTaken(string name, Guid? id)
        {
            try
            {
                var table = GetClassCloudTable();
                if (id != null)
                {
                    var partitionFilter = TableQuery.GenerateFilterCondition(nameof(ClassTableEntity.PartitionKey), QueryComparisons.NotEqual, id.ToString());
                    var rowKeyFilter = TableQuery.GenerateFilterCondition(nameof(ClassTableEntity.RowKey), QueryComparisons.Equal, name);
                    var combinedFilter = TableQuery.CombineFilters(partitionFilter, TableOperators.And, rowKeyFilter);
                    var checkNameTakenByOtherClassQuery = new TableQuery<HeroTableEntity>().Where(combinedFilter);
                    var nameTakenByOtherClassResult = table.ExecuteQuery(checkNameTakenByOtherClassQuery).ToList();
                    var nameTakenByOtherClass = nameTakenByOtherClassResult != null && nameTakenByOtherClassResult.Any();
                    return new Result<bool>(nameTakenByOtherClass, true);
                }

                var filter = TableQuery.GenerateFilterCondition(nameof(ClassTableEntity.RowKey), QueryComparisons.Equal, name);
                var nameTakenQuery = new TableQuery<ClassTableEntity>().Where(filter);
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
