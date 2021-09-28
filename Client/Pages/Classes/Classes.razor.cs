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

namespace BlazorApp.Client.Pages.Classes
{
    public partial class Classes
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
        [Parameter]
        public string Message { get; set; }

        private IEnumerable<Class> _classes;

        protected override async Task OnInitializedAsync()
        {
            if (!string.IsNullOrWhiteSpace(Message))
            {
                NotificationService.Notify(summary: Message);
            }
            _classes = await GetClasses();
        }

        private async Task<List<Class>> GetClasses()
        {
            var url = "api/Classes/All/true";
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

        private void GoToCreateClass()
        {
            NavigationManager.NavigateTo("/Classes/Create");
        }

        private async Task ShowClassDetails(Class selectedClass)
        {
            await DialogService.OpenAsync<ClassDetails>("Class Details", new Dictionary<string, object>() { { "Class", selectedClass }, { "DialogService", DialogService } });
        }

        private void GoToEdit(Class selectedClass)
        {
            NavigationManager.NavigateTo($"/Classes/{selectedClass.Id}/Edit");
        }

        private async Task DeleteClass(Class selectedClass)
        {
            var result = await DialogService.Confirm($"Are you sure you want to delete the class {selectedClass.Name}?");
            if (result == true)
            {
                var url = $"api/Classes/{selectedClass.Id}";
                var deleteClassResponse = await HttpClient.DeleteAsync(url);
                if (deleteClassResponse.IsSuccessStatusCode)
                {
                    NotificationService.Notify(NotificationSeverity.Info, "The class was deleted.");
                    _classes = await GetClasses();
                    StateHasChanged();
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error, "An error occured while trying to delete the class.");
                }
            }
        }

        private void ShowTooltip(ElementReference elementReference, string text)
        {
            TooltipService.Open(elementReference, text, new TooltipOptions { Position = TooltipPosition.Left });
        }
    }
}
