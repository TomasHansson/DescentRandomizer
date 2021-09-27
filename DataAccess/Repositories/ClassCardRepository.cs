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
    public class ClassCardRepository : IClassCardRepository
    {
        private readonly TableStorageOptions _options;

        public ClassCardRepository(IOptions<TableStorageOptions> options)
        {
            _options = options.Value;
        }

        private CloudTable GetCloudTable()
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

        public async Task<Result<List<ClassCard>>> GetAll()
        {
            try
            {
                var table = GetCloudTable();
                var classCards = new List<ClassCard>(0);
                var query = new TableQuery<ClassCardTableEntity>();
                TableContinuationToken continuationToken = default;
                do
                {
                    var queryResult = await table.ExecuteQuerySegmentedAsync(query, continuationToken);
                    classCards.Capacity += queryResult.Results.Count;
                    foreach (var queryItem in queryResult)
                    {
                        classCards.Add(queryItem.ConvertToClassCard());
                    }
                    continuationToken = queryResult.ContinuationToken;
                } while (continuationToken != null);
                return new Result<List<ClassCard>>(classCards, true);
            }
            catch (Exception e)
            {
                return new Result<List<ClassCard>>(default, false, e.Message);
            }
        }

        public Result<ClassCard> Get(Guid id)
        {
            try
            {
                var table = GetCloudTable();
                var filter = TableQuery.GenerateFilterCondition(nameof(ClassCardTableEntity.RowKey), QueryComparisons.Equal, id.ToString());
                var query = new TableQuery<ClassCardTableEntity>().Where(filter);
                var classCards = table.ExecuteQuery(query).ToList();
                if (classCards is null || classCards.Count != 1)
                {
                    return new Result<ClassCard>(default, false, errorType: ErrorType.NotFound);
                }

                return new Result<ClassCard>(classCards.First().ConvertToClassCard(), true);
            }
            catch (Exception e)
            {
                return new Result<ClassCard>(default, false, e.Message);
            }
        }

        public async Task<Result<ClassCard>> Create(ClassCard classCard)
        {
            try
            {
                var table = GetCloudTable();
                var insertOperation = TableOperation.InsertOrMerge(new ClassCardTableEntity(classCard));
                var result = await table.ExecuteAsync(insertOperation);
                if (result != null && result.Result is ClassCardTableEntity classCardTableEntity)
                {
                    return new Result<ClassCard>(classCardTableEntity.ConvertToClassCard(), true);
                }

                return new Result<ClassCard>(default, false, "Failed to create class card.");
            }
            catch (Exception e)
            {
                return new Result<ClassCard>(default, false, e.Message);
            }
        }

        public async Task<Result<ClassCard>> Update(ClassCard classCard)
        {
            try
            {
                var classCardResult = Get(classCard.Id);
                if (classCardResult.Success == false || classCardResult.ResultObject is null)
                {
                    return new Result<ClassCard>(default, false, errorType: ErrorType.NotFound);
                }

                if (classCard.ClassId != classCardResult.ResultObject.ClassId)
                {
                    // PartitionKey will be changed, delete the old one to not end up with two entries.
                    var deleteResult = await Delete(classCard.Id);
                    if (deleteResult.Success == false)
                    {
                        return new Result<ClassCard>(default, false, "Failed to update class card.");
                    }
                }

                var table = GetCloudTable();
                var mergeOperation = TableOperation.InsertOrMerge(new ClassCardTableEntity(classCard));
                var result = await table.ExecuteAsync(mergeOperation);
                if (result != null && result.Result is ClassCardTableEntity classCardTableEntity)
                {
                    return new Result<ClassCard>(classCardTableEntity.ConvertToClassCard(), true);
                }

                return new Result<ClassCard>(default, false, "Failed to update class card.");
            }
            catch (Exception e)
            {
                return new Result<ClassCard>(default, false, e.Message);
            }
        }

        public async Task<Result<bool>> Delete(Guid id)
        {
            try
            {
                var classCardResult = Get(id);
                if (classCardResult.Success == false || classCardResult.ResultObject is null)
                {
                    return new Result<bool>(false, false, errorType: ErrorType.NotFound);
                }

                var table = GetCloudTable();
                var deleteOperation = TableOperation.Delete(new ClassCardTableEntity(classCardResult.ResultObject) { ETag = "*" });
                await table.ExecuteAsync(deleteOperation);
                return new Result<bool>(true, true);
            }
            catch (Exception e)
            {
                return new Result<bool>(false, false, e.Message);
            }
        }

        public Result<bool> IsClassCardNameTaken(string name, Guid? id, Guid classId)
        {
            try
            {
                var table = GetCloudTable();
                if (id != null)
                {
                    var partitionFilter = TableQuery.GenerateFilterCondition(nameof(ClassCardTableEntity.PartitionKey), QueryComparisons.Equal, classId.ToString());
                    var rowKeyFilter = TableQuery.GenerateFilterCondition(nameof(ClassCardTableEntity.RowKey), QueryComparisons.NotEqual, id.ToString());
                    var combinedFilter = TableQuery.CombineFilters(partitionFilter, TableOperators.And, rowKeyFilter);
                    var nameFilter = TableQuery.GenerateFilterCondition(nameof(ClassCardTableEntity.RowKey), QueryComparisons.Equal, name);
                    var finalFilter = TableQuery.CombineFilters(combinedFilter, TableOperators.And, nameFilter);
                    var checkNameTakenByOtherCardForClass = new TableQuery<ClassCardTableEntity>().Where(finalFilter);
                    var nameTakenByOtherCardForClassResult = table.ExecuteQuery(checkNameTakenByOtherCardForClass).ToList();
                    var nameTakenByOtherCardForClass = nameTakenByOtherCardForClassResult != null && nameTakenByOtherCardForClassResult.Any();
                    return new Result<bool>(nameTakenByOtherCardForClass, true);
                }

                var partitionKeyFilter = TableQuery.GenerateFilterCondition(nameof(ClassCardTableEntity.PartitionKey), QueryComparisons.Equal, classId.ToString());
                var nameColumnFilter = TableQuery.GenerateFilterCondition(nameof(ClassCardTableEntity.Name), QueryComparisons.Equal, name);
                var filter = TableQuery.CombineFilters(partitionKeyFilter, TableOperators.And, nameColumnFilter);
                var nameTakenQuery = new TableQuery<ClassCardTableEntity>().Where(filter);
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
