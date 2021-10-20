using BlazorApp.Client.Shared;
using BlazorApp.Client.Utility;
using Domain.DataTransferObjects;
using Microsoft.AspNetCore.Components;
using Radzen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorApp.Client.Pages.ClassItems
{
    public partial class ClassItems
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

        private IEnumerable<ClassItem> _classItems;
        private List<Class> _classes;

        protected override async Task OnInitializedAsync()
        {
            if (!string.IsNullOrWhiteSpace(Message))
            {
                NotificationService.Notify(summary: Message);
            }
            _classItems = await GetClassItems();
            _classes = await GetClassesWithoutClassCards();
        }

        private async Task<List<ClassItem>> GetClassItems()
        {
            var url = "api/ClassItems";
            var response = await HttpClient.GetAsync(url);
            var result = await HttpUtilities.TryParseJsonResponse<List<ClassItem>>(response);
            if (result.Success)
            {
                return result.ResultObject;
            }
            else
            {
                NotificationService.Notify(NotificationSeverity.Error, "Unable to load class items.", result.ErrorMessage);
                return new List<ClassItem>();
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

        private void GoToCreateClassItem()
        {
            NavigationManager.NavigateTo("/ClassItems/Create");
        }

        private async Task ShowClassItemDetails(ClassItem classItem)
        {
            var cardsClass = _classes.First(x => x.Id == classItem.ClassId);
            await DialogService.OpenAsync<ClassItemDetails>("Class Item Details", new Dictionary<string, object>() { { "ClassItem", classItem },
                { "DialogService", DialogService }, { "Class", cardsClass } });
        }

        private void GoToEdit(ClassItem classItem)
        {
            NavigationManager.NavigateTo($"/ClassItems/{classItem.Id}/Edit");
        }

        private async Task DeleteClassItem(ClassItem classItem)
        {
            var result = await DialogService.Confirm($"Are you sure you want to delete the class item {classItem.Name}?");
            if (result == true)
            {
                var url = $"api/ClassItems/{classItem.Id}";
                var deleteClassCardResponse = await HttpClient.DeleteAsync(url);
                if (deleteClassCardResponse.IsSuccessStatusCode)
                {
                    NotificationService.Notify(NotificationSeverity.Info, "The class item was deleted.");
                    _classItems = await GetClassItems();
                    StateHasChanged();
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error, "An error occured while trying to delete the class item.");
                }
            }
        }

        private void ShowTooltip(ElementReference elementReference, string text)
        {
            TooltipService.Open(elementReference, text, new TooltipOptions { Position = TooltipPosition.Left });
        }
    }
}
