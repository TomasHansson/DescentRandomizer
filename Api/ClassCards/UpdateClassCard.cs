using System.Net;
using System.Threading.Tasks;
using Api.Utility;
using DataAccess.Repositories.Interfaces;
using Domain.DataTransferObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace API.ClassCards
{
    public class UpdateClassCard
    {
        private readonly IClassCardRepository _repository;

        public UpdateClassCard(IClassCardRepository repository)
        {
            _repository = repository;
        }

        [FunctionName("UpdateClassCard")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "ClassCards")] HttpRequest req)
        {
            var deserializeResult = await req.TryDeserializeBody<ClassCard>();
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
