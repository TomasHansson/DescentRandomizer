using BlazorApp.Client.Utility;
using Domain.DataTransferObjects;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Radzen;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace BlazorApp.Client.Pages.ClassCards
{
    public partial class EditClassCard
    {
        [Inject]
        public HttpClient HttpClient { get; set; }
        [Inject]
        public NotificationService NotificationService { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }
        [Parameter]
        public string Id { get; set; }

        private ClassCard _classCard;
        private IEnumerable<Class> _classes;

        protected override async Task OnInitializedAsync()
        {
            _classCard = await LoadClassCard(Id);
            _classes = await GetClassesWithoutClassCards();
        }

        private async Task<ClassCard> LoadClassCard(string id)
        {
            var url = $"api/ClassCards/{id}";
            var response = await HttpClient.GetAsync(url);
            var result = await HttpUtilities.TryParseJsonResponse<ClassCard>(response);
            if (result.Success)
            {
                return result.ResultObject;
            }
            else
            {
                NotificationService.Notify(NotificationSeverity.Error, "Unable to load class card.", result.ErrorMessage);
                return new ClassCard();
            }
        }

        private async Task<List<Class>> GetClassesWithoutClassCards()
        {
            var url = "api/Classes/All/false";
            var response = await HttpClient.GetAsync(url);
            var result = await HttpUtilities.TryParseJsonResponse<List<Class>>(response);
            if (result.Success)
            {
                return result.ResultObject;
            }
            else
            {
                NotificationService.Notify(NotificationSeverity.Error, "Unable to load classes.", result.ErrorMessage);
                return new List<Class>();
            }
        }

        private async Task Submit()
        {
            var url = $"api/ClassCards/Name/{_classCard.Name}/{_classCard.ClassId}/{_classCard.Id}";
            var checkNameResponse = await HttpClient.GetAsync(url);
            var checkNameResult = await HttpUtilities.TryReadBooleanResponse(checkNameResponse);
            var errorOccured = false;
            if (checkNameResult.Success && !checkNameResult.ResultObject)
            {
                url = "api/ClassCards";
                var editClassCardResponse = await HttpClient.PutAsJsonAsync(url, _classCard);
                if (editClassCardResponse.IsSuccessStatusCode)
                {
                    NotificationService.Notify(NotificationSeverity.Success, "Class card was edited succesfully.");
                    NavigationManager.NavigateTo("/ClassCards");
                }
                else
                {
                    errorOccured = true;
                }
            }
            else if (checkNameResult.Success && checkNameResult.ResultObject)
            {
                NotificationService.Notify(NotificationSeverity.Warning, "Another class card with that name already exists for this class.");
            }
            else
            {
                errorOccured = true;
            }

            if (errorOccured)
            {
                NotificationService.Notify(NotificationSeverity.Error, "An error occured while trying to update the class card.");
            }
        }
    }
}
