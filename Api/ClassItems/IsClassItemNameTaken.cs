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
    public class IsClassItemNameTaken
    {
        private readonly IClassItemRepository _repository;

        public IsClassItemNameTaken(IClassItemRepository repository)
        {
            _repository = repository;
        }

        [FunctionName("IsClassItemNameTaken")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ClassItems/Name/{name}/{classId}/{id?}")] HttpRequest req,
            string name, string classId, string id)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return HttpUtilities.CreateResponseWithMessage(HttpStatusCode.BadRequest, "Name cannot be null or empty.");
            }

            var repositoryResponse = _repository.IsClassItemNameTaken(name, string.IsNullOrWhiteSpace(id) ? (Guid?)null : new Guid(id), new Guid(classId));
            return repositoryResponse.Success
                ? HttpUtilities.CreateResponseWithMessage(HttpStatusCode.OK, repositoryResponse.ResultObject.ToString())
                : repositoryResponse.CreateResponseFromFailedResult();
        }
    }
}
