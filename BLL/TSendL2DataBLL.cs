using Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;

namespace BLL
{
    /// <summary>
    /// 此版本为为了DB2改的通信程序
    /// 此处修改了sql语句
    /// 1.SQL语句变量的变化
    ///     原来Oracle的变量是desno=:desno
    ///     现在改为desno=?来代表变量
    /// 2.DB2的时间好像哟所变化
    /// </summary>
    public class TSendL2DataBLL
    {
        // 涉密删除
        private static string Conn1 = ConfigurationManager.ConnectionStrings["conn1"].ConnectionString;
        public Action<string,int> ShowMsg { get; set; }
        public TSendL2DataBLL(Action<string, int> showMsg)
        {
            this.ShowMsg = showMsg;
        }
        public List<TSendL2DataModel> ToList() 
        {
            List<TSendL2DataModel> sendL2DataModels = new List<TSendL2DataModel>();
            
            try
            {
                OleDbConnection con = new OleDbConnection(Conn1);
                con.Open();
                DataSet ds = new DataSet();
                OleDbCommand cmd_Select = new OleDbCommand(SELECT_SQL, con);
                OleDbDataAdapter oleDbDataAdapter = new OleDbDataAdapter(cmd_Select);
                oleDbDataAdapter.Fill(ds);
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    object[] objectList = ds.Tables[0].Rows[i].ItemArray;
                    TSendL2DataModel sendL2DataModel = new TSendL2DataModel();
                    // 涉密删除
                    sendL2DataModels.Add(sendL2DataModel);
                }
                //foreach (DataRow item in ds.Tables[0].Rows)
                //{
                //    TSendL2DataModel sendL2DataModel = new TSendL2DataModel();
                //    sendL2DataModel.Desno=item[0].
                //} 
            }
            catch (Exception ex)
            {

                ShowMsg(ex.ToString(),0);
            }
            return sendL2DataModels;
        }
        /// <summary>
        /// 创建一个默认的电文头
        /// </summary>
        /// <returns></returns>
        public Header GenerateHead()
        {
            Header head = new Header();
            // 涉密删除
            return head;
        }

        public int Update(TSendL2DataModel sendL2DataModel)
        {
            try
            {
                OleDbConnection con = new OleDbConnection(Conn1);
                con.Open();
                OleDbCommand cmd_Update = new OleDbCommand(UpdateSql, con);
                OleDbParameter[] p = { new OleDbParameter("?", sendL2DataModel.MsgStatus), new OleDbParameter("?", sendL2DataModel.Desno.ToString()) };
                cmd_Update.Parameters.AddRange(p);
                return cmd_Update.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                ShowMsg(ex.ToString(),0);
                return 0;
            }
            
        }

        
    }
}
