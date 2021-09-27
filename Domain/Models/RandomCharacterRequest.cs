using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class RandomCharacterRequest
    {
        public IEnumerable<Guid> HeroesToExclude { get; set; } = new List<Guid>();
        public IEnumerable<Guid> ClassesToExclude { get; set; } = new List<Guid>();
        public bool AllowHybridClasses { get; set; }
    }
}
