using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.DataTransferObjects
{
    public class ClassItem
    {
        public Guid Id { get; set; }
        public Guid ClassId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Text { get; set; }
        [Required]
        public string Traits { get; set; }
        public EquipType EquipType { get; set; }
        public WeaponType WeaponType { get; set; }
        public PowerDie PowerDie { get; set; }
    }
}
