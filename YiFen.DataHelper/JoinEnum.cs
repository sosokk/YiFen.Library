using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YiFen.DataHelper
{
    public enum RelationEnum
    {
        InnerJoin = 0,
        NaturalJoin = 1,
        LeftOuterJoin = 2,
        RightOuterJoin = 3,
        FullOuterJoin = 4,
        CrossJoin = 5
    }
}
