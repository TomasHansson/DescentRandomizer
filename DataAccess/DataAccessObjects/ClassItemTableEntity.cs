using Domain.DataTransferObjects;
using Domain.Enums;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.DataAccessObjects
{
    public class ClassItemTableEntity : TableEntity
    {
        public Guid Id { get; set; }
        public Guid ClassId { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }
        public string Traits { get; set; }
        public int EquipType { get; set; }
        public int WeaponType { get; set; }
        public int PowerDie { get; set; }

        public ClassItem ConvertToClassItem()
        {
            return new ClassItem
            {
                Id = Id,
                ClassId = ClassId,
                Name = Name,
                Text = Text,
                Traits = Traits,
                EquipType = (EquipType)EquipType,
                WeaponType = (WeaponType)WeaponType,
                PowerDie = (PowerDie)PowerDie
            };
        }

        public ClassItemTableEntity()
        {

        }

        public ClassItemTableEntity(ClassItem classItem)
        {
            PartitionKey = classItem.ClassId.ToString();
            RowKey = classItem.Id.ToString();
            Id = classItem.Id;
            ClassId = classItem.ClassId;
            Name = classItem.Name;
            Text = classItem.Text;
            Traits = classItem.Traits;
            EquipType = (int)classItem.EquipType;
            WeaponType = (int)classItem.WeaponType;
            PowerDie = (int)classItem.PowerDie;
        }
    }
}
