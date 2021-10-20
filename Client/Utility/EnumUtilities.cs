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

        private static List<EquipTypeItem> _equipTypes;
        public static List<EquipTypeItem> GetEquipTypes()
        {
            if (_equipTypes is null)
            {
                _equipTypes = new List<EquipTypeItem>
                {
                    new EquipTypeItem { Name = "One Hands", Value = EquipType.OneHand },
                    new EquipTypeItem { Name = "Two Hands", Value = EquipType.TwoHands },
                    new EquipTypeItem { Name = "Other", Value = EquipType.Other },
                    new EquipTypeItem { Name = "Armor", Value = EquipType.Armor }
                };
            }
            return _equipTypes;
        }

        private static List<PowerDieItem> _powerDices;
        public static List<PowerDieItem> GetPowerDices()
        {
            if (_powerDices is null)
            {
                _powerDices = new List<PowerDieItem>
                {
                    new PowerDieItem { Name = "Green", Value = PowerDie.Green },
                    new PowerDieItem { Name = "Yellow", Value = PowerDie.Yellow },
                    new PowerDieItem { Name = "Red", Value = PowerDie.Red }
                };
            }
            return _powerDices;
        }

        private static List<WeaponTypeItem> _weaponTypes;
        public static List<WeaponTypeItem> GetWeaponTypes()
        {
            if (_weaponTypes is null)
            {
                _weaponTypes = new List<WeaponTypeItem>
                {
                    new WeaponTypeItem { Name = "Melee", Value = WeaponType.Melee },
                    new WeaponTypeItem { Name = "Ranged", Value = WeaponType.Ranged }
                };
            }
            return _weaponTypes;
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

    public class EquipTypeItem
    {
        public string Name { get; set; }
        public EquipType Value { get; set; }
    }

    public class PowerDieItem
    {
        public string Name { get; set; }
        public PowerDie Value { get; set; }
    }

    public class WeaponTypeItem
    {
        public string Name { get; set; }
        public WeaponType Value { get; set; }
    }
}
