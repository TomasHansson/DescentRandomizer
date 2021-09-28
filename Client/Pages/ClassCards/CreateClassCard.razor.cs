using BlazorApp.Client.Utility;
using Domain.DataTransferObjects;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Radzen;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace BlazorApp.Client.Pages.ClassCards
{
    public partial class CreateClassCard
    {
        [Inject]
        public HttpClient HttpClient { get; set; }
        [Inject]
        public NotificationService NotificationService { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        private readonly ClassCard _classCard = new() { Id = Guid.NewGuid() };
        private IEnumerable<Class> _classes;

        protected override async Task OnInitializedAsync()
        {
            _classes = await GetClassesWithoutClassCards();
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
            if (_classCard != null)
            {
                var url = $"api/ClassCards/Name/{_classCard.Name}/{_classCard.ClassId}";
                var checkNameResponse = await HttpClient.GetAsync(url);
                var checkNameResult = await HttpUtilities.TryReadBooleanResponse(checkNameResponse);
                var errorOccured = false;
                if (checkNameResult.Success && !checkNameResult.ResultObject)
                {
                    url = "api/ClassCards";
                    var createClassCardResponse = await HttpClient.PostAsJsonAsync(url, _classCard);
                    if (createClassCardResponse.IsSuccessStatusCode)
                    {
                        NavigationManager.NavigateTo("/ClassCards/Class card was created succesfully.");
                    }
                    else
                    {
                        errorOccured = true;
                    }
                }
                else if (checkNameResult.Success && checkNameResult.ResultObject)
                {
                    NotificationService.Notify(NotificationSeverity.Warning, "A class card with that name already exists for this class.");
                }
                else
                {
                    errorOccured = true;
                }

                if (errorOccured)
                {
                    NotificationService.Notify(NotificationSeverity.Error, "An error occured while trying to create the new class card.");
                }
            }
        }
    }
}
