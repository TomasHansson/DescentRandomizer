using BlazorApp.Client.Shared;
using Domain.DataTransferObjects;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Radzen;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace BlazorApp.Client.Pages.ClassCards
{
    public partial class ClassCards
    {
        [Inject]
        public HttpClient HttpClient { get; set; }
        [Inject]
        public IConfiguration Configuration { get; set; }
        [Inject]
        public DialogService DialogService { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }
        [Inject]
        public NotificationService NotificationService { get; set; }
        [Inject]
        public TooltipService TooltipService { get; set; }
        [Parameter]
        public string Message { get; set; }

        private IEnumerable<ClassCard> _classCards;
        private List<Class> _classes;
        private string _baseUrl;

        protected override async Task OnInitializedAsync()
        {
            _baseUrl = Configuration.GetConnectionString("API");
            if (!string.IsNullOrWhiteSpace(Message))
            {
                NotificationService.Notify(summary: Message);
            }
            _classCards = await GetClassCards();
            _classes = await GetClassesWithoutClassCards();
        }

        private async Task<List<ClassCard>> GetClassCards()
        {
            var url = $"api/ClassCards";
            return await HttpClient.GetFromJsonAsync<List<ClassCard>>(url);
        }

        private async Task<List<Class>> GetClassesWithoutClassCards()
        {
            var url = $"api/Classes/All/false";
            return await HttpClient.GetFromJsonAsync<List<Class>>(url);
        }

        private void GoToCreateClassCard()
        {
            NavigationManager.NavigateTo("/ClassCards/Create");
        }

        private async Task ShowClassCardDetails(ClassCard classCard)
        {
            var cardsClass = _classes.First(x => x.Id == classCard.ClassId);
            await DialogService.OpenAsync<ClassCardDetails>("Class Card Details", new Dictionary<string, object>() { { "ClassCard", classCard },
                { "DialogService", DialogService }, { "Class", cardsClass } });
        }

        private void GoToEdit(ClassCard classCard)
        {
            NavigationManager.NavigateTo($"/ClassCards/{classCard.Id}/Edit");
        }

        private async Task DeleteClassCard(ClassCard classCard)
        {
            var result = await DialogService.Confirm($"Are you sure you want to delete the hero {classCard.Name}?");
            if (result == true)
            {
                var url = $"api/ClassCards/{classCard.Id}";
                var deleteClassCardResponse = await HttpClient.DeleteAsync(url);
                if (deleteClassCardResponse.IsSuccessStatusCode)
                {
                    _classCards = await GetClassCards();
                    StateHasChanged();
                    NotificationService.Notify(NotificationSeverity.Info, "The class card was deleted.");
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error, "An error occured while trying to delete the class card.");
                }
            }
        }

        private void ShowTooltip(ElementReference elementReference, string text)
        {
            TooltipService.Open(elementReference, text, new TooltipOptions { Position = TooltipPosition.Left });
        }
    }
}
