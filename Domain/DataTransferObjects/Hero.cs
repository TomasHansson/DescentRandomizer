using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DataTransferObjects
{
    public class Hero
    {
        public Guid Id { get; set; }
        [Required]
        public Archetype Archetype { get; set; }
        public string Name { get; set; }
        public int Speed { get; set; }
        public int Health { get; set; }
        public int Stamina { get; set; }
        public Defense Defense { get; set; }
        public int Willpower { get; set; }
        public int Might { get; set; }
        public int Knowledge { get; set; }
        public int Awareness { get; set; }
        [Required]
        public string HeroAbility { get; set; }
        [Required]
        public string HeroicFeat { get; set; }
    }
}
