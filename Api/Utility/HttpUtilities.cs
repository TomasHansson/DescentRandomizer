using Domain.Enums;
using Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Api.Utility
{
    public static class HttpUtilities
    {
        public static async Task<Result<T>> TryDeserializeBody<T>(this HttpRequest req)
        {
            try
            {
                using StreamReader streamReader = new StreamReader(req.Body);
                string body = await streamReader.ReadToEndAsync();
                T model = JsonSerializer.Deserialize<T>(body);
                return new Result<T>(model, true);
            }
            catch (Exception)
            {
                return new Result<T>(default, false, "Failed to deserialize request body.", ErrorType.InvalidInput);
            }
        }

        public static ObjectResult CreateResponseWithMessage(HttpStatusCode statusCode, string errorMessage)
        {
            return new ObjectResult(errorMessage) { StatusCode = (int)statusCode };
        }

        public static ObjectResult CreateResponseFromSuccesfulResult<T>(HttpStatusCode statusCode, T resultObject)
        {
            return new ObjectResult(resultObject) { StatusCode = (int)statusCode };
        }

        public static ActionResult CreateResponseFromFailedResult<T>(this Result<T> result)
        {
            HttpStatusCode statusCode = result.ErrorType switch
            {
                ErrorType.Exception => HttpStatusCode.InternalServerError,
                ErrorType.InvalidInput => HttpStatusCode.BadRequest,
                ErrorType.NotFound => HttpStatusCode.NotFound,
                _ => HttpStatusCode.InternalServerError
            };

            if (string.IsNullOrWhiteSpace(result.ErrorMessage))
            {
                return new StatusCodeResult((int)statusCode);
            }

            return new ObjectResult(result.ErrorMessage) { StatusCode = (int)statusCode };
        }
    }
}
