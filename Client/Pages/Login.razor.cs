using BlazorApp.Client.Services;
using Domain.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Radzen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorApp.Client.Pages
{
    public partial class Login
    {
        [Inject]
        public AuthenticationStateProvider AuthenticationStateProvider { get; set; }
        [Inject]
        public NotificationService NotificationService { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        private readonly Authorization _authorization = new ();

        public async Task Authorize()
        {
            var success = await (AuthenticationStateProvider as BasicAuthenticationStateProvider).Authorize(_authorization.Username, _authorization.Password);
            if (success == false)
            {
                NotificationService.Notify(NotificationSeverity.Warning, "Invalid Username and/or Password.");
                return;
            }

            NotificationService.Notify(summary: "Successfully logged in.");
            NavigationManager.NavigateTo("/");
        }
    }
}
