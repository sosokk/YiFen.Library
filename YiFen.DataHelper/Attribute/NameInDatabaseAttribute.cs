using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YiFen.DataHelper
{
    public class NameInDatabaseAttribute : Attribute
    {
        public string Name { get; set; }

        public NameInDatabaseAttribute(string Name)
        {
            this.Name = Name;
        }
    }
}
