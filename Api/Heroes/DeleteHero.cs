using System;
using System.Threading.Tasks;
using Api.Utility;
using DataAccess.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace API.Heroes
{
    public class DeleteHero
    {
        private readonly IHeroesRepository _repository;

        public DeleteHero(IHeroesRepository repository)
        {
            _repository = repository;
        }

        [FunctionName("DeleteHero")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "Heroes/{id}")] HttpRequest req,
            string id)
        {
            var repositoryResult = await _repository.Delete(new Guid(id));
            return repositoryResult.Success && repositoryResult.ResultObject
                ? new OkResult()
                : repositoryResult.CreateResponseFromFailedResult();
        }
    }
}
