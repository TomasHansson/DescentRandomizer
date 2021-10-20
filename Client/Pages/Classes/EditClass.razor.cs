using BlazorApp.Client.Utility;
using Domain.DataTransferObjects;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Radzen;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace BlazorApp.Client.Pages.Classes
{
    public partial class EditClass
    {
        [Inject]
        public HttpClient HttpClient { get; set; }
        [Inject]
        public NotificationService NotificationService { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }
        [Parameter]
        public string Id { get; set; }

        private Class _class;

        protected override async Task OnInitializedAsync()
        {
            _class = await LoadClass(Id);
        }

        private async Task<Class> LoadClass(string id)
        {
            var url = $"api/Classes/{id}";
            var response = await HttpClient.GetAsync(url);
            var result = await HttpUtilities.TryParseJsonResponse<Class>(response);
            if (result.Success)
            {
                return result.ResultObject;
            }
            else
            {
                NotificationService.Notify(NotificationSeverity.Error, "Unable to load class.", result.ErrorMessage);
                return new Class();
            }
        }

        private async Task Submit()
        {
            var url = $"api/Classes/Name/{_class.Name}/{_class.Id}";
            var checkNameResponse = await HttpClient.GetAsync(url);
            var checkNameResult = await HttpUtilities.TryReadBooleanResponse(checkNameResponse);
            var errorOccured = false;
            if (checkNameResult.Success && !checkNameResult.ResultObject)
            {
                url = "api/Classes";
                var editClassResponse = await HttpClient.PutAsJsonAsync(url, _class);
                if (editClassResponse.IsSuccessStatusCode)
                {
                    NotificationService.Notify(NotificationSeverity.Success, "Class was edited succesfully.");
                    NavigationManager.NavigateTo("/Classes");
                }
                else
                {
                    errorOccured = true;
                }
            }
            else if (checkNameResult.Success && checkNameResult.ResultObject)
            {
                NotificationService.Notify(NotificationSeverity.Warning, "Another class with that name already exists.");
            }
            else
            {
                errorOccured = true;
            }

            if (errorOccured)
            {
                NotificationService.Notify(NotificationSeverity.Error, "An error occured while trying to update the class.");
            }
        }
    }
}
