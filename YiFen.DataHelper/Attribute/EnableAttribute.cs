using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YiFen.DataHelper
{
    public abstract class EnableAttribute : Attribute
    {
        public bool Enable { get; set; }
    }
}
