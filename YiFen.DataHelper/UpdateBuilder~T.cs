using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.ComponentModel;

namespace YiFen.DataHelper
{
    public class UpdateBuilder<T>
        where T : IEntity
    {
        IDbQuery dbQuery = DataUtility.GetDbQuery();

        public List<IDataParameter> DataParameters { get; set; }

        public Type EntityType { get; set; }

        public string Table { get; set; }

        public List<string> Fields { get; set; }

        public StringBuilder Condition { get; set; }

        public UpdateBuilder()
        {
            EntityType = typeof(T);
            Fields = new List<string>();
            Condition = new StringBuilder();
        }

        public UpdateBuilder<T> Update(string tableName)
        {
            this.Table = tableName;
            return this;
        }
        public UpdateBuilder<T> Where(object where)
        {
            if (where != null)
            {
                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(where))
                {
                    string parmName = "@" + Guid.NewGuid().ToString();
                    if (this.Condition.Length > 0)
                    {
                        this.Condition.Append(" AND ");
                    }
                    this.Condition.Append(descriptor.Name).Append("=").Append(parmName);
                    IDataParameter pram = DataUtility.GetDbQuery().NewDataParameter();
                    pram.ParameterName = parmName;
                    pram.Value = descriptor.GetValue(where);
                    this.DataParameters.Add(pram);
                }
            }
            return this;
        }
        public UpdateBuilder<T> Where(string whereString, object parameters = null)
        {
            if (parameters != null)
            {
                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(parameters))
                {
                    IDataParameter pram = DataUtility.GetDbQuery().NewDataParameter();
                    pram.ParameterName = "@" + descriptor;
                    pram.Value = descriptor.GetValue(parameters);
                    this.DataParameters.Add(pram);
                }
            }
            this.Condition.Append(whereString);
            return this;
        }
        public UpdateBuilder<T> Set(object fields)
        {
            if (fields != null)
            {
                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(fields))
                {
                    string parmName = "@" + Guid.NewGuid().ToString();
                    this.Fields.Add(descriptor.Name + "=" + parmName);
                    IDataParameter pram = dbQuery.NewDataParameter();
                    pram.ParameterName = parmName;
                    pram.Value = descriptor.GetValue(fields);
                    this.DataParameters.Add(pram);
                }
            }
            return this;
        }

        public override string ToString()
        {
            return "";
        }

        public SQLInfo ToSQLInfo()
        {
            return Query.Select(this.ToString(), DataParameters);
        }
    }
}
