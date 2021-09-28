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
    public partial class Party
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

        private RandomPartyRequest _request = new();
        private IEnumerable<Hero> _heroes;
        private IEnumerable<Class> _classes;
        private Random randomizer = new();
        private List<Domain.Models.Character> _party = new();

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
            _party = new();
            var selectedCombinations = new List<Domain.Models.Character>();
            for (int i = 0; i < _request.NumberOfCharacters; i++)
            {
                var validArchetypes = ValidArchetypes(selectedCombinations);
                if (validArchetypes.Count == 0)
                {
                    NotifyUnableToBuildPartyBasesOnFilters();
                    return;
                }
                var validHeroSelections = ValidHeroSelections(selectedCombinations, validArchetypes);
                if (validHeroSelections.Count == 0)
                {
                    NotifyUnableToBuildPartyBasesOnFilters();
                    return;
                }
                int index = randomizer.Next(0, validHeroSelections.Count);
                var hero = validHeroSelections[index];
                validArchetypes = validArchetypes.Where(x => x == hero.Archetype).ToList();
                var validClassSelections = ValidClassSelections(selectedCombinations, validArchetypes);
                if (validClassSelections.Count == 0)
                {
                    NotifyUnableToBuildPartyBasesOnFilters();
                    return;
                }
                index = randomizer.Next(0, validClassSelections.Count);
                var mainClass = validClassSelections[index];
                Class secondaryClass = null;
                if (mainClass.HybridClass)
                {
                    var validSecondaryClasses = validClassSelections.Where(x => !x.HybridClass && x.Archetype == mainClass.HybridArchetype).ToList();
                    index = randomizer.Next(0, validSecondaryClasses.Count);
                    secondaryClass = validSecondaryClasses[index];
                }
                selectedCombinations.Add(new Domain.Models.Character { Hero = hero, MainClass = mainClass, SecondaryClass = secondaryClass });
            }
            NotificationService.Notify(summary: "Party has been generated.");
            _party = selectedCombinations;
        }

        private void NotifyUnableToBuildPartyBasesOnFilters()
        {
            NotificationService.Notify(summary: "There are no combinations of heroes and classes in the database that would build a complete party with your current filters.");
        }

        private List<Archetype> ValidArchetypes(List<Domain.Models.Character> selectedCombinations)
        {
            var remainingHeroes = _heroes.Where(x => selectedCombinations.Any(y => y.Hero == x) == false);
            remainingHeroes = remainingHeroes.Where(x => _request.HeroesToExclude.Contains(x.Id) == false);
            var remainingClasses = _classes.Where(x => selectedCombinations.Any(y => y.MainClass == x || y.SecondaryClass == x) == false);
            remainingClasses = remainingClasses.Where(x => _request.ClassesToExclude.Contains(x.Id) == false);
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
                        if (remainingClasses.Where(x => x.Archetype != archetype).Any(x => subClassArchetypes.Contains(x.Archetype)))
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

        private List<Hero> ValidHeroSelections(List<Domain.Models.Character> selectedCombinations, List<Archetype> validArchetypes)
        {
            var validHeroSelections = _heroes.Where(x => selectedCombinations.Any(y => y.Hero == x) == false);
            validHeroSelections = validHeroSelections.Where(x => validArchetypes.Contains(x.Archetype));
            validHeroSelections = validHeroSelections.Where(x => _request.HeroesToExclude.Contains(x.Id) == false);
            if (_request.Criteria == Criteria.MaxXOfEachArchetype)
            {
                foreach (Archetype archetype in Enum.GetValues(typeof(Archetype)))
                {
                    if (selectedCombinations.Where(x => x.Hero.Archetype == archetype).Count() == _request.MaxNumberOfEachArchetype)
                    {
                        validHeroSelections = validHeroSelections.Where(x => x.Archetype != archetype);
                    }
                }
            }
            return validHeroSelections.ToList();
        }

        private List<Class> ValidClassSelections(List<Domain.Models.Character> selectedCombinations, List<Archetype> validArchetypes)
        {
            var validClassSelections = _classes.Where(x => selectedCombinations.Any(y => y.MainClass == x || y.SecondaryClass == x) == false);
            validClassSelections = validClassSelections.Where(x => validArchetypes.Contains(x.Archetype));
            validClassSelections = validClassSelections.Where(x => _request.ClassesToExclude.Contains(x.Id) == false);
            if (_request.Criteria == Criteria.MaxXOfEachArchetype)
            {
                foreach (Archetype archetype in Enum.GetValues(typeof(Archetype)))
                {
                    if (selectedCombinations.Where(x => x.MainClass.Archetype == archetype).Count() == _request.MaxNumberOfEachArchetype)
                    {
                        validClassSelections = validClassSelections.Where(x => x.Archetype != archetype);
                    }
                }
            }
            if (_request.AllowHybridClasses == false)
            {
                validClassSelections = validClassSelections.Where(x => x.HybridClass == false);
            }
            return validClassSelections.ToList();
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
