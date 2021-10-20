using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityImporter.Models
{
    public class HybridSkillImport
    { 
        public string Name { get; set; }
        public int Points { get; set; }
        public string Archetype { get; set; }
        public string Class { get; set; }
        public int? Xpcost { get; set; }
        public string Rules { get; set; }
        public string Fatiguecost { get; set; }
        public string Expansion { get; set; }
        public string Image { get; set; }
        public string Xws { get; set; }
    }
}
