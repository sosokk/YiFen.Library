using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YiFen.Core
{
    public class PageInfo
    {
        public int Page { get; set; }
        public int PageSize { get; set; }

        public PageInfo()
        {
            Page = 1;
            PageSize = 10;
            RecordCollection a = new RecordCollection();
            Record b = new Record();
        }
    }
}
