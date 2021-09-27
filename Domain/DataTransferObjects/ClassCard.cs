using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DataTransferObjects
{
    public class ClassCard
    {
        public Guid Id { get; set; }
        public Guid ClassId { get; set; }
        [Required]
        public string Name { get; set; }
        public int ExperienceCost { get; set; }
        public int PlayCost { get; set; }
        [Required]
        public string Text { get; set; }
    }
}
