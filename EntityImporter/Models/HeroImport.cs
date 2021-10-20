using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityImporter.Models
{
    public class HeroImport
    {
        public string Name { get; set; }
        public int Points { get; set; }
        public string Archetype { get; set; }
        public int? Speed { get; set; }
        public int? Health { get; set; }
        public int? Stamina { get; set; }
        public string Defense { get; set; }
        public int? Willpower { get; set; }
        public int? Might { get; set; }
        public int? Knowledge { get; set; }
        public int? Awareness { get; set; }
        public string Ability { get; set; }
        public string Feat { get; set; }
        public string Expansion { get; set; }
        public string Image { get; set; }
        public string Xws { get; set; }
    }
}
