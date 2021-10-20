using BlazorApp.Client.Shared;
using BlazorApp.Client.Utility;
using Domain.DataTransferObjects;
using Domain.Enums;
using Domain.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Radzen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace BlazorApp.Client.Pages.Randomize
{
    public partial class Character
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

        private readonly RandomCharacterRequest _request = new();
        private IEnumerable<Hero> _heroes;
        private IEnumerable<Class> _classes;
        private readonly Random _randomizer = new();
        private Domain.Models.Character _character = new();

        protected override async Task OnInitializedAsync()
        {
            _heroes = await GetHeroes();
            _classes = await GetClasses();
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

        private void Submit()
        {
            _character = new();
            var validArchetypes = ValidArchetypes();
            if (validArchetypes.Count == 0)
            {
                NotifyUnableToBuildCharacterBasesOnFilters();
                return;
            }
            var validHeroSelections = ValidHeroSelections(validArchetypes);
            if (validHeroSelections.Count == 0)
            {
                NotifyUnableToBuildCharacterBasesOnFilters();
                return;
            }
            int index = _randomizer.Next(0, validHeroSelections.Count);
            var hero = validHeroSelections[index];
            var validMainClassSelections = ValidMainClassSelections(hero);
            if (validMainClassSelections.Count == 0)
            {
                NotifyUnableToBuildCharacterBasesOnFilters();
                return;
            }
            index = _randomizer.Next(0, validMainClassSelections.Count);
            var mainClass = validMainClassSelections[index];
            Class secondaryClass = null;
            if (mainClass.HybridClass)
            {
                var validSecondaryClasses = ValidSecondaryClassSelections(mainClass);
                index = _randomizer.Next(0, validSecondaryClasses.Count);
                secondaryClass = validSecondaryClasses[index];
            }
            NotificationService.Notify(summary: "Character has been generated.");
            _character = new Domain.Models.Character { Hero = hero, MainClass = mainClass, SecondaryClass = secondaryClass };
        }

        private void NotifyUnableToBuildCharacterBasesOnFilters()
        {
            NotificationService.Notify(summary: "There are no combinations of heroes and classes in the database that would build a character with your current filters.");
        }

        private List<Archetype> ValidArchetypes()
        {
            var remainingHeroes = _heroes.Where(x => _request.HeroesToExclude.Contains(x.Id) == false);
            var remainingClasses = _classes.Where(x => _request.ClassesToExclude.Contains(x.Id) == false);
            if (_request.AllowHybridClasses == false)
            {
                remainingClasses = remainingClasses.Where(x => x.HybridClass == false);
            }
            var result = new List<Archetype>();
            foreach (Archetype archetype in Enum.GetValues(typeof(Archetype)))
            {
                if (remainingHeroes.Any(x => x.Archetype == archetype) && remainingClasses.Any(x => x.Archetype == archetype))
                {
                    if (remainingClasses.All(x => x.HybridClass))
                    {
                        // Not a valid archetype.
                    }
                    else if (remainingClasses.Where(x => x.Archetype == archetype).All(x => x.HybridClass))
                    {
                        var subClassArchetypes = remainingClasses.Where(x => x.Archetype == archetype).Select(x => x.HybridArchetype).ToList();
                        if (remainingClasses.Where(x => x.Archetype != archetype && x.HybridClass == false).Any(x => subClassArchetypes.Contains(x.Archetype)))
                        {
                            result.Add(archetype);
                        }
                    }
                    else
                    {
                        result.Add(archetype);
                    }
                }
            }
            return result;
        }

        private List<Hero> ValidHeroSelections(List<Archetype> validArchetypes)
        {
            return _heroes
                .Where(x => validArchetypes.Contains(x.Archetype))
                .Where(x => _request.HeroesToExclude.Contains(x.Id) == false)
                .ToList();
        }

        private List<Class> ValidMainClassSelections(Hero hero)
        {
            var validMainClassSelections = _classes
                .Where(x => x.Archetype == hero.Archetype)
                .Where(x => _request.ClassesToExclude.Contains(x.Id) == false);
            if (_request.AllowHybridClasses == false)
            {
                validMainClassSelections = validMainClassSelections.Where(x => x.HybridClass == false);
            }
            return validMainClassSelections.ToList();
        }

        private List<Class> ValidSecondaryClassSelections(Class mainClass)
        {
            return _classes
                .Where(x => _request.ClassesToExclude.Contains(x.Id) == false)
                .Where(x => x.Archetype == mainClass.HybridArchetype)
                .Where(x => x.HybridClass == false)
                .ToList();
        }

        private void ShowTooltip(ElementReference elementReference, string text)
        {
            TooltipService.Open(elementReference, text, new TooltipOptions { Position = TooltipPosition.Left });
        }

        private async Task ShowHeroDetails(Hero hero)
        {
            await DialogService.OpenAsync<HeroDetails>("Hero Details", new Dictionary<string, object>() { { "Hero", hero }, { "DialogService", DialogService } });
        }

        private async Task ShowClassDetails(Class selectedClass)
        {
            await DialogService.OpenAsync<ClassDetails>("Class Details", new Dictionary<string, object>() { { "Class", selectedClass }, { "DialogService", DialogService } });
        }
    }
}
