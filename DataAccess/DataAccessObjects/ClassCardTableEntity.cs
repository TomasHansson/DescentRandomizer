using Domain.DataTransferObjects;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DataAccessObjects
{
    public class ClassCardTableEntity : TableEntity
    {
        public Guid Id { get; set; }
        public Guid ClassId { get; set; }
        public string Name { get; set; }
        public int ExperienceCost { get; set; }
        public string PlayCost { get; set; }
        public string Text { get; set; }

        public ClassCard ConvertToClassCard()
        {
            return new ClassCard
            {
                Id = Id,
                ClassId = ClassId,
                Name = Name,
                ExperienceCost = ExperienceCost,
                PlayCost = PlayCost,
                Text = Text
            };
        }

        public ClassCardTableEntity()
        {

        }

        public ClassCardTableEntity(ClassCard classCard)
        {
            PartitionKey = classCard.ClassId.ToString();
            RowKey = classCard.Id.ToString();
            Id = classCard.Id;
            ClassId = classCard.ClassId;
            Name = classCard.Name;
            ExperienceCost = classCard.ExperienceCost;
            PlayCost = classCard.PlayCost;
            Text = classCard.Text;
        }
    }
}
