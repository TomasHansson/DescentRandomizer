using Domain.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class Character
    {
        public Hero Hero { get; set; }
        public Class MainClass { get; set; }
        public Class SecondaryClass { get; set; }
    }
}
