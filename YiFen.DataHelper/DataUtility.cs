using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using YiFen.DataHelper.QueryAdapter;
using System.Reflection;
using System.Diagnostics;
using System.Data;
using YiFen.Core;
using System.Linq.Expressions;
using Artech.ILInvoker;

namespace YiFen.DataHelper
{
    public static class DataUtility
    {
        public static IQuery<T> GetAdapter<T>(string ConnectionString = null, string AdapterName = null)
            where T : IEntity, new()
        {
            IQuery<T> iq = null;
            if (string.IsNullOrEmpty(ConnectionString))
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["DataHelperConnectionString"].ToString();
            }
            if (string.IsNullOrEmpty(AdapterName))
            {
                AdapterName = ConfigurationManager.AppSettings["DataHelperAdapter"];
            }
            switch (AdapterName)
            {
                case "SQL2005Adapter":
                    iq = new SQL2005Query<T>() { ConnectionString = ConnectionString };
                    break;
                default:
                    iq = Assembly.LoadFrom(AdapterName) as IQuery<T>;
                    break;
            }
            iq.ConnectionString = ConnectionString;
            return iq;
        }

        public static IQuery GetAdapter(string ConnectionString = null, string AdapterName = null)
        {
            IQuery iq = null;
            if (string.IsNullOrEmpty(ConnectionString))
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["DataHelperConnectionString"].ToString();
            }
            if (string.IsNullOrEmpty(AdapterName))
            {
                AdapterName = ConfigurationManager.AppSettings["DataHelperAdapter"];
            }
            switch (AdapterName)
            {
                case "SQL2005Adapter":
                    iq = new SQL2005Query() { ConnectionString = ConnectionString };
                    break;
                default:
                    iq = Assembly.LoadFrom(AdapterName) as IQuery;
                    break;
            }
            iq.ConnectionString = ConnectionString;
            return iq;
        }

        public static IDbQuery GetDbQuery()
        {
            IDbQuery ih = null;
            string AdapterName = ConfigurationManager.AppSettings["DataHelperAdapter"];
            switch (AdapterName)
            {
                case "SQL2005Adapter":
                    ih = new YiFen.DataHelper.QueryAdapter.SQL2005DbQuery();
                    break;
                default:
                    ih = new YiFen.DataHelper.QueryAdapter.SQL2005DbQuery();
                    break;
            }
            return ih;
        }

        public static T ToEntity<T>(this IDataReader Reader) where T : IEntity, new()
        {
            T entity = new T();
            Type type = typeof(T);
            using (Reader)
            {
                if (Reader.Read())
                {
                    SetValue(Reader, entity, type);
                    entity.IsDBNull = false;
                }
            }
            return entity;
        }

        public static IList<T> ToListEntity<T>(this IDataReader Reader)
            where T : IEntity, new()
        {
            List<T> Entities = new List<T>();
            Type type = typeof(T);
            using (Reader)
            {
                while (Reader.Read())
                {
                    T entity = new T();
                    SetValue(Reader, entity, type);
                    entity.IsDBNull = false;
                    Entities.Add(entity);
                }
            }
            return Entities;
        }

        public static IPagingList<T> ToPagingList<T>(this IEnumerable<T> List, int PageNumber, int PageSize, int TotalItems)
        {
            PagingList<T> pagingList = new PagingList<T>(List, PageNumber, PageSize, TotalItems);
            return pagingList;
        }

        public static T[] ToDataParameter<T>(this IEntity entity) where T : IDataParameter, new()
        {
            List<T> dataParameters = new List<T>();
            Type entityType = entity.GetType();

            foreach (PropertyInfo property in entityType.GetProperties())
            {
                string propertyName = GetPropertyName(property);
                T dataParameter = new T();
                dataParameter.ParameterName = "@" + propertyName;
                if (property.PropertyType.BaseType == typeof(Entity))
                {
                    dataParameter.Value = property.PropertyType.GetProperty("ID").GetValue(property.GetValue(entity, null), null);
                }
                else
                {
                    dataParameter.Value = property.GetValue(entity, null);
                }
                if (dataParameter.Value != null)
                {
                    dataParameters.Add(dataParameter);
                }
            }
            return dataParameters.ToArray();
        }

        public static string GetPropertyName(PropertyInfo Property)
        {
            string PropertyName = null;
            NameInDatabaseAttribute NameInDatabase = Attribute.GetCustomAttribute(Property, typeof(NameInDatabaseAttribute), true) as NameInDatabaseAttribute;
            if (NameInDatabase == null)
            {
                PropertyName = Property.Name;
            }
            else
            {
                PropertyName = NameInDatabase.Name;
            }
            return PropertyName;
        }

        public static StringBuilder ToText(this List<IDataParameter> dataParameters)
        {
            StringBuilder text = new StringBuilder();
            if (dataParameters != null)
            {
                foreach (IDataParameter datap in dataParameters)
                {
                    text.Append("[").Append(datap.ParameterName).Append("");
                    text.Append("(").Append(datap.DbType).Append(")]=>");
                    text.Append("").Append(datap.Value).Append("\r\n");
                }
            }
            return text;
        }

        static T SetValue<T>(IDataReader Reader, T t, Type type)
            where T : new()
        {
            if (t is IEntity)
            {
                IEntity entity = t as IEntity;
                entity.IsDBNull = false;
                for (int i = 0; i < Reader.FieldCount; i++)
                {
                    SetValue(Reader.GetName(i), Reader[i], t, type);
                }
            }
            else
            {
                t = (T)Reader[0];
            }
            return t;
        }

        static void SetValue(string key, object value, object entity, Type type)
        {
            string[] keys = key.Split('.');
            object currentT = entity;
            bool IsSuccess = true;
            for (int i = 0; i < keys.Length - 1; i++)
            {
                PropertyInfo property = currentT.GetType().GetProperty(keys[i]);
                if (property == null)
                {
                    IsSuccess = false;
                    break;
                }

                if (PropertyAccessor.Get(currentT, property.Name) == null)
                    PropertyAccessor.Set(currentT, property.Name, property.PropertyType.GetConstructor(Type.EmptyTypes).Invoke(null));

                currentT = PropertyAccessor.Get(currentT, property.Name);
            }
            if (IsSuccess && !(value is DBNull) && currentT.GetType().GetProperty(keys[keys.Length - 1]) != null)
                PropertyAccessor.Set(currentT, keys[keys.Length - 1], value);
        }

        public static List<T> ToSimpleList<T>(this IDataReader Reader) where T : new()
        {
            List<T> tList = new List<T>();
            using (Reader)
            {
                while (Reader.Read())
                {
                    T t = new T();
                    t = (T)Reader[0];
                    tList.Add(t);
                }
            }
            return tList;
        }
        public static List<T> ToList<T>(this IDataReader Reader) where T : new()
        {
            List<T> tList = new List<T>();
            Type type = typeof(T);
            using (Reader)
            {
                while (Reader.Read())
                {
                    T t = new T();
                    t = SetValue(Reader, t, type);
                    tList.Add(t);
                }
            }
            return tList;
        }

        public static string GetTableName(this Type entityType)
        {
            string TableName = null;
            NameInDatabaseAttribute[] NameInDatabase = entityType.GetCustomAttributes(typeof(NameInDatabaseAttribute), true) as NameInDatabaseAttribute[];
            if (NameInDatabase.Length == 0)
            {
                TableName = entityType.Name;
                if (TableName.EndsWith("Entity", StringComparison.OrdinalIgnoreCase))
                {
                    TableName = TableName.Substring(0, TableName.Length - 6);
                }
            }
            else
            {
                TableName = NameInDatabase[0].Name;
            }
            return TableName;
        }
        public static string GetFieldName(this PropertyInfo property)
        {
            string PropertyName = null;
            NameInDatabaseAttribute NameInDatabase = Attribute.GetCustomAttribute(property, typeof(NameInDatabaseAttribute), true) as NameInDatabaseAttribute;
            if (NameInDatabase == null)
            {
                PropertyName = property.Name;
            }
            else
            {
                PropertyName = NameInDatabase.Name;
            }
            return PropertyName;
        }
        public static string GetFieldAsName(this PropertyInfo property, string prefix = null)
        {
            string asName = string.IsNullOrEmpty(prefix) ? string.Empty : prefix + ".";
            asName = asName + property.Name;
            if (property.PropertyType.GetInterface("IEntity") != null)
            {
                asName = asName + ".ID";
            }
            return asName;
        }

        public static List<string> GetSelectValidFields(this Type entityType, string include, string exclude, string prefix = null)
        {
            List<string> list = new List<string>();
            IDbQuery dbQuery = DataUtility.GetDbQuery();
            string tableName = entityType.GetTableName();
            string[] excludeSplit = exclude.SplitString();
            string[] includeSplit = include.SplitString();

            prefix += (string.IsNullOrWhiteSpace(prefix) ? "" : ".") + tableName;

            foreach (PropertyInfo property in entityType.GetProperties())
            {
                if (IsPropertyAllowed(property, SQLMethod.Select, includeSplit, excludeSplit))
                {
                    StringBuilder selectBuilder = new StringBuilder();
                    selectBuilder.Append(dbQuery.AddSeparator(tableName))
                        .Append(".")
                        .Append(dbQuery.AddSeparator(GetFieldName(property)))
                        .Append(" AS ")
                        .Append(dbQuery.AddSeparator(GetFieldAsName(property, prefix)));
                    list.Add(selectBuilder.ToString());
                }
            }
            return list;
        }
        public static List<string> GetUpdateValidFields(this Type entityType, string include, string exclude, string prefix = null)
        {
            List<string> list = new List<string>();
            IDbQuery dbQuery = DataUtility.GetDbQuery();
            string[] excludeSplit = exclude.SplitString();
            string[] includeSplit = include.SplitString();

            foreach (PropertyInfo property in entityType.GetProperties())
            {
                if (IsPropertyAllowed(property, SQLMethod.Update, includeSplit, excludeSplit))
                {
                    string field = GetFieldName(property);
                }
            }
            return list;
        }
        public static List<string> GetInsertValidFields(this Type entityType, string include, string exclude, string prefix = null)
        {
            List<string> list = new List<string>();
            IDbQuery dbQuery = DataUtility.GetDbQuery();
            string[] excludeSplit = exclude.SplitString();
            string[] includeSplit = include.SplitString();

            foreach (PropertyInfo property in entityType.GetProperties())
            {
                if (IsPropertyAllowed(property, SQLMethod.Insert, includeSplit, excludeSplit))
                {
                    string field = GetFieldName(property);
                    StringBuilder updateBuilder = new StringBuilder();
                    updateBuilder.Append(dbQuery.AddSeparator(field))
                        .Append(" = ")
                        .Append("@").Append(field);
                    list.Add(updateBuilder.ToString());
                }
            }
            return list;
        }

        public static string[] SplitString(this string original, char c = ',')
        {
            if (String.IsNullOrEmpty(original))
            {
                return new string[0];
            }
            var split = from piece in original.Split(c)
                        let trimmed = piece.Trim()
                        where !String.IsNullOrEmpty(trimmed)
                        select trimmed;
            return split.ToArray();
        }
        public static bool IsPropertyAllowed(string propertyName, string[] includeProperties, string[] excludeProperties)
        {
            // We allow a property to be bound if its both in the include list AND not in the exclude list.
            // An empty include list implies all properties are allowed.
            // An empty exclude list implies no properties are disallowed.
            bool includeProperty = (includeProperties == null) || (includeProperties.Length == 0) || includeProperties.Contains(propertyName, StringComparer.OrdinalIgnoreCase);
            bool excludeProperty = (excludeProperties != null) && excludeProperties.Contains(propertyName, StringComparer.OrdinalIgnoreCase);
            return includeProperty && !excludeProperty;
        }
        public static bool IsPropertyAllowed(PropertyInfo property, SQLMethod SQLMethod, string[] _includeSplit, string[] _excludeSplit)
        {
            EnableAttribute Enable = null;
            switch (SQLMethod)
            {
                case SQLMethod.Select:
                    Enable = Attribute.GetCustomAttribute(property, typeof(SelectableAttribute), true) as EnableAttribute;
                    break;
                case SQLMethod.Insert:
                    Enable = Attribute.GetCustomAttribute(property, typeof(InsertableAttribute), true) as EnableAttribute;
                    break;
                case SQLMethod.Update:
                    Enable = Attribute.GetCustomAttribute(property, typeof(UpdatableAttribute), true) as EnableAttribute;
                    break;
            }
            if (Enable == null)
            {
                return IsPropertyAllowed(property.Name, _includeSplit, _excludeSplit);
            }

            if (_includeSplit.Contains(property.Name, StringComparer.OrdinalIgnoreCase))
            {
                return true;
            }
            if (_excludeSplit.Contains(property.Name, StringComparer.OrdinalIgnoreCase))
            {
                return false;
            }
            return Enable.Enable;
        }

    }

    public enum SQLMethod
    {
        Select = 0,
        Insert = 1,
        Update = 2,
        Delete = 3
    }
}
