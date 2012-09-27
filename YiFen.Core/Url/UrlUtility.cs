using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace YiFen.Core
{
    public static class UrlUtility
    {
        public static Uri ChangeValue(this HttpRequest Request, string Name, string Value)
        {
            Uri rawUri = new Uri(Request.RawUrl);
            if (string.IsNullOrEmpty(rawUri.Query))
            {
                //rawUri.s = "?" + Name + "=" + Value;
            }
            return rawUri;
        }
    }
}
