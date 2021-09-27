using Domain.DataTransferObjects;
using Domain.Enums;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DataAccessObjects
{
    public class HeroTableEntity : TableEntity
    {
        public Guid Id { get; set; }
        public int Archetype { get; set; }
        public string Name { get; set; }
        public int Speed { get; set; }
        public int Health { get; set; }
        public int Stamina { get; set; }
        public int Defense { get; set; }
        public int Willpower { get; set; }
        public int Might { get; set; }
        public int Knowledge { get; set; }
        public int Awareness { get; set; }
        public string HeroAbility { get; set; }
        public string HeroicFeat { get; set; }

        public Hero ConvertToHero()
        {
            return new Hero
            {
                Id = Id,
                Archetype = (Archetype)Archetype,
                Name = Name,
                Speed = Speed,
                Health = Health,
                Stamina = Stamina,
                Defense = (Defense)Defense,
                Willpower = Willpower,
                Might = Might,
                Knowledge = Knowledge,
                Awareness = Awareness,
                HeroAbility = HeroAbility,
                HeroicFeat = HeroicFeat
            };
        }

        public HeroTableEntity()
        {

        }

        public HeroTableEntity(Hero hero)
        {
            PartitionKey = hero.Id.ToString();
            RowKey = hero.Name;
            Id = hero.Id;
            Archetype = (int)hero.Archetype;
            Name = hero.Name;
            Speed = hero.Speed;
            Health = hero.Health;
            Stamina = hero.Stamina;
            Defense = (int)hero.Defense;
            Willpower = hero.Willpower;
            Might = hero.Might;
            Knowledge = hero.Knowledge;
            Awareness = hero.Awareness;
            HeroAbility = hero.HeroAbility;
            HeroicFeat = hero.HeroicFeat;
        }
    }
}
