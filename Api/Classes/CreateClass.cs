using System.Net;
using System.Threading.Tasks;
using Api.Utility;
using DataAccess.Repositories.Interfaces;
using Domain.DataTransferObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace API.Classes
{
    public class CreateClassCard
    {
        private readonly IClassRepository _repository;

        public CreateClassCard(IClassRepository repository)
        {
            _repository = repository;
        }

        [FunctionName("CreateClass")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Classes")] HttpRequest req)
        {
            var deserializeResult = await req.TryDeserializeBody<Class>();
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
