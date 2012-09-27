using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YiFen.TemplateGenerator
{
    public interface IGetInformation
    {
        List<TableInfo> Tables(string ConnectionString);
    }
}
