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
    public class UpdateClass
    {
        private readonly IClassRepository _repository;

        public UpdateClass(IClassRepository repository)
        {
            _repository = repository;
        }

        [FunctionName("UpdateClass")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "Classes")] HttpRequest req)
        {
            var deserializeResult = await req.TryDeserializeBody<Class>();
            if (deserializeResult.Success == false || deserializeResult.ResultObject is null)
            {
                return deserializeResult.CreateResponseFromFailedResult();
            }

            var repositoryResponse = await _repository.Update(deserializeResult.ResultObject);
            return repositoryResponse.Success
                ? HttpUtilities.CreateResponseFromSuccesfulResult(HttpStatusCode.OK, repositoryResponse.ResultObject)
                : repositoryResponse.CreateResponseFromFailedResult(); 
        }
    }
}
