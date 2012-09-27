using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using Microsoft.ApplicationBlocks.Data;
using System.Data;
using YiFen.Core;
using YiFen.DataHelper;

namespace YiFen.DataHelper.QueryAdapter
{
    public class SQL2005DbQuery : IDbQuery
    {
        #region IDbQuery 成员

        public string ConnectionString { get; set; }

        public IDbConnection NewDbConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        public IDbDataParameter NewDataParameter()
        {
            return new SqlParameter();
        }

        public IDbCommand NewDbCommand()
        {
            return new SqlCommand();
        }

        public IDataReader ExecuteReader(IDbTransaction Transaction, string SQLText, IDataParameter[] DataParameter, CommandType CommandType)
        {
            return SqlHelper.ExecuteReader((SqlTransaction)Transaction, CommandType, SQLText,
                DataParameter == null ? null : (SqlParameter[])DataParameter);
        }

        public int ExecuteNonQuery(IDbTransaction Transaction, string SQLText, IDataParameter[] DataParameter, CommandType CommandType)
        {
            return SqlHelper.ExecuteNonQuery((SqlTransaction)Transaction, CommandType, SQLText,
                DataParameter == null ? null : (SqlParameter[])DataParameter);
        }

        public object ExecuteScalar(IDbTransaction Transaction, string SQLText, IDataParameter[] DataParameter, CommandType CommandType)
        {
            return SqlHelper.ExecuteScalar((SqlTransaction)Transaction, CommandType, SQLText,
                DataParameter == null ? null : (SqlParameter[])DataParameter);
        }

        public string GetString<T>(SelectBuilder<T> selectBuilder)
            where T : IEntity
        {
            StringBuilder sqlBuilder = new StringBuilder(" SELECT ");
            if (selectBuilder.LimitOffset == 0 && selectBuilder.LimitRows > 0)
            { sqlBuilder.Append(" TOP ").Append(selectBuilder.LimitRows).Append("  "); }
            sqlBuilder.Append(string.Join(",", selectBuilder.Fields));
            if (selectBuilder.LimitOffset > 0)
            {
                sqlBuilder.Append(", ROW_NUMBER() OVER (ORDER  BY ");
                if (selectBuilder.Sort.Length > 0)
                {
                    sqlBuilder.Append(selectBuilder.Sort);
                }
                else
                {
                    sqlBuilder.Append(selectBuilder.Table).Append(".").Append("[ID]");
                }
                sqlBuilder.Append(" ) as [RowNumber06474106A96A42DC9AA771668C8927D0] ");
            }
            sqlBuilder.Append(" FROM ").Append(selectBuilder.Table).Append(" ");

            if (selectBuilder.Relation.Count > 0) sqlBuilder.Append(GetJoinString(selectBuilder.Relation));
            if (selectBuilder.Condition.Length > 0) sqlBuilder.Append(" WHERE ").Append(selectBuilder.Condition);
            if (selectBuilder.Group.Length > 0) sqlBuilder.Append(" GROUP BY ").Append(selectBuilder.Group);
            if (selectBuilder.LimitOffset == 0 && selectBuilder.Sort.Length > 0)
            { sqlBuilder.Append(" ORDER BY ").Append(selectBuilder.Sort); }

            if (selectBuilder.LimitOffset > 0)
            {
                sqlBuilder.Insert(0, " SELECT  * FROM ( ");
                sqlBuilder.Append(" ) as T WHERE RowNumber06474106A96A42DC9AA771668C8927D0 between ")
                    .Append(selectBuilder.LimitOffset + 1).Append(" AND ").Append(selectBuilder.LimitOffset + 1 + selectBuilder.LimitRows);
            }
            return sqlBuilder.ToString();
        }

        StringBuilder GetJoinString(List<RelationInfo> Joins)
        {
            StringBuilder sqlBuilder = new StringBuilder();
            foreach (RelationInfo item in Joins)
            {
                switch (item.Relation)
                {
                    case RelationEnum.InnerJoin:
                        sqlBuilder.Append(" Inner Join ");
                        break;
                    case RelationEnum.NaturalJoin:
                        sqlBuilder.Append(" Natural Join ");
                        break;
                    case RelationEnum.LeftOuterJoin:
                        sqlBuilder.Append(" Left Outer Join ");
                        break;
                    case RelationEnum.RightOuterJoin:
                        sqlBuilder.Append(" Right  Outer  Join ");
                        break;
                    case RelationEnum.FullOuterJoin:
                        sqlBuilder.Append("Full  Outer  Join");
                        break;
                    case RelationEnum.CrossJoin:
                        sqlBuilder.Append(" Cross  Join ");
                        break;
                }
                sqlBuilder.Append(item.TableName);
                if (string.IsNullOrEmpty(item.On)) sqlBuilder.Append(" ON ").Append(item.On);
            }
            return sqlBuilder;
        }

        public string AddSeparator(string Name)
        {
            Name = Name.Trim();
            if (!Name.StartsWith("[")) Name = "[" + Name;
            if (!Name.EndsWith("]")) Name = Name + "]";
            return Name;
        }

        #endregion



    }
}
