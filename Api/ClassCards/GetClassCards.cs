using System.Net;
using System.Threading.Tasks;
using Api.Utility;
using DataAccess.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace API.ClassCards
{
    public class GetClassCards
    {
        private readonly IClassCardRepository _repository;

        public GetClassCards(IClassCardRepository repository)
        {
            _repository = repository;
        }

        [FunctionName("GetClassCards")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ClassCards")] HttpRequest req)
        {
            var repositoryResponse = await _repository.GetAll();
            return repositoryResponse.Success
                ? HttpUtilities.CreateResponseFromSuccesfulResult(HttpStatusCode.OK, repositoryResponse.ResultObject)
                : repositoryResponse.CreateResponseFromFailedResult(); 
        }
    }
}
