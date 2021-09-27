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
    public class DeleteClassCard
    {
        private readonly IClassCardRepository _repository;

        public DeleteClassCard(IClassCardRepository repository)
        {
            _repository = repository;
        }

        [FunctionName("DeleteClassCard")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "ClassCards/{id}")] HttpRequest req,
            string id)
        {
            var repositoryResult = await _repository.Delete(new Guid(id));
            return repositoryResult.Success && repositoryResult.ResultObject
                ? new OkResult()
                : HttpUtilities.CreateResponseFromFailedResult(repositoryResult);
        }
    }
}
