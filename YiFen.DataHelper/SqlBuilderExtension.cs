using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Reflection;
using System.Collections.Specialized;
using YiFen.Core;
using System.Data;
using System.ComponentModel;

namespace YiFen.DataHelper
{
    public static class SQLBuilderExtension
    {


        public static SelectBuilder Select(this SelectBuilder sqlBuilder, string selectString)
        {
            if (sqlBuilder.Select.Length != 0 & !string.IsNullOrEmpty(selectString))
            {
                sqlBuilder.Select.Append(",");
            }
            sqlBuilder.Select.Append(selectString);
            return sqlBuilder;
        }
        public static SelectBuilder Select(this SelectBuilder sqlBuilder, params string[] fields)
        {
            return sqlBuilder.Select(string.Join(",", fields));
        }
        public static SelectBuilder Select<T>(this SelectBuilder sqlBuilder, string include = null, string exclude = null)
            where T : IEntity
        {
            sqlBuilder.EntityType = typeof(T);
            string tableName = GetTableName(sqlBuilder.EntityType);
            return sqlBuilder.Select(GetValidFieldsForSelect(sqlBuilder.EntityType, include, exclude, tableName))
                .From(SQLHelper.AddSeparator(tableName));
        }

        public static SelectBuilder From(this SelectBuilder sqlBuilder, string tableName)
        {
            sqlBuilder.From = tableName;
            return sqlBuilder;
        }
        public static SelectBuilder From<T>(this SelectBuilder sqlBuilder, string include = null, string exclude = null)
            where T : IEntity
        {
            return sqlBuilder.Select<T>(include, exclude);
        }

        public static SelectBuilder Join(this SelectBuilder sqlBuilder, string joinTableName, object on, JoinEnum join = JoinEnum.InnerJoin)
        {
            StringBuilder onString = new StringBuilder();
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(on))
            {
                if (sqlBuilder.Where.Length > 0)
                {
                    sqlBuilder.Where.Append(" AND ");
                }
                onString.Append(descriptor.Name).Append("=").Append(descriptor.GetValue(on));
            }
            sqlBuilder.Join.Add(new JoinInfo() { TableName = joinTableName, On = onString.ToString(), Join = join });
            return sqlBuilder;
        }
        public static SelectBuilder Join(this SelectBuilder sqlBuilder, string propertyName, object on, string include, string exclude, JoinEnum join = JoinEnum.InnerJoin)
        {
            Type propertyType = sqlBuilder.EntityType;
            foreach (string item in propertyName.SplitString('.'))
            {
                propertyType = propertyType.GetProperty(item).PropertyType;
            }
            string joinTableName = GetTableName(propertyType);
            string joinSelectString = GetValidFieldsForSelect(propertyType, include, exclude, propertyName);
            sqlBuilder.Select(joinSelectString);
            return sqlBuilder.Join(DataUtility.GetDbQuery().AddSeparator(joinTableName), on, join);
        }

        public static SelectBuilder Where(this SelectBuilder sqlBuilder, string whereString, object parameters = null)
        {
            return sqlBuilder;
        }
        public static SelectBuilder Where(this SelectBuilder sqlBuilder, object where)
        {
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(where))
            {
                string parmName = "@" + Guid.NewGuid().ToString();
                if (sqlBuilder.Where.Length > 0)
                {
                    sqlBuilder.Where.Append(" AND ");
                }
                sqlBuilder.Where.Append(descriptor.Name).Append("=").Append(parmName);
                IDataParameter pram = SQLHelper.NewDataParameter();
                pram.ParameterName = parmName;
                pram.Value = descriptor.GetValue(where);
                sqlBuilder.DataParameters.Add(pram);
            }
            return sqlBuilder;
        }

        public static SelectBuilder OrderBy(this SelectBuilder sqlBuilder, string orderByString)
        {
            sqlBuilder.OrderBy.Append(orderByString);
            return sqlBuilder;
        }
        public static SelectBuilder GroupBy(this SelectBuilder sqlBuilder, string groupByString)
        {
            sqlBuilder.GroupBy.Append(groupByString);
            return sqlBuilder;
        }

        public static IDataReader GetReader(this SelectBuilder sqlBuilder)
        {
            return null;
        }
        public static IDataReader GetReader(this SelectBuilder sqlBuilder, int offset, int rows)
        {
            return null;
        }
        public static IDataReader GetReader(this SelectBuilder sqlBuilder, PageInfo pageInfo)
        {
            return null;
        }
        public static T ToEntity<T>(this SelectBuilder sqlBuilder)
            where T : IEntity, new()
        {
            return sqlBuilder.GetReader().ToEntity<T>();
        }
        public static IList<T> ToList<T>(this SelectBuilder sqlBuilder, int offset, int rows)
            where T : new()
        {
            return sqlBuilder.GetReader().ToList<T>();
        }
        public static IPagingList<T> ToPagingList<T>(this SelectBuilder sqlBuilder, PageInfo pageInfo)
            where T : new()
        {
            IList<T> list = sqlBuilder.ToList<T>((pageInfo.Page - 1) * pageInfo.PageSize, pageInfo.PageSize);
            return null;
        }

        public static string GetTableName(Type entityType)
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
        public static string GetValidFieldsForSelect(Type entityType, string include, string exclude, string prefix = null)
        {
            string[] excludeSplit = exclude.SplitString();
            string[] includeSplit = include.SplitString();

            StringBuilder selectBuilder = new StringBuilder();
            foreach (PropertyInfo property in entityType.GetProperties())
            {
                if (IsPropertyAllowed(property, SQLMethod.Select, includeSplit, excludeSplit))
                {
                    selectBuilder.Append(SQLHelper.AddSeparator(GetTableName(entityType)))
                        .Append(".")
                        .Append(SQLHelper.AddSeparator(GetFieldName(property)))
                        .Append(" AS ")
                        .Append(SQLHelper.AddSeparator(GetFieldAsName(property, prefix)))
                        .Append(",");
                }
            }
            if (selectBuilder.Length > 1) selectBuilder.Remove(selectBuilder.Length - 1, 1);
            return selectBuilder.ToString();
        }

        static string GetFieldName(PropertyInfo property)
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
        static string GetFieldAsName(PropertyInfo property, string prefix = null)
        {
            string asName = string.IsNullOrEmpty(prefix) ? string.Empty : prefix + ".";
            asName = asName + "." + property.Name;
            if (property.PropertyType.GetInterface("IEntity") != null)
            {
                asName = asName + ".ID";
            }
            return asName;
        }
        static string[] SplitString(this string original, char c = ',')
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
        static bool IsPropertyAllowed(string propertyName, string[] includeProperties, string[] excludeProperties)
        {
            // We allow a property to be bound if its both in the include list AND not in the exclude list.
            // An empty include list implies all properties are allowed.
            // An empty exclude list implies no properties are disallowed.
            bool includeProperty = (includeProperties == null) || (includeProperties.Length == 0) || includeProperties.Contains(propertyName, StringComparer.OrdinalIgnoreCase);
            bool excludeProperty = (excludeProperties != null) && excludeProperties.Contains(propertyName, StringComparer.OrdinalIgnoreCase);
            return includeProperty && !excludeProperty;
        }
        static bool IsPropertyAllowed(PropertyInfo property, SQLMethod SQLMethod, string[] _includeSplit, string[] _excludeSplit)
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
        enum SQLMethod
        {
            Select = 0,
            Insert = 1,
            Update = 2,
            Delete = 3
        }
    }
}
