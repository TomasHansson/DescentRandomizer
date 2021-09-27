using System.Net;
using System.Threading.Tasks;
using Api.Utility;
using DataAccess.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace API.Classes
{
    public class GetClasses
    {
        private readonly IClassRepository _repository;

        public GetClasses(IClassRepository repository)
        {
            _repository = repository;
        }

        [FunctionName("GetClasses")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Classes/All/{includeClassCards:bool}")] HttpRequest req, 
            bool includeClassCards)
        {
            var repositoryResponse = await _repository.GetAll(includeClassCards);
            return repositoryResponse.Success
                ? HttpUtilities.CreateResponseFromSuccesfulResult(HttpStatusCode.OK, repositoryResponse.ResultObject)
                : repositoryResponse.CreateResponseFromFailedResult(); 
        }
    }
}
