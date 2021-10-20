using DataAccess.Options;
using DataAccess.Repositories;
using Domain.DataTransferObjects;
using Domain.Enums;
using EntityImporter.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace EntityImporter
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            // Insert ConnectionString below and activate the relevant functions whenever importing is required.
        }

        private static IOptions<TableStorageOptions> GetTableStorageOptions()
        {
            var tableStorageOptions = new TableStorageOptions
            {
                ConnectionString = "",
                HeroesTable = "heroes",
                ClassesTable = "classes",
                ClassCardsTable = "classcards",
                UsersTable = "users",
                ClassItemsTable = "classitems"
            };
            return Options.Create(tableStorageOptions);
        }

        private static List<string> ExpansionsToExclude()
        {
            return new List<string> { "Sands Of The Past", "Maze Of The Drakon", "Conversion Kit", "User Community", "Maze of the Drakon" };
        }

        private static void ImportHeroes()
        {
            using var streamReader = new StreamReader("./Entities/heroes.json");
            var content = streamReader.ReadToEnd();
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true };
            var heroes = JsonSerializer.Deserialize<List<HeroImport>>(content, options);
            heroes = heroes.Where(x => !x.Name.EndsWith(" Back")).ToList();
            heroes = heroes.Where(x => !ExpansionsToExclude().Contains(x.Expansion)).ToList();
            var convertedHeroes = new List<Hero>();
            foreach (var hero in heroes)
            {
                convertedHeroes.Add(new Hero
                {
                    Id = Guid.NewGuid(),
                    Archetype = Enum.Parse<Archetype>(hero.Archetype),
                    Name = hero.Name,
                    Speed = hero.Speed.Value,
                    Health = hero.Health.Value,
                    Stamina = hero.Stamina.Value,
                    Defense = Enum.Parse<Defense>(hero.Defense),
                    Willpower = hero.Willpower.Value,
                    Might = hero.Might.Value,
                    Knowledge = hero.Knowledge.Value,
                    Awareness = hero.Awareness.Value,
                    HeroAbility = hero.Ability,
                    HeroicFeat = hero.Feat
                });
            }
            var repository = new HeroesRepository(GetTableStorageOptions());
            foreach (var convertedHero in convertedHeroes)
            {
                repository.Create(convertedHero).GetAwaiter().GetResult();
            }
        }

        private static void ImportNonHybridClasses()
        {
            using var streamReader = new StreamReader("./Entities/class-skills.json");
            var content = streamReader.ReadToEnd();
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true };
            var classSkills = JsonSerializer.Deserialize<List<ClassSkillImport>>(content, options);
            classSkills = classSkills.Where(x => !ExpansionsToExclude().Contains(x.Expansion)).ToList();
            var importedClasses = classSkills.GroupBy(x => x.Class);
            var convertedClasses = new List<Class>();
            foreach (var group in importedClasses)
            {
                convertedClasses.Add(new Class
                {
                    Id = Guid.NewGuid(),
                    Name = group.Key,
                    Archetype = Enum.Parse<Archetype>(group.First().Archetype),
                    HybridClass = false,
                    HybridArchetype = Archetype.Warrior,
                    ClassCards = new List<ClassCard>()
                });
            }
            var repository = new ClassRepository(GetTableStorageOptions());
            foreach (var convertedClass in convertedClasses)
            {
                repository.Create(convertedClass).GetAwaiter().GetResult();
            }
        }

        private static void ImportHybridClasses()
        {
            using var streamReader = new StreamReader("./Entities/hybrid-class-skills.json");
            var content = streamReader.ReadToEnd();
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true };
            var classSkills = JsonSerializer.Deserialize<List<ClassSkillImport>>(content, options);
            classSkills = classSkills.Where(x => !ExpansionsToExclude().Contains(x.Expansion)).ToList();
            var importedClasses = classSkills.GroupBy(x => x.Class);
            var convertedClasses = new List<Class>();
            foreach (var group in importedClasses)
            {
                string hybridArchetype = group.First(x => x.Xpcost == 0)
                    .Rules.Substring("When you gain this card, choose 1 standard Class deck belonging to the ".Length).Split(" ").First();
                hybridArchetype = hybridArchetype.Substring(0, 1) + hybridArchetype.Substring(1).ToLower();
                convertedClasses.Add(new Class
                {
                    Id = Guid.NewGuid(),
                    Name = group.Key,
                    Archetype = Enum.Parse<Archetype>(group.First().Archetype),
                    HybridClass = true,
                    HybridArchetype = Enum.Parse<Archetype>(hybridArchetype),
                    ClassCards = new List<ClassCard>()
                });
            }
            var repository = new ClassRepository(GetTableStorageOptions());
            foreach (var convertedClass in convertedClasses)
            {
                repository.Create(convertedClass).GetAwaiter().GetResult();
            }
        }

        private static async Task ImportNonHybridClassCards()
        {
            using var streamReader = new StreamReader("./Entities/class-skills.json");
            var content = streamReader.ReadToEnd();
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true };
            var classSkills = JsonSerializer.Deserialize<List<ClassSkillImport>>(content, options);
            classSkills = classSkills.Where(x => !ExpansionsToExclude().Contains(x.Expansion)).ToList();
            var importedClassCards = classSkills.GroupBy(x => x.Class);

            var classRepository = new ClassRepository(GetTableStorageOptions());
            var classesInStorage = await classRepository.GetAll();
            if (classesInStorage.Success == false)
            {
                return;
            }

            var convertedClassCards = new List<ClassCard>();
            foreach (var cardsForClass in importedClassCards)
            {
                var classInStorage = classesInStorage.ResultObject.FirstOrDefault(x => x.Name == cardsForClass.Key);
                if (classInStorage is null)
                {
                    continue;
                }

                foreach (var card in cardsForClass)
                {
                    if (card.Image.EndsWith("errata.png"))
                    {
                        continue;
                    }

                    convertedClassCards.Add(new ClassCard
                    {
                        Id = Guid.NewGuid(),
                        ClassId = classInStorage.Id,
                        Name = card.Name,
                        ExperienceCost = card.Xpcost.Value,
                        PlayCost = card.Fatiguecost,
                        Text = card.Rules
                    });
                }
            }
            var classCardRepository = new ClassCardRepository(GetTableStorageOptions());
            foreach (var convertedClassCard in convertedClassCards)
            {
                classCardRepository.Create(convertedClassCard).GetAwaiter().GetResult();
            }
        }

        private static async Task ImportHybridClassCards()
        {
            using var streamReader = new StreamReader("./Entities/hybrid-class-skills.json");
            var content = streamReader.ReadToEnd();
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true };
            var hybridClassSkills = JsonSerializer.Deserialize<List<HybridSkillImport>>(content, options);
            hybridClassSkills = hybridClassSkills.Where(x => !ExpansionsToExclude().Contains(x.Expansion)).ToList();
            var importedClassCards = hybridClassSkills.GroupBy(x => x.Class);

            var classRepository = new ClassRepository(GetTableStorageOptions());
            var classesInStorage = await classRepository.GetAll();
            if (classesInStorage.Success == false)
            {
                return;
            }

            var convertedHybridClassCards = new List<ClassCard>();
            foreach (var cardsForClass in importedClassCards)
            {
                var classInStorage = classesInStorage.ResultObject.FirstOrDefault(x => x.Name == cardsForClass.Key);
                if (classInStorage is null)
                {
                    continue;
                }

                foreach (var card in cardsForClass)
                {
                    if (card.Image.EndsWith("errata.png"))
                    {
                        continue;
                    }

                    convertedHybridClassCards.Add(new ClassCard
                    {
                        Id = Guid.NewGuid(),
                        ClassId = classInStorage.Id,
                        Name = card.Name,
                        ExperienceCost = card.Xpcost.Value,
                        PlayCost = card.Fatiguecost,
                        Text = card.Rules
                    });
                }
            }
            var classCardRepository = new ClassCardRepository(GetTableStorageOptions());
            foreach (var convertedClassCard in convertedHybridClassCards)
            {
                classCardRepository.Create(convertedClassCard).GetAwaiter().GetResult();
            }
        }

        private static async Task ImportClassItems()
        {
            using var streamReader = new StreamReader("./Entities/class-items.json");
            var content = streamReader.ReadToEnd();
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true };
            var classItems = JsonSerializer.Deserialize<List<ClassItemImport>>(content, options);
            classItems = classItems.Where(x => !ExpansionsToExclude().Contains(x.Expansion)).ToList();
            var importedClassItems = classItems.GroupBy(x => x.Class);

            var classRepository = new ClassRepository(GetTableStorageOptions());
            var classesInStorage = await classRepository.GetAll();
            if (classesInStorage.Success == false)
            {
                return;
            }

            var convertedClassItems = new List<ClassItem>();
            foreach (var itemsForClass in importedClassItems)
            {
                var classInStorage = classesInStorage.ResultObject.FirstOrDefault(x => x.Name == itemsForClass.Key);
                if (classInStorage is null)
                {
                    continue;
                }

                foreach (var item in itemsForClass)
                {
                    if (item.Image.EndsWith("errata.png"))
                    {
                        continue;
                    }

                    convertedClassItems.Add(new ClassItem
                    {
                        Id = Guid.NewGuid(),
                        ClassId = classInStorage.Id,
                        Name = item.Name,
                        Text = item.Rules,
                        Traits = item.Traits,
                        EquipType = ConvertToEquipType(item.Equip),
                        WeaponType = ConvertToWeaponType(item.Attack),
                        PowerDie = ConvertToPowerDie(item.Dice)
                    });
                }
            }
            var classItemRepository = new ClassItemRepository(GetTableStorageOptions());
            foreach (var convertedClassItem in convertedClassItems)
            {
                classItemRepository.Create(convertedClassItem).GetAwaiter().GetResult();
            }
        }

        private static EquipType ConvertToEquipType(string equip)
        {
            return equip switch
            {
                "One Hand" => EquipType.OneHand,
                "Two Hands" => EquipType.TwoHands,
                "Other" => EquipType.Other,
                "Armor" => EquipType.Armor,
                _ => EquipType.Other
            };
        }

        private static WeaponType ConvertToWeaponType(string attack)
        {
            return attack switch
            {
                "Melee" => WeaponType.Melee,
                "Range" => WeaponType.Ranged,
                _ => WeaponType.None
            };
        }

        private static PowerDie ConvertToPowerDie(string dice)
        {
            if (string.IsNullOrWhiteSpace(dice) || dice == "-")
            {
                return PowerDie.None;
            }
            string powerDie = dice.Split(" ").Last();
            return powerDie switch
            {
                "Green" => PowerDie.Green,
                "Yellow" => PowerDie.Yellow,
                "Red" => PowerDie.Red,
                _ => PowerDie.None
            };
        }
    }
}
