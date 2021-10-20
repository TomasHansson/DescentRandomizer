using BlazorApp.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Radzen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorApp.Client.Pages
{
    public partial class Logout
    {
        [Inject]
        public AuthenticationStateProvider AuthenticationStateProvider { get; set; }
        [Inject]
        public NotificationService NotificationService { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        private void LogoutUser()
        {
            (AuthenticationStateProvider as BasicAuthenticationStateProvider).Logout();
            NotificationService.Notify(summary: "Successfully logged out.");
            NavigationManager.NavigateTo("/");
        }
    }
}
