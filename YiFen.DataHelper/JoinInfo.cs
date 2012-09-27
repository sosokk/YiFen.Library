using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YiFen.DataHelper
{
    public class RelationInfo
    {
        public RelationEnum Relation { get; set; }

        public string TableName { get; set; }

        public string On { get; set; }
    }
}
