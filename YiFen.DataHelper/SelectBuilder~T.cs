using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.ComponentModel;

namespace YiFen.DataHelper
{
    public class SelectBuilder<T>
        where T : IEntity
    {
        IDbQuery dbQuery = DataUtility.GetDbQuery();

        public List<IDataParameter> DataParameters { get; set; }

        public Type EntityType { get; set; }

        public List<string> Fields { get; set; }

        public string Table { get; set; }

        public List<RelationInfo> Relation { get; set; }

        public StringBuilder Condition { get; set; }

        public StringBuilder Group { get; set; }

        public StringBuilder Sort { get; set; }

        public int LimitOffset { get; set; }

        public int LimitRows { get; set; }

        public SelectBuilder()
        {
            EntityType = typeof(T);
            Relation = new List<RelationInfo>();
            Fields = new List<string>();
            Condition = new StringBuilder();
            Group = new StringBuilder();
            Sort = new StringBuilder();
            DataParameters = new List<IDataParameter>();
        }

        public SelectBuilder<T> Select(params string[] fields)
        {
            this.Fields.AddRange(fields);
            return this;
        }

        public SelectBuilder<T> From(string tableName)
        {
            this.Table = tableName;
            return this;
        }

        public SelectBuilder<T> Join(string propertyName, object on, string include, string exclude, RelationEnum join = RelationEnum.InnerJoin)
        {
            Type propertyType = this.EntityType;
            foreach (string item in propertyName.SplitString('.'))
            {
                propertyType = propertyType.GetProperty(item).PropertyType;
            }
            string joinTableName = propertyType.GetTableName();
            List<string> joinFields = propertyType.GetSelectValidFields(include, exclude, propertyName);
            this.Fields.AddRange(joinFields);

            StringBuilder onstring = new StringBuilder();
            if (on != null)
            {
                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(on))
                {
                    if (onstring.Length > 0)
                    {
                        onstring.Append(" AND ");
                    }
                    onstring.Append(descriptor.Name).Append("=").Append(descriptor.GetValue(on));
                }
            }
            return this.Join(DataUtility.GetDbQuery().AddSeparator(joinTableName), on.ToString(), include, exclude, join);
        }
        public SelectBuilder<T> Join(string propertyName, string on, string include, string exclude, RelationEnum join = RelationEnum.InnerJoin)
        {
            Type propertyType = this.EntityType;
            foreach (string item in propertyName.SplitString('.'))
            {
                propertyType = propertyType.GetProperty(item).PropertyType;
            }
            string joinTableName = propertyType.GetTableName();
            List<string> joinFields = propertyType.GetSelectValidFields(include, exclude, propertyName);
            this.Fields.AddRange(joinFields);

            return this.Join(DataUtility.GetDbQuery().AddSeparator(joinTableName), on, join);
        }
        public SelectBuilder<T> Join(string joinTableName, string on, RelationEnum join = RelationEnum.InnerJoin)
        {
            StringBuilder onString = new StringBuilder();
            this.Relation.Add(new RelationInfo() { TableName = joinTableName, On = on, Relation = join });
            return this;
        }

        public SelectBuilder<T> Where(object where)
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
                    IDataParameter pram = dbQuery.NewDataParameter();
                    pram.ParameterName = parmName;
                    pram.Value = descriptor.GetValue(where);
                    this.DataParameters.Add(pram);
                }
            }
            return this;
        }
        public SelectBuilder<T> Where(string whereString, object parameters = null)
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

        public SelectBuilder<T> OrderBy(string orderByString)
        {
            this.Sort.Append(orderByString);
            return this;
        }
        public SelectBuilder<T> GroupBy(string groupByString)
        {
            this.Group.Append(groupByString);
            return this;
        }

        public SelectBuilder<T> CleanFields()
        {
            this.Fields = new List<string>();
            return this;
        }
        public SelectBuilder<T> CleanSort()
        {
            this.Group = new StringBuilder();
            return this;
        }
        public SelectBuilder<T> CleanCondition()
        {
            this.Condition = new StringBuilder();
            return this;
        }
        public SelectBuilder<T> CleanGroup()
        {
            this.Group = new StringBuilder();
            return this;
        }
        public SelectBuilder<T> CleanRelation()
        {
            this.Relation = new List<RelationInfo>();
            return this;
        }

        public override string ToString()
        {
            return dbQuery.GetString(this);
        }

        public SQLInfo ToSQLInfo()
        {
            return Query.Select(this.ToString(), DataParameters);
        }
    }
}
