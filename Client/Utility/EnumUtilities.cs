using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorApp.Client.Utility
{
    public static class EnumUtilities
    {
        private static List<ArchetypeItem> _archetypes;
        public static List<ArchetypeItem> GetArchetypes()
        {
            if (_archetypes is null)
            {
                _archetypes = new List<ArchetypeItem>();
                foreach (Archetype archetype in Enum.GetValues(typeof(Archetype)))
                {
                    _archetypes.Add(new ArchetypeItem { Name = archetype.ToString(), Value = archetype });
                }
            }
            return _archetypes;
        }

        private static List<DefenseItem> _defenses;
        public static List<DefenseItem> GetDefenses()
        {
            if (_defenses is null)
            {
                _defenses = new List<DefenseItem>();
                foreach (Defense defense in Enum.GetValues(typeof(Defense)))
                {
                    _defenses.Add(new DefenseItem { Name = defense.ToString(), Value = defense });
                }
            }
            return _defenses;
        }

        private static List<CriteriaItem> _criterias;
        public static List<CriteriaItem> GetCriterias()
        {
            if (_criterias is null)
            {
                _criterias = new List<CriteriaItem>
                {
                    new CriteriaItem { Name = "Fully Randomized", Value = Criteria.FullyRandomized },
                    new CriteriaItem { Name = "Max X Of Each Archetype", Value = Criteria.MaxXOfEachArchetype }
                };
            }
            return _criterias;
        }
    }

    public class ArchetypeItem
    {
        public string Name { get; set; }
        public Archetype Value { get; set; }
    }

    public class DefenseItem
    {
        public string Name { get; set; }
        public Defense Value { get; set; }
    }

    public class CriteriaItem
    {
        public string Name { get; set; }
        public Criteria Value { get; set; }
    }
}
