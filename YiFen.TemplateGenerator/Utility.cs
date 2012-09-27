using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YiFen.TemplateGenerator.GetInformationAdapter;
using System.IO;
using System.Collections.Specialized;

namespace YiFen.TemplateGenerator
{
    public class Utility
    {
        public static IGetInformation GetInformation(string DataType)
        {
            switch (DataType)
            {
                case "SQLServer":
                    return new GetSQLServerInformation();
                default:
                    return new GetSQLServerInformation();
            }
        }

        public static void GeneratorEntity(List<TableInfo> Tables, string NameSpace, string SavePath)
        {
            string path = SavePath + NameSpace;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            foreach (TableInfo table in Tables)
            {
                File.WriteAllText(path + "\\" + table.Name + "Entity.cs", TemplateEntity(table, NameSpace));
            }
        }
        public static string TemplateEntity(TableInfo Table, string NameSpace)
        {
            StringBuilder codestring = new StringBuilder(@"using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using YiFen.DataHelper;
");
            codestring.Append("namespace ").Append(NameSpace).Append("\r\n{\r\n");
            codestring.Append("public class  ").Append(Table.Name).Append("Entity : YiFen.DataHelper.Entity\r\n{");
            foreach (FieldInfo field in Table.Fields)
            {
                codestring.Append(TemplateEntity(field));
            }
            codestring.Append("}\r\n}");
            return codestring.ToString();
        }
        public static string TemplateEntity(FieldInfo field)
        {
            StringBuilder codestring = new StringBuilder();
            codestring.Append("\r\npublic ")
                .Append(FriendlyName(field.DbType)).Append(" ")
                .Append(field.Name)
                .Append(" { get; set; } \r\n");
            return codestring.ToString();
        }

        public static void GeneratorService(List<TableInfo> Tables, string ServiceNameSpace, string EntityNameSpace, string SavePath)
        {
            string path = SavePath + ServiceNameSpace;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            foreach (TableInfo table in Tables)
            {
                File.WriteAllText(path + "\\" + table.Name + "Service.cs", TemplateService(table, ServiceNameSpace, EntityNameSpace));
            }
        }
        public static string TemplateService(TableInfo Table, string ServiceNameSpace, string EntityNameSpace)
        {
            StringBuilder codestring = new StringBuilder(@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YiFen.Core;
using YiFen.DataHelper;
");
            codestring.Append("using ").Append(EntityNameSpace).Append(";\r\n\r\n");
            codestring.Append("namespace ").Append(ServiceNameSpace).Append("\r\n{\r\n");
            codestring.Append("public class  ").Append(Table.Name).Append("Service : YiFen.DataHelper.Service<").Append(Table.Name).Append("Entity>\r\n{");
            codestring.Append("\r\n}\r\n}");
            return codestring.ToString();
        }



        public static string FriendlyName(Type type)
        {
            NameValueCollection typeNames = new NameValueCollection();
            typeNames.Add("String", "string");
            typeNames.Add("Int32", "int");
            typeNames.Add("Int64", "long");
            typeNames.Add("Decimal", "decimal");

            return typeNames[type.Name] ?? type.Name;
        }
    }
}
