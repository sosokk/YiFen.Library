using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YiFen.TemplateGenerator;
using Microsoft.ApplicationBlocks.Data;
using System.Data.SqlClient;
using System.Data;

namespace YiFen.TemplateGenerator.GetInformationAdapter
{
    public class GetSQLServerInformation : IGetInformation
    {
        #region IGetInformation 成员

        public List<TableInfo> Tables(string ConnectionString)
        {
            List<TableInfo> Tables = new List<TableInfo>();
            string sql = "select name from sysobjects where type='U'";
            using (SqlDataReader rd = SqlHelper.ExecuteReader(ConnectionString, CommandType.Text, sql))
            {
                while (rd.Read())
                {
                    TableInfo table = GetTableInfo(ConnectionString, rd.GetString(0));
                    Tables.Add(table);
                }
            }
            return Tables;
        }

        #endregion

        TableInfo GetTableInfo(string ConnectionString, string TableName)
        {
            TableInfo table = new TableInfo();
            table.Name = TableName;
            string sql = "SELECT TOP 1 * FROM [" + TableName + "]";
            using (SqlDataReader rd = SqlHelper.ExecuteReader(ConnectionString, CommandType.Text, sql))
            {
                for (int i = 0; i < rd.FieldCount; i++)
                {
                    FieldInfo field = new FieldInfo();
                    field.Name = rd.GetName(i);
                    field.DbType = rd.GetFieldType(i);
                    table.Fields.Add(field);
                }
            }
            return table;
        }
    }
}
