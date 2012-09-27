using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YiFen.DataHelper
{
    public class SelectableAttribute : EnableAttribute
    {
        public SelectableAttribute(bool Enable)
        {
            base.Enable = Enable;
        }
    }
}
