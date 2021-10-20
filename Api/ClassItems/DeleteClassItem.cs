using System;
using System.Threading.Tasks;
using Api.Utility;
using DataAccess.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace API.ClassCards
{
    public class DeleteClassItem
    {
        private readonly IClassItemRepository _repository;

        public DeleteClassItem(IClassItemRepository repository)
        {
            _repository = repository;
        }

        [FunctionName("DeleteClassItem")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "ClassItems/{id}")] HttpRequest req,
            string id)
        {
            var repositoryResult = await _repository.Delete(new Guid(id));
            return repositoryResult.Success && repositoryResult.ResultObject
                ? new OkResult()
                : HttpUtilities.CreateResponseFromFailedResult(repositoryResult);
        }
    }
}
