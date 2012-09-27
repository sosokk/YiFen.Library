using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YiFen.DataHelper
{
    public class UpdatableAttribute : EnableAttribute
    {
        public UpdatableAttribute(bool Enable)
        {
            base.Enable = Enable;
        }
    }
}
