using BlazorApp.Client.Utility;
using Domain.DataTransferObjects;
using Domain.Enums;
using Microsoft.AspNetCore.Components;
using Radzen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace BlazorApp.Client.Pages.ClassItems
{
    public partial class EditClassItem
    {
        [Inject]
        public HttpClient HttpClient { get; set; }
        [Inject]
        public NotificationService NotificationService { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }
        [Parameter]
        public string Id { get; set; }

        private ClassItem _classItem;
        private IEnumerable<Class> _classes;

        protected override async Task OnInitializedAsync()
        {
            _classItem = await LoadClassItem(Id);
            _classes = await GetClassesWithoutClassCards();
        }

        private async Task<ClassItem> LoadClassItem(string id)
        {
            var url = $"api/ClassItems/{id}";
            var response = await HttpClient.GetAsync(url);
            var result = await HttpUtilities.TryParseJsonResponse<ClassItem>(response);
            if (result.Success)
            {
                return result.ResultObject;
            }
            else
            {
                NotificationService.Notify(NotificationSeverity.Error, "Unable to load class item.", result.ErrorMessage);
                return new ClassItem();
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
            if ((_classItem.EquipType == EquipType.OneHand || _classItem.EquipType == EquipType.TwoHands)
                && _classItem.WeaponType == WeaponType.None || _classItem.PowerDie == PowerDie.None)
            {
                NotificationService.Notify(summary: "You must set both Weapon Type and Power Die when creating a one- or two-handed item.");
                return;
            }

            var url = $"api/ClassItems/Name/{_classItem.Name}/{_classItem.ClassId}/{_classItem.Id}";
            var checkNameResponse = await HttpClient.GetAsync(url);
            var checkNameResult = await HttpUtilities.TryReadBooleanResponse(checkNameResponse);
            var errorOccured = false;
            if (checkNameResult.Success && !checkNameResult.ResultObject)
            {
                if (_classItem.EquipType != EquipType.OneHand && _classItem.EquipType != EquipType.TwoHands)
                {
                    _classItem.WeaponType = WeaponType.None;
                    _classItem.PowerDie = PowerDie.None;
                }
                url = "api/ClassItems";
                var editClassCardResponse = await HttpClient.PutAsJsonAsync(url, _classItem);
                if (editClassCardResponse.IsSuccessStatusCode)
                {
                    NotificationService.Notify(NotificationSeverity.Success, "Class item was edited succesfully.");
                    NavigationManager.NavigateTo("/ClassItems");
                }
                else
                {
                    errorOccured = true;
                }
            }
            else if (checkNameResult.Success && checkNameResult.ResultObject)
            {
                NotificationService.Notify(NotificationSeverity.Warning, "Another class item with that name already exists for this class.");
            }
            else
            {
                errorOccured = true;
            }

            if (errorOccured)
            {
                NotificationService.Notify(NotificationSeverity.Error, "An error occured while trying to update the class item.");
            }
        }
    }
}
