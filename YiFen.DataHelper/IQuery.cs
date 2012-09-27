using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YiFen.Core;
using System.Data;
using System.Collections.Specialized;

namespace YiFen.DataHelper
{
    public interface IQuery
    {
        string SQLOutPutTxt { get; set; }
        string ConnectionString { get; set; }

        List<IDataParameter> DataParameters { get; set; }
        CommandType CommandType { get; set; }

        IDataParameter NewIDataParameter(string Name, object Value);

        IDataReader ExecuteReader(string SQLText);

        RecordCollection ExecuteRecord(string SQLText);

        int ExecuteNonQuery(string SQLText);

        object ExecuteScalar(string SQLText);
    }

    public interface IQuery<T> where T : IEntity, new()
    {
        string ConnectionString { get; set; }
        string Exclude { get; set; }
        string Include { get; set; }
        string SQLOutPutTxt { get; set; }
        IDataParameter NewIDataParameter(string Name, object Value);
        List<IDataParameter> DataParameters { get; set; }

        T SelectByID<K>(K ID);
        T SelectByID<K>(string SelectJoinString, string JoinString, K ID);
        T Select(string WhereString, string OrderString);
        T Select(string SelectJoinString, string JoinString, string WhereString, string OrderString);

        IList<T> SelectAll();
        IList<T> SelectAll(string WhereString, string OrderString);
        IList<T> SelectAll(string SelectJoinString, string JoinString, string WhereString, string OrderString);
        IList<T> SelectAll(string SelectJoinString, string JoinString, string WhereString, string OrderString, int Offset, int Count);

        IPagingList<T> SelectAll(string WhereString, string OrderString, PageInfo pageInfo);
        IPagingList<T> SelectAll(string SelectJoinString, string JoinString, string WhereString, string OrderString, PageInfo pageInfo);

        int GetCount();
        int GetCount(string WhereString);
        int GetCount(string JoinString, string WhereString);

        int Insert(T Entity);

        int Update(T Entity);
        int Update(T Entity, string WhereString);
        int Update(string UpdateString, string WhereString);

        int Delete<K>(K ID);
        int Delete<K>(K[] IDs);
        int Delete(string WhereString);

        string GetSelectString();
        string GetSelectString(string SelectJoinString, string JoinString);
        string GetInsertString();
        string GetUpdateString();
    }
}
