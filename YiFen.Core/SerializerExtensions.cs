using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Linq.Expressions;
using System.Collections.Specialized;
using System.IO;
using System.Xml.Serialization;

namespace YiFen.Core
{
    public static class SerializerExtensions
    {
        public static string ToXMLString(this object o)
        {
            MemoryStream mstream = new MemoryStream();
            XmlAttributeOverrides dd = new XmlAttributeOverrides();
            XmlSerializer xmls = new XmlSerializer(o.GetType());
            xmls.Serialize(mstream, o);
            mstream.Position = 0;
            StreamReader sstream = new StreamReader(mstream);
            return sstream.ReadToEnd();
        }

        public static T XMLDeserialize<T>(this string str)
        {
            MemoryStream mstream = new MemoryStream(Encoding.UTF8.GetBytes(str));
            return mstream.XMLDeserialize<T>();
        }

        public static T XMLDeserialize<T>(this Stream stream)
        {
            XmlSerializer xmls = new XmlSerializer(typeof(T));
            try
            {
                return (T)xmls.Deserialize(stream);
            }
            catch
            {
                return default(T);
            }

        }
    }
}