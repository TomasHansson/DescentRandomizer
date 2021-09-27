using System.Net;
using System.Threading.Tasks;
using Api.Utility;
using DataAccess.Repositories.Interfaces;
using Domain.DataTransferObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace API.Heroes
{
    public class UpdateHero
    {
        private readonly IHeroesRepository _repository;

        public UpdateHero(IHeroesRepository repository)
        {
            _repository = repository;
        }

        [FunctionName("UpdateHero")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "Heroes")] HttpRequest req)
        {
            var deserializeResult = await req.TryDeserializeBody<Hero>();
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
