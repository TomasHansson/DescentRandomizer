using BlazorApp.Client.Utility;
using Domain.DataTransferObjects;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Radzen;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace BlazorApp.Client.Pages.Heroes
{
    public partial class CreateHero
    {
        [Inject]
        public HttpClient HttpClient { get; set; }
        [Inject]
        public NotificationService NotificationService { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        private readonly Hero _hero = new() { Id = Guid.NewGuid() };

        private async Task Submit()
        {
            if (_hero != null)
            {
                var url = $"api/Heroes/Name/{_hero.Name}";
                var checkNameResponse = await HttpClient.GetAsync(url);
                var checkNameResult = await HttpUtilities.TryReadBooleanResponse(checkNameResponse);
                var errorOccured = false;
                if (checkNameResult.Success && !checkNameResult.ResultObject)
                {
                    url = "api/Heroes";
                    var createHeroResponse = await HttpClient.PostAsJsonAsync(url, _hero);
                    if (createHeroResponse.IsSuccessStatusCode)
                    {
                        NotificationService.Notify(NotificationSeverity.Success, "Hero was created succesfully..");
                        NavigationManager.NavigateTo("/Heroes");
                    }
                    else
                    {
                        errorOccured = true;
                    }
                }
                else if (checkNameResult.Success && checkNameResult.ResultObject)
                {
                    NotificationService.Notify(NotificationSeverity.Warning, "A hero with that name already exists.");
                }
                else
                {
                    errorOccured = true;
                }

                if (errorOccured)
                {
                    NotificationService.Notify(NotificationSeverity.Error, "An error occured while trying to create the new hero.");
                }
            }
        }
    }
}
