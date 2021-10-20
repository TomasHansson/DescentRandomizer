using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DataTransferObjects
{
    public class Class
    {
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        public Archetype Archetype { get; set; }
        public bool HybridClass { get; set; }
        public Archetype HybridArchetype { get; set; }
        public List<ClassCard> ClassCards { get; set; }
        public List<ClassItem> ClassItems { get; set; }
    }
}
