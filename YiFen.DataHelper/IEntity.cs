using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YiFen.DataHelper
{
    public interface IEntity
    {
        /// <summary>
        /// 兼容旧版本，新版本不再使用
        /// </summary>
        bool IsDBNull { get; set; }
    }
}
