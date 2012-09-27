using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YiFen.Core;
using System.Configuration;
using Artech.ILInvoker;

namespace YiFen.DataHelper
{
    public class Service<K> where K : IEntity, new()
    {
        public string OrderString { get; set; }
        public string ConnectionString { get; set; }
        public string SQLOutPutTxtPath { get; set; }
        string SelectJoinString = null;
        string JoinString = null;

        public void SetJoinString(string SelectJoinString, string JoinString)
        {
            this.SelectJoinString = SelectJoinString;
            this.JoinString = JoinString;
        }

        public Service()
        {
            ConnectionStringSettings con = ConfigurationManager.ConnectionStrings["DataHelperConnectionString"];
            if (con != null)
                ConnectionString = con.ToString();
        }

        public virtual K Get<T>(T ID)
        {
            IQuery<K> query = DataUtility.GetAdapter<K>(ConnectionString);
            query.SQLOutPutTxt = SQLOutPutTxtPath;
            return query.SelectByID(SelectJoinString, JoinString, ID);
        }

        public virtual K Get<T>(string FielName, T FielValue)
        {
            IQuery<K> query = DataUtility.GetAdapter<K>(ConnectionString);
            query.SQLOutPutTxt = SQLOutPutTxtPath;
            query.DataParameters.Add(query.NewIDataParameter("@FielName", FielValue));
            return query.Select(SelectJoinString, JoinString, " " + FielName + "=@FielName", OrderString);
        }

        public virtual IList<K> GetAll()
        {
            IQuery<K> query = DataUtility.GetAdapter<K>(ConnectionString);
            query.SQLOutPutTxt = SQLOutPutTxtPath;
            return query.SelectAll(SelectJoinString, JoinString, null, OrderString);
        }

        public virtual IList<K> GetAll<T>(params T[] IDs)
        {
            return GetAll("[" + typeof(K).GetTableName() + "].[ID]", IDs);
        }

        public virtual IList<K> GetAll<T>(string FielName, params T[] FielValues)
        {
            IQuery<K> query = DataUtility.GetAdapter<K>(ConnectionString);
            query.SQLOutPutTxt = SQLOutPutTxtPath;
            string Where = " " + FielName + " IN ('" + string.Join("','", FielValues.Select(x => x.ToString()).ToArray()) + "')";
            return query.SelectAll(SelectJoinString, JoinString, Where, OrderString);
        }

        public virtual IPagingList<K> GetPaging(PageInfo pageInfo)
        {
            IQuery<K> query = DataUtility.GetAdapter<K>(ConnectionString);
            query.SQLOutPutTxt = SQLOutPutTxtPath;
            return query.SelectAll(SelectJoinString, JoinString, "", OrderString, pageInfo);
        }

        public virtual IPagingList<K> GetPaging(string FielName, object FielValue, PageInfo pageInfo)
        {
            IQuery<K> query = DataUtility.GetAdapter<K>(ConnectionString);
            query.SQLOutPutTxt = SQLOutPutTxtPath;
            query.DataParameters.Add(query.NewIDataParameter("@FielName", FielValue));
            return query.SelectAll(SelectJoinString, JoinString, " " + FielName + "=@FielName", OrderString, pageInfo);
        }

        public virtual int Insert(K entity)
        {
            int i = 0;
            IQuery<K> query = DataUtility.GetAdapter<K>(ConnectionString);
            query.SQLOutPutTxt = SQLOutPutTxtPath;
            i = query.Insert(entity);
            return i;
        }

        public virtual int Update(K entity)
        {
            int i = 0;
            IQuery<K> query = DataUtility.GetAdapter<K>(ConnectionString);
            query.SQLOutPutTxt = SQLOutPutTxtPath;
            i = query.Update(entity);
            return i;
        }

        public virtual int Update(K entity, string include, string exclude)
        {
            int i = 0;
            IQuery<K> query = DataUtility.GetAdapter<K>(ConnectionString);
            query.SQLOutPutTxt = SQLOutPutTxtPath;
            query.Include = include;
            query.Exclude = exclude;
            i = query.Update(entity);
            return i;
        }

        public virtual int Update(K entity, string include, string exclude, string WhereString)
        {
            int i = 0;
            IQuery<K> query = DataUtility.GetAdapter<K>(ConnectionString);
            query.Include = include;
            query.SQLOutPutTxt = SQLOutPutTxtPath;
            query.Exclude = exclude;
            i = query.Update(entity, WhereString);
            return i;
        }

        public virtual int Update(string UpdateString, string WhereString)
        {
            int i = 0;
            IQuery<K> query = DataUtility.GetAdapter<K>(ConnectionString);
            query.SQLOutPutTxt = SQLOutPutTxtPath;
            i = query.Update(UpdateString, WhereString);
            return i;
        }

        /// <summary>
        /// 根据ID保存内容
        /// 如果ID存在则更新
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual SaveEnum Save(K entity)
        {
            object id = PropertyAccessor.Get(entity, "ID");
            if (id != null && !Get(id).IsDBNull)
            {
                Update(entity);
                return SaveEnum.Update;
            }
            Insert(entity);
            return SaveEnum.Insert;
        }

        public virtual SaveEnum Save(K entity, string include, string exclude)
        {
            object id = PropertyAccessor.Get(entity, "ID");
            if (id != null && !Get(id).IsDBNull)
            {
                Update(entity, include, exclude);
                return SaveEnum.Update;
            }
            Insert(entity);
            return SaveEnum.Insert;
        }

        public virtual int Delete<T>(params T[] IDs)
        {
            int i = 0;
            IQuery<K> query = DataUtility.GetAdapter<K>(ConnectionString);
            query.SQLOutPutTxt = SQLOutPutTxtPath;
            i = query.Delete<T>(IDs);
            return i;
        }

        public virtual int Delete<T>(string FielName, params T[] Values)
        {
            int i = 0;
            IQuery<K> query = DataUtility.GetAdapter<K>(ConnectionString);
            query.SQLOutPutTxt = SQLOutPutTxtPath;
            i = query.Delete(FielName + " IN ('" + string.Join("','", Values.Select(x => x.ToString()).ToArray()) + "')");
            return i;
        }
    }
}
