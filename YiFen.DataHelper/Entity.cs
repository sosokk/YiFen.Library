using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YiFen.DataHelper
{
    [Serializable]
    public class Entity : IEntity
    {
        bool _IsDBNull = true;

        [Updatable(false)]
        [Insertable(false)]
        [Selectable(false)]
        public bool IsDBNull { get { return _IsDBNull; } set { _IsDBNull = value; } }
    }
}
