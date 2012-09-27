using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YiFen.DataHelper
{
    public class InsertableAttribute : EnableAttribute
    {
        public InsertableAttribute(bool Enable)
        {
            base.Enable = Enable;
        }
    }
}
