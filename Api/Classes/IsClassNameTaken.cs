using System;
using System.Net;
using Api.Utility;
using DataAccess.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace API.Classes
{
    public class IsClassNameTaken
    {
        private readonly IClassRepository _repository;

        public IsClassNameTaken(IClassRepository repository)
        {
            _repository = repository;
        }

        [FunctionName("IsClassNameTaken")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Classes/Name/{name}/{id?}")] HttpRequest req,
            string name, string id)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return HttpUtilities.CreateResponseWithMessage(HttpStatusCode.BadRequest, "Name cannot be null or empty.");
            }

            var repositoryResponse = _repository.IsClassNameTaken(name, string.IsNullOrWhiteSpace(id) ? (Guid?)null : new Guid(id));
            return repositoryResponse.Success
                ? HttpUtilities.CreateResponseWithMessage(HttpStatusCode.OK, repositoryResponse.ResultObject.ToString())
                : repositoryResponse.CreateResponseFromFailedResult();
        }
    }
}
