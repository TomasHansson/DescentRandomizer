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
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class ClassItemRepository : IClassItemRepository
    {
        private readonly TableStorageOptions _options;

        public ClassItemRepository(IOptions<TableStorageOptions> options)
        {
            _options = options.Value;
        }

        private CloudTable GetCloudTable()
        {
            // PartitionKey: ClassId.ToString()
            // RowKey: Id.ToString()
            var connectionString = _options.ConnectionString;
            var tableName = _options.ClassItemsTable;
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            CloudTable table = tableClient.GetTableReference(tableName);
            return table;
        }

        public async Task<Result<List<ClassItem>>> GetAll()
        {
            try
            {
                var table = GetCloudTable();
                var classItems = new List<ClassItem>(0);
                var query = new TableQuery<ClassItemTableEntity>();
                TableContinuationToken continuationToken = default;
                do
                {
                    var queryResult = await table.ExecuteQuerySegmentedAsync(query, continuationToken);
                    classItems.Capacity += queryResult.Results.Count;
                    foreach (var queryItem in queryResult)
                    {
                        classItems.Add(queryItem.ConvertToClassItem());
                    }
                    continuationToken = queryResult.ContinuationToken;
                } while (continuationToken != null);
                return new Result<List<ClassItem>>(classItems, true);
            }
            catch (Exception e)
            {
                return new Result<List<ClassItem>>(default, false, e.Message);
            }
        }

        public Result<ClassItem> Get(Guid id)
        {
            try
            {
                var table = GetCloudTable();
                var filter = TableQuery.GenerateFilterCondition(nameof(ClassItemTableEntity.RowKey), QueryComparisons.Equal, id.ToString());
                var query = new TableQuery<ClassItemTableEntity>().Where(filter);
                var classItems = table.ExecuteQuery(query).ToList();
                if (classItems is null || classItems.Count != 1)
                {
                    return new Result<ClassItem>(default, false, errorType: ErrorType.NotFound);
                }

                return new Result<ClassItem>(classItems.First().ConvertToClassItem(), true);
            }
            catch (Exception e)
            {
                return new Result<ClassItem>(default, false, e.Message);
            }
        }

        public async Task<Result<ClassItem>> Create(ClassItem classItem)
        {
            try
            {
                var table = GetCloudTable();
                var insertOperation = TableOperation.InsertOrMerge(new ClassItemTableEntity(classItem));
                var result = await table.ExecuteAsync(insertOperation);
                if (result != null && result.Result is ClassItemTableEntity classCardTableEntity)
                {
                    return new Result<ClassItem>(classCardTableEntity.ConvertToClassItem(), true);
                }

                return new Result<ClassItem>(default, false, "Failed to create class item.");
            }
            catch (Exception e)
            {
                return new Result<ClassItem>(default, false, e.Message);
            }
        }

        public async Task<Result<ClassItem>> Update(ClassItem classItem)
        {
            try
            {
                var classItemResult = Get(classItem.Id);
                if (classItemResult.Success == false || classItemResult.ResultObject is null)
                {
                    return new Result<ClassItem>(default, false, errorType: ErrorType.NotFound);
                }

                if (classItem.ClassId != classItemResult.ResultObject.ClassId)
                {
                    // PartitionKey will be changed, delete the old one to not end up with two entries.
                    var deleteResult = await Delete(classItem.Id);
                    if (deleteResult.Success == false)
                    {
                        return new Result<ClassItem>(default, false, "Failed to update class item.");
                    }
                }

                var table = GetCloudTable();
                var mergeOperation = TableOperation.InsertOrMerge(new ClassItemTableEntity(classItem));
                var result = await table.ExecuteAsync(mergeOperation);
                if (result != null && result.Result is ClassItemTableEntity classCardTableEntity)
                {
                    return new Result<ClassItem>(classCardTableEntity.ConvertToClassItem(), true);
                }

                return new Result<ClassItem>(default, false, "Failed to update class item.");
            }
            catch (Exception e)
            {
                return new Result<ClassItem>(default, false, e.Message);
            }
        }

        public async Task<Result<bool>> Delete(Guid id)
        {
            try
            {
                var classItemResult = Get(id);
                if (classItemResult.Success == false || classItemResult.ResultObject is null)
                {
                    return new Result<bool>(false, false, errorType: ErrorType.NotFound);
                }

                var table = GetCloudTable();
                var deleteOperation = TableOperation.Delete(new ClassItemTableEntity(classItemResult.ResultObject) { ETag = "*" });
                await table.ExecuteAsync(deleteOperation);
                return new Result<bool>(true, true);
            }
            catch (Exception e)
            {
                return new Result<bool>(false, false, e.Message);
            }
        }

        public Result<bool> IsClassItemNameTaken(string name, Guid? id, Guid classId)
        {
            try
            {
                var table = GetCloudTable();
                if (id != null)
                {
                    var partitionFilter = TableQuery.GenerateFilterCondition(nameof(ClassItemTableEntity.PartitionKey), QueryComparisons.Equal, classId.ToString());
                    var rowKeyFilter = TableQuery.GenerateFilterCondition(nameof(ClassItemTableEntity.RowKey), QueryComparisons.NotEqual, id.ToString());
                    var combinedFilter = TableQuery.CombineFilters(partitionFilter, TableOperators.And, rowKeyFilter);
                    var nameFilter = TableQuery.GenerateFilterCondition(nameof(ClassItemTableEntity.RowKey), QueryComparisons.Equal, name);
                    var finalFilter = TableQuery.CombineFilters(combinedFilter, TableOperators.And, nameFilter);
                    var checkNameTakenByOtherItemForClass = new TableQuery<ClassItemTableEntity>().Where(finalFilter);
                    var nameTakenByOtherItemForClassResult = table.ExecuteQuery(checkNameTakenByOtherItemForClass).ToList();
                    var nameTakenByOtherItemForClass = nameTakenByOtherItemForClassResult != null && nameTakenByOtherItemForClassResult.Any();
                    return new Result<bool>(nameTakenByOtherItemForClass, true);
                }

                var partitionKeyFilter = TableQuery.GenerateFilterCondition(nameof(ClassItemTableEntity.PartitionKey), QueryComparisons.Equal, classId.ToString());
                var nameColumnFilter = TableQuery.GenerateFilterCondition(nameof(ClassItemTableEntity.Name), QueryComparisons.Equal, name);
                var filter = TableQuery.CombineFilters(partitionKeyFilter, TableOperators.And, nameColumnFilter);
                var nameTakenQuery = new TableQuery<ClassItemTableEntity>().Where(filter);
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
