using System;
using System.Net;
using Api.Utility;
using DataAccess.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace API.ClassCards
{
    public class GetClassCard
    {
        private readonly IClassCardRepository _repository;

        public GetClassCard(IClassCardRepository repository)
        {
            _repository = repository;
        }

        [FunctionName("GetClassCard")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ClassCards/{id}")] HttpRequest req, 
            string id)
        {
            var repositoryResponse = _repository.Get(new Guid(id));
            return repositoryResponse.Success
                ? HttpUtilities.CreateResponseFromSuccesfulResult(HttpStatusCode.OK, repositoryResponse.ResultObject)
                : repositoryResponse.CreateResponseFromFailedResult();
        }
    }
}
