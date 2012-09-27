using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using YiFen.Core;

namespace YiFen.DataHelper
{
    public interface IDbQuery
    {
        string ConnectionString { get; set; }

        IDbConnection NewDbConnection();
        IDbDataParameter NewDataParameter();
        IDbCommand NewDbCommand();

        IDataReader ExecuteReader(IDbTransaction Transaction, string SQLText, IDataParameter[] DataParameter, CommandType CommandType);
        int ExecuteNonQuery(IDbTransaction Transaction, string SQLText, IDataParameter[] DataParameter, CommandType CommandType);
        object ExecuteScalar(IDbTransaction Transaction, string SQLText, IDataParameter[] DataParameter, CommandType CommandType);

        string GetString<T>(SelectBuilder<T> selectBuilder) where T : IEntity;

        /// <summary>
        /// 为表名和字段名加分隔符
        /// </summary>
        /// <param name="Name">表名或字段名</param>
        /// <returns>加了分隔符的字段名货表名的</returns>
        string AddSeparator(string Name);
    }
}
