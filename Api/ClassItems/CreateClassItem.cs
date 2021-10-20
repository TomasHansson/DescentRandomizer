using System.Net;
using System.Threading.Tasks;
using Api.Utility;
using DataAccess.Repositories.Interfaces;
using Domain.DataTransferObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace Api.ClassCards
{
    public class CreateClassItem
    {
        private readonly IClassItemRepository _repository;

        public CreateClassItem(IClassItemRepository repository)
        {
            _repository = repository;
        }

        [FunctionName("CreateClassItem")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "ClassItems")] HttpRequest req)
        {
            var deserializeResult = await req.TryDeserializeBody<ClassItem>();
            if (deserializeResult.Success == false || deserializeResult.ResultObject is null)
            {
                return deserializeResult.CreateResponseFromFailedResult();
            }

            var repositoryResult = await _repository.Create(deserializeResult.ResultObject);
            return repositoryResult.Success
                ? HttpUtilities.CreateResponseFromSuccesfulResult(HttpStatusCode.Created, repositoryResult.ResultObject)
                : repositoryResult.CreateResponseFromFailedResult();
        }
    }
}
