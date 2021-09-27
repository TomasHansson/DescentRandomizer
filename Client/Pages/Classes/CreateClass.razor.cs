using BlazorApp.Client.Utility;
using Domain.DataTransferObjects;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Radzen;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace BlazorApp.Client.Pages.Classes
{
    public partial class CreateClass
    {
        [Inject]
        public IConfiguration Configuration { get; set; }
        [Inject]
        public HttpClient HttpClient { get; set; }
        [Inject]
        public NotificationService NotificationService { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        private readonly Class _class = new() { Id = Guid.NewGuid() };
        private string _baseUrl;

        protected override async Task OnInitializedAsync()
        {
            _baseUrl = Configuration.GetConnectionString("API");
        }

        private async Task Submit()
        {
            if (_class != null)
            {
                var url = $"api/Classes/Name/{_class.Name}";
                var checkNameResponse = await HttpClient.GetAsync(url);
                var checkNameResult = await HttpUtilities.TryReadBooleanResponse(checkNameResponse);
                var errorOccured = false;
                if (checkNameResult.Success && !checkNameResult.ResultObject)
                {
                    url = $"api/Classes";
                    var createClassResponse = await HttpClient.PostAsJsonAsync(url, _class);
                    if (createClassResponse.IsSuccessStatusCode)
                    {
                        NavigationManager.NavigateTo("/Classes/Class was created succesfully.");
                    }
                    else
                    {
                        errorOccured = true;
                    }
                }
                else if (checkNameResult.Success && checkNameResult.ResultObject)
                {
                    NotificationService.Notify(NotificationSeverity.Warning, "A class with that name already exists.");
                }
                else
                {
                    errorOccured = true;
                }

                if (errorOccured)
                {
                    NotificationService.Notify(NotificationSeverity.Error, "An error occured while trying to create the new class.");
                }
            }
        }
    }
}
