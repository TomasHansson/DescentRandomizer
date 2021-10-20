using BlazorApp.Client.Utility;
using Domain.DataTransferObjects;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlazorApp.Client.Services
{
    public class BasicAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _httpClient;
        private User _user;

        public BasicAuthenticationStateProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> Authorize(string username, string password)
        {
            var url = $"api/Users/Authorize/{username}/{password}";
            var response = await _httpClient.GetAsync(url);
            var result = await HttpUtilities.TryParseJsonResponse<User>(response);
            if (result.Success)
            {
                _user = result.ResultObject;
                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
                return true;
            }

            return false;
        }

        public void Logout()
        {
            _user = null;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (_user is null)
            {
                return Task.FromResult(new AuthenticationState(new ClaimsPrincipal()));
            }

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, _user.Username)
            }, "Authorized");
            var user = new ClaimsPrincipal(identity);
            return Task.FromResult(new AuthenticationState(user));
        }
    }
}
