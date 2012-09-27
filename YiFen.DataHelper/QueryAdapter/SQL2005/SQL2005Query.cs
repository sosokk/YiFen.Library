using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Specialized;
using YiFen.Core;
using Microsoft.ApplicationBlocks.Data;
using System.Reflection;
using System.IO;

namespace YiFen.DataHelper.QueryAdapter
{
    public class SQL2005Query : IQuery
    {
        public SQL2005Query()
        {
            CommandType = CommandType.Text;
            DataParameters = new List<IDataParameter>();
        }
        public string SQLOutPutTxt { get; set; }
        public string ConnectionString { get; set; }

        public List<IDataParameter> DataParameters { get; set; }

        public CommandType CommandType { get; set; }

        public IDataParameter NewIDataParameter(string Name, object Value)
        {
            return new SqlParameter(Name, Value);
        }

        public IDataReader ExecuteReader(string SQLText)
        {
            SaveSQLText(SQLText);
            return SqlHelper.ExecuteReader(ConnectionString
                , CommandType
                , SQLText
                , DataParameters.Select(x => x as SqlParameter).ToArray()
                );
        }

        public RecordCollection ExecuteRecord(string SQLText)
        {
            RecordCollection rcc = new RecordCollection();
            using (IDataReader rd = ExecuteReader(SQLText))
            {
                while (rd.Read())
                {
                    Record rc = new Record();
                    for (int i = 0; i < rd.FieldCount; i++)
                    {
                        rc.Add(rd.GetName(i), rd.GetValue(i));
                    }
                    rcc.Add(rc);
                }
            }
            return rcc;
        }

        public int ExecuteNonQuery(string SQLText)
        {
            SaveSQLText(SQLText);
            return SqlHelper.ExecuteNonQuery(ConnectionString
                , CommandType
                , SQLText
                , DataParameters.Select(x => x as SqlParameter).ToArray()
                );
        }

        public object ExecuteScalar(string SQLText)
        {
            SaveSQLText(SQLText);
            return SqlHelper.ExecuteScalar(ConnectionString
                , CommandType
                , SQLText
                , DataParameters.Select(x => x as SqlParameter).ToArray()
                );
        }

        void SaveSQLText(string SQLText)
        {
            if (!string.IsNullOrEmpty(SQLOutPutTxt))
            {
                StringBuilder sqlBulider = DataParameters.ToText();
                sqlBulider.Append("CommandType:").Append(CommandType).Append("\r\n");
                sqlBulider.Append("SQLText:").Append(SQLText).Append("\r\n");
                sqlBulider.Append("Time:").Append(DateTime.Now).Append("\r\n");
                File.AppendAllText(SQLOutPutTxt, sqlBulider.ToString());
            }
        }
    }

    public class SQL2005Query<T> : IQuery<T> where T : IEntity, new()
    {
        public SQL2005Query()
        {
            DataParameters = new List<IDataParameter>();
        }

        #region IQuery<T> 辅助方法

        Type entityType = typeof(T);
        Type baseentityType = typeof(Entity);

        private string _exclude;
        private string[] _excludeSplit = new string[0];
        private string _include;
        private string[] _includeSplit = new string[0];

        string GetTableName()
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
        string GetPropertyName(PropertyInfo Property)
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
        string GetPropertyASName(PropertyInfo Property)
        {
            if (Property.PropertyType.GetInterface("IEntity") != null)
            {
                return Property.Name + ".ID";
            }
            return Property.Name;
        }
        string[] SplitString(string original)
        {
            if (String.IsNullOrEmpty(original))
            {
                return new string[0];
            }

            var split = from piece in original.Split(',')
                        let trimmed = piece.Trim()
                        where !String.IsNullOrEmpty(trimmed)
                        select trimmed;
            return split.ToArray();
        }
        bool IsPropertyAllowed(string propertyName, string[] includeProperties, string[] excludeProperties)
        {
            // We allow a property to be bound if its both in the include list AND not in the exclude list.
            // An empty include list implies all properties are allowed.
            // An empty exclude list implies no properties are disallowed.
            bool includeProperty = (includeProperties == null) || (includeProperties.Length == 0) || includeProperties.Contains(propertyName, StringComparer.OrdinalIgnoreCase);
            bool excludeProperty = (excludeProperties != null) && excludeProperties.Contains(propertyName, StringComparer.OrdinalIgnoreCase);
            return includeProperty && !excludeProperty;
        }

        List<PropertyInfo> GetAllowedProperties(PropertyInfo[] Properties, SQLMethod SQLMethod)
        {
            List<PropertyInfo> AllowedProperties = new List<PropertyInfo>();
            if (Properties != null && Properties.Length > 0)
            {
                foreach (PropertyInfo property in Properties)
                {
                    if (IsPropertyAllowed(property, SQLMethod))
                    {
                        AllowedProperties.Add(property);
                    }
                }
            }
            return AllowedProperties;
        }
        bool IsPropertyAllowed(PropertyInfo Property, SQLMethod SQLMethod)
        {
            EnableAttribute Enable = null;
            switch (SQLMethod)
            {
                case SQLMethod.Select:
                    Enable = Attribute.GetCustomAttribute(Property, typeof(SelectableAttribute), true) as EnableAttribute;
                    break;
                case SQLMethod.Insert:
                    Enable = Attribute.GetCustomAttribute(Property, typeof(InsertableAttribute), true) as EnableAttribute;
                    break;
                case SQLMethod.Update:
                    Enable = Attribute.GetCustomAttribute(Property, typeof(UpdatableAttribute), true) as EnableAttribute;
                    break;
            }
            if (Enable == null)
            {
                return IsPropertyAllowed(Property.Name, _includeSplit, _excludeSplit);
            }

            if (_includeSplit.Contains(Property.Name, StringComparer.OrdinalIgnoreCase))
            {
                return true;
            }
            if (_excludeSplit.Contains(Property.Name, StringComparer.OrdinalIgnoreCase))
            {
                return false;
            }
            return Enable.Enable;
        }

        string GetSelectString(string SelectJoinString, string JoinString, PropertyInfo[] Properties, string TableName)
        {
            StringBuilder SelectString = new StringBuilder("SELECT ");
            foreach (PropertyInfo Property in Properties)
            {
                SelectString
                    .Append(" [").Append(TableName).Append("].")
                    .Append("[").Append(GetPropertyName(Property)).Append("] AS [")
                    .Append(GetPropertyASName(Property)).Append("],");
            }
            if (!string.IsNullOrEmpty(SelectJoinString)) SelectString.Append(SelectJoinString);
            else SelectString.Remove(SelectString.Length - 1, 1);

            SelectString.Append(" FROM [").Append(TableName).Append("] ");
            if (!string.IsNullOrEmpty(JoinString)) SelectString.Append(JoinString);
            return SelectString.ToString();
        }
        string GetSelectString(string SelectJoinString, string JoinString, PropertyInfo[] Properties, string TableName, int TopN)
        {
            StringBuilder SelectString = new StringBuilder("SELECT TOP ");
            SelectString.Append(TopN);
            foreach (PropertyInfo Property in Properties)
            {
                SelectString
                    .Append(" [").Append(TableName).Append("].")
                    .Append("[").Append(GetPropertyName(Property)).Append("] AS [")
                    .Append(GetPropertyASName(Property)).Append("],");
            }
            if (!string.IsNullOrEmpty(SelectJoinString)) SelectString.Append(SelectJoinString);
            else SelectString.Remove(SelectString.Length - 1, 1);

            SelectString.Append(" FROM [").Append(TableName).Append("] ");
            if (!string.IsNullOrEmpty(JoinString)) SelectString.Append(JoinString);
            return SelectString.ToString();
        }
        string GetSelectString(string SelectJoinString, string JoinString, PropertyInfo[] Properties, string WhereString, string OrderString, int Start, int End)
        {
            List<PropertyInfo> properties = Properties.ToList();
            string TableName = GetTableName();
            if (string.IsNullOrEmpty(OrderString))
            {
                PropertyInfo PropertyID = entityType.GetProperty("ID") ?? properties[0];
                OrderString = "[" + TableName + "].[" + GetPropertyName(PropertyID) + "]";
            }
            StringBuilder SelectString = new StringBuilder("SELECT  * FROM ( SELECT ");

            foreach (PropertyInfo Property in Properties)
            {
                SelectString.Append(" [").Append(TableName).Append("].")
                    .Append("[").Append(GetPropertyName(Property)).Append("] AS [")
                    .Append(GetPropertyASName(Property)).Append("],");
            }
            if (!string.IsNullOrEmpty(SelectJoinString)) SelectString.Append(SelectJoinString).Append(",");

            SelectString.Append(" ROW_NUMBER() OVER (ORDER  BY ").Append(OrderString).Append(" ) as [RowNumber06474106A96A42DC9AA771668C8927D0] ").
                Append(" FROM [").Append(TableName).Append("]");
            if (!string.IsNullOrEmpty(JoinString)) SelectString.Append(JoinString);

            if (!string.IsNullOrEmpty(WhereString))
            {
                SelectString.Append(" WHERE ").Append(WhereString);
            }
            SelectString.Append(" ) as T WHERE RowNumber06474106A96A42DC9AA771668C8927D0 between ").Append(Start).Append(" AND ").Append(End);
            return SelectString.ToString();
        }
        string GetInsertString(string[] Fields, string TableName)
        {
            StringBuilder SelectString = new StringBuilder("INSERT INTO  ");
            SelectString.Append("[").Append(TableName).Append("] ");
            SelectString.Append(" ([");
            SelectString.Append(string.Join("],[", Fields));
            SelectString.Append("])  VALUES  (@");
            SelectString.Append(string.Join(",@", Fields));
            SelectString.Append(")");
            return SelectString.ToString();
        }
        string GetUpdateString(string[] Fields, string TableName)
        {
            StringBuilder SelectString = new StringBuilder("UPDATE  ");
            SelectString.Append("[").Append(TableName).Append("]  SET");

            if (Fields.Length == 1)
            {
                SelectString.Append("  [").Append(Fields[0]).Append("]=@").Append(Fields[0]);
            }
            else
            {
                int length = Fields.Length - 1;
                for (int i = 0; i < length; i++)
                {
                    SelectString.Append("  [").Append(Fields[i]).Append("]=@").Append(Fields[i]).Append(", ");
                }
                SelectString.Append("  [").Append(Fields[length]).Append("]=@").Append(Fields[length]);
            }

            return SelectString.ToString();
        }

        #endregion

        enum SQLMethod
        {
            Select = 0,
            Insert = 1,
            Update = 2,
            Delete = 3
        }

        #region IQuery<T> 成员

        public string ConnectionString { get; set; }
        public string Exclude
        {
            get
            {
                return _exclude ?? String.Empty;
            }
            set
            {
                _exclude = value;
                _excludeSplit = SplitString(value);
            }
        }
        public string Include
        {
            get
            {
                return _include ?? String.Empty;
            }
            set
            {
                _include = value;
                _includeSplit = SplitString(value);
            }
        }
        public string SQLOutPutTxt { get; set; }
        public IDataParameter NewIDataParameter(string Name, object Value)
        {
            return new SqlParameter(Name, Value);
        }
        public List<IDataParameter> DataParameters { get; set; }

        public T SelectByID<K>(K ID)
        {
            return SelectByID(null, null, ID);
        }
        public T SelectByID<K>(string SelectJoinString, string JoinString, K ID)
        {
            PropertyInfo propertyID = entityType.GetProperty("ID");
            string IDName = GetPropertyName(propertyID);
            string WhereString = "  [" + GetTableName() + "].[" + IDName + "]=@ID";
            DataParameters = new List<IDataParameter>();
            DataParameters.Add(new SqlParameter("@ID", ID));
            return Select(SelectJoinString, JoinString, WhereString, null);
        }
        public T Select(string WhereString, string OrderString)
        {
            return Select(null, null, WhereString, OrderString);
        }
        public T Select(string SelectJoinString, string JoinString, string WhereString, string OrderString)
        {
            List<PropertyInfo> properties = GetAllowedProperties(entityType.GetProperties(), SQLMethod.Select);
            StringBuilder SQLText = new StringBuilder(GetSelectString(SelectJoinString, JoinString, properties.ToArray(), GetTableName(), 1));
            if (!string.IsNullOrEmpty(WhereString))
            {
                SQLText.Append(" WHERE ").Append(WhereString);
            }
            if (!string.IsNullOrEmpty(OrderString))
            {
                SQLText.Append(" ORDER BY ").Append(OrderString);
            }
            SQL2005Query sql05 = new SQL2005Query();
            sql05.DataParameters = DataParameters;
            sql05.ConnectionString = ConnectionString;
            sql05.SQLOutPutTxt = SQLOutPutTxt;
            return sql05.ExecuteReader(SQLText.ToString()).ToEntity<T>();
        }

        public IList<T> SelectAll()
        {
            return SelectAll(null, null, 0, null, null);
        }
        public IList<T> SelectAll(string WhereString, string OrderString)
        {
            return SelectAll(null, null, 0, WhereString, OrderString);
        }
        public IList<T> SelectAll(string SelectJoinString, string JoinString, string WhereString, string OrderString)
        {
            return SelectAll(SelectJoinString, JoinString, 0, WhereString, OrderString);
        }
        public IList<T> SelectAll(string SelectJoinString, string JoinString, string WhereString, string OrderString, int Offset, int Count)
        {
            if (Offset == 0)
            {
                return SelectAll(SelectJoinString, JoinString, Count, WhereString, OrderString);
            }
            int Start = Offset;
            int End = Count;
            PropertyInfo[] properties = GetAllowedProperties(entityType.GetProperties(), SQLMethod.Select).ToArray();
            string SQLText = GetSelectString(SelectJoinString, JoinString, properties, WhereString, OrderString, Start, End);
            SQL2005Query sql05 = new SQL2005Query();
            sql05.DataParameters = DataParameters;
            sql05.ConnectionString = ConnectionString;
            sql05.SQLOutPutTxt = SQLOutPutTxt;
            return sql05.ExecuteReader(SQLText).ToListEntity<T>();
        }
        private IList<T> SelectAll(string SelectJoinString, string JoinString, int TopN, string WhereString, string OrderString)
        {
            List<PropertyInfo> properties = GetAllowedProperties(entityType.GetProperties(), SQLMethod.Select);
            StringBuilder SQLText = new StringBuilder();
            if (TopN > 0)
            {
                SQLText.Append(GetSelectString(SelectJoinString, JoinString, properties.ToArray(), GetTableName(), TopN));
            }
            else
            {
                SQLText.Append(GetSelectString(SelectJoinString, JoinString, properties.ToArray(), GetTableName()));
            }
            if (!string.IsNullOrEmpty(WhereString))
            {
                SQLText.Append(" WHERE ").Append(WhereString);
            }
            if (!string.IsNullOrEmpty(OrderString))
            {
                SQLText.Append(" ORDER BY ").Append(OrderString);
            }
            SQL2005Query sql05 = new SQL2005Query();
            sql05.DataParameters = DataParameters;
            sql05.ConnectionString = ConnectionString;
            sql05.SQLOutPutTxt = SQLOutPutTxt;
            return sql05.ExecuteReader(SQLText.ToString()).ToListEntity<T>();
        }
        public IPagingList<T> SelectAll(string WhereString, string OrderString, PageInfo pageInfo)
        {
            return SelectAll(null, null, WhereString, OrderString, pageInfo);
        }
        public IPagingList<T> SelectAll(string SelectJoinString, string JoinString, string WhereString, string OrderString, PageInfo pageInfo)
        {
            int TotalItems = GetCount(JoinString, WhereString);
            if (pageInfo.Page == 1)
            {
                return SelectAll(SelectJoinString, JoinString, pageInfo.PageSize, WhereString, OrderString).ToPagingList(pageInfo.Page, pageInfo.PageSize, TotalItems);
            }
            int Start = (pageInfo.Page - 1) * pageInfo.PageSize + 1;
            int End = Start + pageInfo.PageSize - 1;
            PropertyInfo[] properties = GetAllowedProperties(entityType.GetProperties(), SQLMethod.Select).ToArray();
            string SQLText = GetSelectString(SelectJoinString, JoinString, properties, WhereString, OrderString, Start, End);
            SQL2005Query sql05 = new SQL2005Query();
            sql05.DataParameters = DataParameters;
            sql05.ConnectionString = ConnectionString;
            sql05.SQLOutPutTxt = SQLOutPutTxt;
            return sql05.ExecuteReader(SQLText).ToListEntity<T>().ToPagingList(pageInfo.Page, pageInfo.PageSize, TotalItems);
        }
        public int GetCount()
        {
            return GetCount(null);
        }
        public int GetCount(string WhereString)
        {
            return GetCount(null, WhereString);
        }
        public int GetCount(string JoinString, string WhereString)
        {
            StringBuilder SQLText = new StringBuilder("SELECT COUNT(*) FROM");
            SQLText.Append("  [").Append(GetTableName()).Append("] ");
            if (!string.IsNullOrEmpty(JoinString)) SQLText.Append(JoinString);
            if (!string.IsNullOrEmpty(WhereString)) SQLText.Append(" WHERE ").Append(WhereString);
            SQL2005Query sql05 = new SQL2005Query();
            sql05.DataParameters = DataParameters;
            sql05.ConnectionString = ConnectionString;
            sql05.SQLOutPutTxt = SQLOutPutTxt;
            return Convert.ToInt32(sql05.ExecuteScalar(SQLText.ToString()));
        }

        public int Insert(T Entity)
        {
            List<string> Fields = new List<string>();
            foreach (PropertyInfo property in entityType.GetProperties())
            {
                if (IsPropertyAllowed(property, SQLMethod.Insert))
                {
                    string propertyName = GetPropertyName(property);
                    object temo = Artech.ILInvoker.PropertyAccessor.Get(Entity, property.Name);
                    if (temo != null)
                    {
                        SqlParameter p = new SqlParameter();
                        p.ParameterName = "@" + propertyName;
                        if (temo is IEntity)
                        {
                            p.Value = Artech.ILInvoker.PropertyAccessor.Get(temo, "ID");
                        }
                        else
                        {
                            p.Value = temo;
                        }
                        Fields.Add(propertyName);
                        DataParameters.Add(p);
                    }
                }
            }
            string SQLText = GetInsertString(Fields.ToArray(), GetTableName());
            SQL2005Query sql05 = new SQL2005Query();
            sql05.DataParameters = DataParameters;
            sql05.ConnectionString = ConnectionString;
            sql05.SQLOutPutTxt = SQLOutPutTxt;
            return sql05.ExecuteNonQuery(SQLText);
        }

        public int Update(T Entity)
        {
            List<string> Fields = new List<string>();
            foreach (PropertyInfo property in entityType.GetProperties())
            {
                if (IsPropertyAllowed(property, SQLMethod.Update))
                {
                    if (property.Name == "ID") continue;

                    string propertyName = GetPropertyName(property);
                    object temo = Artech.ILInvoker.PropertyAccessor.Get(Entity, property.Name);
                    SqlParameter p = new SqlParameter();
                    p.ParameterName = "@" + propertyName;
                    if (temo != null)
                    {
                        if (temo is IEntity)
                        {
                            p.Value = Artech.ILInvoker.PropertyAccessor.Get(temo, "ID");
                        }
                        else { p.Value = temo; }
                    }
                    else
                    {
                        p.Value = DBNull.Value;
                    }
                    Fields.Add(propertyName);
                    DataParameters.Add(p);
                }
            }

            PropertyInfo propertyID = entityType.GetProperty("ID");
            string IDName = GetPropertyName(propertyID);
            SqlParameter IDp = new SqlParameter();
            IDp.ParameterName = "@" + IDName;
            IDp.Value = propertyID.GetValue(Entity, null);
            DataParameters.Add(IDp);
            string SQLText = GetUpdateString(Fields.ToArray(), GetTableName()) + " WHERE [" + IDName + "]=@" + IDName;
            SQL2005Query sql05 = new SQL2005Query();
            sql05.DataParameters = DataParameters;
            sql05.ConnectionString = ConnectionString;
            sql05.SQLOutPutTxt = SQLOutPutTxt;
            return sql05.ExecuteNonQuery(SQLText);
        }
        public int Update(T Entity, string WhereString)
        {
            List<string> Fields = new List<string>();
            foreach (PropertyInfo property in entityType.GetProperties())
            {
                if (IsPropertyAllowed(property, SQLMethod.Update))
                {
                    string propertyName = GetPropertyName(property);
                    object temo = property.GetValue(Entity, null);
                    SqlParameter p = new SqlParameter();
                    p.ParameterName = "@" + propertyName;
                    if (temo != null)
                    {
                        if (temo is IEntity)
                        {
                            p.Value = Artech.ILInvoker.PropertyAccessor.Get(temo, "ID");
                        }
                        else { p.Value = temo; }
                    }
                    else
                    {
                        p.Value = DBNull.Value;
                    }
                    Fields.Add(propertyName);
                    DataParameters.Add(p);
                }
            }
            string SQLText = GetUpdateString(Fields.ToArray(), GetTableName());
            if (!string.IsNullOrEmpty(WhereString))
            {
                SQLText += " WHERE " + WhereString;
            }
            SQL2005Query sql05 = new SQL2005Query();
            sql05.DataParameters = DataParameters;
            sql05.ConnectionString = ConnectionString;
            sql05.SQLOutPutTxt = SQLOutPutTxt;
            return sql05.ExecuteNonQuery(SQLText);
        }

        public int Update(string UpdateString, string WhereString)
        {
            string SQLText = "UPDATE [" + GetTableName() + "] SET " + UpdateString;
            if (!string.IsNullOrEmpty(WhereString))
            {
                SQLText += " WHERE " + WhereString;
            }
            SQL2005Query sql05 = new SQL2005Query();
            sql05.DataParameters = DataParameters;
            sql05.ConnectionString = ConnectionString;
            sql05.SQLOutPutTxt = SQLOutPutTxt;
            return sql05.ExecuteNonQuery(SQLText);
        }

        public int Delete<K>(K ID)
        {
            if (ID == null)
            {
                return 0;
            }
            string IDName = GetPropertyName(entityType.GetProperty("ID"));
            string WhereString = "[" + IDName + "] =@ID";
            DataParameters.Add(new SqlParameter("@ID", ID));
            return Delete(WhereString);
        }
        public int Delete<K>(K[] IDs)
        {
            if (IDs == null || IDs.Length == 0)
            {
                return 0;
            }
            string WhereString = "[" + GetPropertyName(entityType.GetProperty("ID")) + "] IN";
            WhereString += "('" + string.Join("','", IDs.Select(x => x.ToString()).ToArray()) + "')";
            return Delete(WhereString);
        }
        public int Delete(string WhereString)
        {
            StringBuilder SQLText = new StringBuilder("DELETE  FROM");
            SQLText.Append("  [").Append(GetTableName()).Append("] ");
            if (!string.IsNullOrEmpty(WhereString))
            {
                SQLText.Append(" WHERE ").Append(WhereString);
            }
            SQL2005Query sql05 = new SQL2005Query();
            sql05.DataParameters = DataParameters;
            sql05.ConnectionString = ConnectionString;
            sql05.SQLOutPutTxt = SQLOutPutTxt;
            return sql05.ExecuteNonQuery(SQLText.ToString());
        }

        public string GetSelectString()
        {
            List<PropertyInfo> properties = GetAllowedProperties(entityType.GetProperties(), SQLMethod.Select);
            return GetSelectString(null, null, properties.ToArray(), GetTableName());
        }
        public string GetSelectString(string SelectJoinString, string JoinString)
        {
            List<PropertyInfo> properties = GetAllowedProperties(entityType.GetProperties(), SQLMethod.Select);
            return GetSelectString(SelectJoinString, JoinString, properties.ToArray(), GetTableName());
        }
        public string GetInsertString()
        {
            List<PropertyInfo> properties = GetAllowedProperties(entityType.GetProperties(), SQLMethod.Insert);
            return GetInsertString(properties.Select(x => GetPropertyName(x)).ToArray(), GetTableName()); ;
        }
        public string GetUpdateString()
        {
            List<PropertyInfo> properties = GetAllowedProperties(entityType.GetProperties(), SQLMethod.Update);
            return GetUpdateString(properties.Select(x => GetPropertyName(x)).ToArray(), GetTableName()); ;
        }

        #endregion
    }
}
