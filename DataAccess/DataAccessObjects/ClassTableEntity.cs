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
    public class ClassTableEntity : TableEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Archetype { get; set; }
        public bool HybridClass { get; set; }
        public int HybridArchetype { get; set; }

        public Class ConvertToClass()
        {
            return new Class
            {
                Id = Id,
                Name = Name,
                Archetype = (Archetype)Archetype,
                HybridClass = HybridClass,
                HybridArchetype = (Archetype)HybridArchetype
            };
        }

        public ClassTableEntity()
        {

        }

        public ClassTableEntity(Class classObject)
        {
            PartitionKey = classObject.Id.ToString();
            RowKey = classObject.Name;
            Id = classObject.Id;
            Name = classObject.Name;
            Archetype = (int)classObject.Archetype;
            HybridClass = classObject.HybridClass;
            HybridArchetype = (int)classObject.HybridArchetype;
        }
    }
}
