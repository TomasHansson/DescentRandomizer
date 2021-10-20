using BlazorApp.Client.Shared;
using BlazorApp.Client.Utility;
using Domain.DataTransferObjects;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Radzen;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace BlazorApp.Client.Pages.Heroes
{
    public partial class Heroes
    {
        [Inject]
        public HttpClient HttpClient { get; set; }
        [Inject]
        public DialogService DialogService { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }
        [Inject]
        public NotificationService NotificationService { get; set; }
        [Inject]
        public TooltipService TooltipService { get; set; }

        private IEnumerable<Hero> _heroes;

        protected override async Task OnInitializedAsync()
        {
            _heroes = await GetHeroes();
        }

        private async Task<List<Hero>> GetHeroes()
        {
            var url = "api/Heroes";
            var response = await HttpClient.GetAsync(url);
            var result = await HttpUtilities.TryParseJsonResponse<List<Hero>>(response);
            if (result.Success)
            {
                return result.ResultObject;
            }
            else
            {
                NotificationService.Notify(NotificationSeverity.Error, "Unable to load heroes.", result.ErrorMessage);
                return new List<Hero>();
            }
        }

        private void GoToCreateHero()
        {
            NavigationManager.NavigateTo("/Heroes/Create");
        }

        private async Task ShowHeroDetails(Hero hero)
        {
            await DialogService.OpenAsync<HeroDetails>("Hero Details", new Dictionary<string, object>() { { "Hero", hero }, { "DialogService", DialogService } });
        }

        private void GoToEdit(Hero hero)
        {
            NavigationManager.NavigateTo($"/Heroes/{hero.Id}/Edit");
        }

        private async Task DeleteHero(Hero hero)
        {
            var result = await DialogService.Confirm($"Are you sure you want to delete the hero {hero.Name}?");
            if (result == true)
            {
                var url = $"api/Heroes/{hero.Id}";
                var deleteHeroResponse = await HttpClient.DeleteAsync(url);
                if (deleteHeroResponse.IsSuccessStatusCode)
                {
                    NotificationService.Notify(NotificationSeverity.Info, "The hero was deleted.");
                    _heroes = await GetHeroes();
                    StateHasChanged();
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error, "An error occured while trying to delete the hero.");
                }
            }
        }

        private void ShowTooltip(ElementReference elementReference, string text)
        {
            TooltipService.Open(elementReference, text, new TooltipOptions { Position = TooltipPosition.Left });
        }
    }
}
