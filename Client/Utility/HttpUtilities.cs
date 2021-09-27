using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
            }

            return new Result<bool>(false, false, "Failed to read boolean response.");
        }
    }
}
