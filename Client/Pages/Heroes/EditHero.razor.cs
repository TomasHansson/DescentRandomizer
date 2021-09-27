using BlazorApp.Client.Utility;
using Domain.DataTransferObjects;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Radzen;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace BlazorApp.Client.Pages.Heroes
{
    public partial class EditHero
    {
        [Inject]
        public IConfiguration Configuration { get; set; }
        [Inject]
        public HttpClient HttpClient { get; set; }
        [Inject]
        public NotificationService NotificationService { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }
        [Parameter]
        public string Id { get; set; }

        private Hero _hero;
        private string _baseUrl;

        protected override async Task OnInitializedAsync()
        {
            _baseUrl = Configuration.GetConnectionString("API");
            await LoadHero(Id);
        }

        private async Task LoadHero(string id)
        {
            var url = $"api/Heroes/{id}";
            _hero = await HttpClient.GetFromJsonAsync<Hero>(url);
        }

        private async Task Submit()
        {
            var url = $"api/Heroes/Name/{_hero.Name}/{_hero.Id}";
            var checkNameResponse = await HttpClient.GetAsync(url);
            var checkNameResult = await HttpUtilities.TryReadBooleanResponse(checkNameResponse);
            var errorOccured = false;
            if (checkNameResult.Success && !checkNameResult.ResultObject)
            {
                url = $"api/Heroes";
                var editHeroResponse = await HttpClient.PutAsJsonAsync(url, _hero);
                if (editHeroResponse.IsSuccessStatusCode)
                {
                    NavigationManager.NavigateTo("/Heroes/Hero was edited succesfully.");
                }
                else
                {
                    errorOccured = true;
                }
            }
            else if (checkNameResult.Success && checkNameResult.ResultObject)
            {
                NotificationService.Notify(NotificationSeverity.Warning, "Another hero with that name already exists.");
            }
            else
            {
                errorOccured = true;
            }

            if (errorOccured)
            {
                NotificationService.Notify(NotificationSeverity.Error, "An error occured while trying to update the hero.");
            }
        }
    }
}
