using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace YiFen.DataHelper
{
    public class SQLInfo
    {

        public List<IDataParameter> DataParameters { get; set; }

        public string SQLString { get; set; }

        public SQLInfo()
        {
            DataParameters = new List<IDataParameter>();
        }
    }


}
