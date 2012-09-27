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
    public static class Query
    {
        public static SQLInfo Select(string SQLString, object parameters = null)
        {
            SQLInfo sqlInfo = new SQLInfo();
            sqlInfo.SQLString = SQLString;
            if (parameters != null)
            {
                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(parameters))
                {
                    IDataParameter pram = DataUtility.GetDbQuery().NewDataParameter();
                    pram.ParameterName = "@" + parameters;
                    pram.Value = descriptor.GetValue(parameters);
                    sqlInfo.DataParameters.Add(pram);
                }
            }
            return sqlInfo;
        }
        public static SQLInfo Select(string SQLString, List<IDataParameter> parameters = null)
        {
            SQLInfo sqlInfo = new SQLInfo();
            sqlInfo.SQLString = SQLString;
            sqlInfo.DataParameters = parameters;
            return sqlInfo;
        }

        public static SelectBuilder<T> Select<T>(string include = null, string exclude = null)
            where T : IEntity
        {
            SelectBuilder<T> sqlBuilder = new SelectBuilder<T>();
            sqlBuilder.Fields.AddRange(sqlBuilder.EntityType.GetSelectValidFields(include, exclude));
            sqlBuilder.Table = DataUtility.GetDbQuery().AddSeparator(sqlBuilder.EntityType.GetTableName());
            return sqlBuilder;
        }
        public static SelectBuilder<T> Select<T>(string fields)
            where T : IEntity
        {
            SelectBuilder<T> sqlBuilder = new SelectBuilder<T>();
            sqlBuilder.Table = DataUtility.GetDbQuery().AddSeparator(sqlBuilder.EntityType.GetTableName());
            return sqlBuilder.Select(fields);
        }

        public static UpdateBuilder<T> Update<T>(T entity, string include = null, string exclude = null)
            where T : IEntity
        {
            UpdateBuilder<T> updateBuilder = new UpdateBuilder<T>();
            return updateBuilder;
        }

        public static IDataReader ExecuteReader(this SQLInfo sqlInfo, string connectionString = null)
        {
            IDbConnection conn = null;

            try
            {
                conn = DataUtility.GetDbQuery().NewDbConnection();
                conn.ConnectionString = string.IsNullOrEmpty(connectionString) ? ConfigurationManager.ConnectionStrings["DataHelperAdapter"].ToString() : connectionString;
                conn.Open();
                IDbCommand cmd = conn.CreateCommand();
                cmd.CommandText = sqlInfo.SQLString;
                if (sqlInfo.DataParameters != null)
                {
                    foreach (IDataParameter item in sqlInfo.DataParameters)
                    {
                        cmd.Parameters.Add(item);
                    }
                }
                return cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch
            {
                if (conn != null)
                {
                    if (conn.State == ConnectionState.Open) conn.Close();
                    conn.Dispose();
                }
                throw;
            }
        }
        public static object ExecuteScalar(this SQLInfo sqlInfo, string connectionString = null)
        {
            IDbConnection conn = null;
            try
            {
                conn = DataUtility.GetDbQuery().NewDbConnection();
                conn.ConnectionString = string.IsNullOrEmpty(connectionString) ? ConfigurationManager.AppSettings["DataHelperAdapter"] : connectionString;
                conn.Open();
                IDbCommand cmd = conn.CreateCommand();
                cmd.CommandText = sqlInfo.SQLString;
                if (sqlInfo.DataParameters != null)
                {
                    foreach (IDataParameter item in sqlInfo.DataParameters)
                    {
                        cmd.Parameters.Add(item);
                    }
                }
                return cmd.ExecuteScalar();
            }
            finally
            {
                if (conn != null)
                {
                    if (conn.State == ConnectionState.Open) conn.Close();
                    conn.Dispose();
                }
            }

        }
        public static int ExecuteNonQuery(this SQLInfo sqlInfo, string connectionString = null)
        {
            IDbConnection conn = null;
            try
            {
                conn = DataUtility.GetDbQuery().NewDbConnection();
                conn.ConnectionString = string.IsNullOrEmpty(connectionString) ? ConfigurationManager.AppSettings["DataHelperAdapter"] : connectionString;
                conn.Open();
                IDbCommand cmd = conn.CreateCommand();
                cmd.CommandText = sqlInfo.SQLString;
                if (sqlInfo.DataParameters != null)
                {
                    foreach (IDataParameter item in sqlInfo.DataParameters)
                    {
                        cmd.Parameters.Add(item);
                    }
                }
                return cmd.ExecuteNonQuery();
            }
            finally
            {
                if (conn != null)
                {
                    if (conn.State == ConnectionState.Open) conn.Close();
                    conn.Dispose();
                }
            }
        }

        public static IDataReader GetReader<T>(this SelectBuilder<T> sqlBuilder, int rows, int offset = 0, string connectionString = null)
            where T : IEntity
        {
            sqlBuilder.LimitOffset = offset;
            sqlBuilder.LimitRows = rows;
            return sqlBuilder.ToSQLInfo().ExecuteReader(connectionString);
        }
        public static IDataReader GetReader<T>(this SelectBuilder<T> sqlBuilder, PageInfo pageInfo, string connectionString = null)
            where T : IEntity
        {
            return sqlBuilder.GetReader(pageInfo.PageSize, (pageInfo.Page - 1), connectionString);
        }

        public static object ToObject<T>(this SelectBuilder<T> sqlBuilder, string connectionString = null)
            where T : IEntity, new()
        {
            return sqlBuilder.ToSQLInfo().ExecuteScalar(connectionString);
        }
        public static T ToEntity<T>(this SelectBuilder<T> sqlBuilder, string connectionString = null)
            where T : IEntity, new()
        {
            return sqlBuilder.GetReader(1, 0, connectionString).ToEntity<T>();
        }
        public static IList<T> ToList<T>(this SelectBuilder<T> sqlBuilder, int rows = 0, int offset = 0, string connectionString = null)
            where T : IEntity, new()
        {
            return sqlBuilder.GetReader(rows, offset, connectionString).ToList<T>();
        }
        public static IPagingList<T> ToPagingList<T>(this SelectBuilder<T> sqlBuilder, PageInfo pageInfo, string connectionString = null)
            where T : IEntity, new()
        {
            IList<T> list = sqlBuilder.ToList<T>((pageInfo.Page - 1) * pageInfo.PageSize, pageInfo.PageSize, connectionString);
            sqlBuilder.CleanFields().Select("COUNT(*)").CleanSort();
            int count;
            int.TryParse(sqlBuilder.ToObject(connectionString).ToString(), out count);
            return list.ToPagingList(pageInfo.Page, pageInfo.PageSize, count);
        }
    }
}
