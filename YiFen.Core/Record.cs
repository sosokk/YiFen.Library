using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YiFen.Core
{
    public class Record : List<RecordValue>
    {
        public void Add(string Name, object Value)
        {
            RecordValue rv = new RecordValue();
            rv.Name = Name;
            rv.Value = Value;
            this.Add(rv);
        }

        public object this[string Name]
        {
            get
            {
                RecordValue rv = this.FirstOrDefault(x => x.Name == Name);
                if (rv == null) rv = new RecordValue();
                return rv.Value;
            }
            set
            {
                Add(Name, value);
            }
        }
    }

    public class RecordValue
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public override string ToString()
        {
            return Value == null ? base.ToString() : Value.ToString();
        }
    }
}
