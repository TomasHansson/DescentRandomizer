using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlazorApp.Client.Utility
{
    public static class HttpUtilities
    {
        public static async Task<Result<bool>> TryReadBooleanResponse(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                if (bool.TryParse(content, out bool result))
                {
                    return new Result<bool>(result, true);
                }

                return new Result<bool>(false, false, "Failed to read boolean response.");
            }

            return new Result<bool>(false, false, "Communication with the server failed.", Domain.Enums.ErrorType.NotSuccessStatusCode);
        }

        public static async Task<Result<T>> TryParseJsonResponse<T>(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };
                    T model = JsonSerializer.Deserialize<T>(content, options);
                    if (model != null)
                    {
                        return new Result<T>(model, true);
                    }

                    return new Result<T>(default, false, "Failed to parse response.", Domain.Enums.ErrorType.InvalidInput);
                }
                catch 
                {
                    return new Result<T>(default, false, "Failed to parse response.", Domain.Enums.ErrorType.InvalidInput);
                }
            }

            return new Result<T>(default, false, "Communication with the server failed.", Domain.Enums.ErrorType.NotSuccessStatusCode);
        }
    }
}
