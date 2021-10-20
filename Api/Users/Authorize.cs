using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using DataAccess.Repositories.Interfaces;
using Api.Utility;
using System.Net;

namespace BlazorApp.Api.Users
{
    public class Authorize
    {
        private readonly IUserRepository _userRepository;

        public Authorize(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [FunctionName("Authorize")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Users/Authorize/{username}/{password}")] 
            HttpRequest req, string username, string password)
        {
            var repositoryResponse = _userRepository.TryAuthorize(username, password);
            return repositoryResponse.Success
                ? HttpUtilities.CreateResponseFromSuccesfulResult(HttpStatusCode.OK, repositoryResponse.ResultObject)
                : repositoryResponse.CreateResponseFromFailedResult();
        }
    }
}
