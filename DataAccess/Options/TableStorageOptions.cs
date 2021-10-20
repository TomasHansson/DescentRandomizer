using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Options
{
    public class TableStorageOptions
    {
        public string ConnectionString { get; set; }
        public string HeroesTable { get; set; }
        public string ClassesTable { get; set; }
        public string ClassCardsTable { get; set; }
        public string ClassItemsTable { get; set; }
        public string UsersTable { get; set; }
    }
}
