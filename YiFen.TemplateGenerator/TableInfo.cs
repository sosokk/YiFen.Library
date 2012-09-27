using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YiFen.TemplateGenerator
{
    public class TableInfo
    {
        public string Name { get; set; }

        public List<FieldInfo> Fields { get; set; }

        public TableInfo()
        {
            Fields = new List<FieldInfo>();
        }
    }
}
