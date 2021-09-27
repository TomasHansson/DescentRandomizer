using System;
using System.Net;
using Api.Utility;
using DataAccess.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace API.Heroes
{
    public class IsHeroNameTaken
    {
        private readonly IHeroesRepository _repository;

        public IsHeroNameTaken(IHeroesRepository repository)
        {
            _repository = repository;
        }

        [FunctionName("IsHeroNameTaken")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Heroes/Name/{name}/{id?}")] HttpRequest req,
            string name, string id)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return HttpUtilities.CreateResponseWithMessage(HttpStatusCode.BadRequest, "Name cannot be null or empty.");
            }

            var repositoryResponse = _repository.IsHeroNameTaken(name, string.IsNullOrWhiteSpace(id) ? (Guid?)null : new Guid(id));
            return repositoryResponse.Success
                ? HttpUtilities.CreateResponseWithMessage(HttpStatusCode.OK, repositoryResponse.ResultObject.ToString())
                : repositoryResponse.CreateResponseFromFailedResult();
        }
    }
}
