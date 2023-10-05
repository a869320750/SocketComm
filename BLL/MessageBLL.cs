using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;

using System.Configuration;
using System.IO;
using System.Data.OleDb;
using System.Reflection;


namespace BLL
{
    //public delegate void ShowMsgDelegate(String v, int type);
    /// <summary>
    /// 此版本为为了DB2改的通信程序
    /// 此处修改了sql语句
    /// 1.SQL语句变量的变化
    ///     原来Oracle的变量是desno=:desno
    ///     现在改为desno=?来代表变量
    /// 2.DB2的时间好像哟所变化
    /// </summary>
    public class MessageBLL
    {
        // 涉密删除

        public void SaveWithoutDllDB2(Model.Message msg)
        {

            if (msg.desno[0] == '2')
            {
                ShowMsg("处理号为:" + msg.desno + "处理号的第一位为2,，不作处理", 0);
                return;
            }
            string[,] str = new string[77, 2];
            {
                #region BuildStr
                // 涉密删除
                #endregion
            }
            OleDbConnection con = new OleDbConnection(Conn1);
            con.Open();
            try
            {
                Type t = msg.GetType();
                PropertyInfo[] PropertyList = t.GetProperties();
                OleDbParameter pp = new OleDbParameter(P_desno, msg.desno);
                OleDbCommand cmd_Select_one = new OleDbCommand(SELECT1_SQL, con);
                cmd_Select_one.Parameters.Add(pp);
                object o = cmd_Select_one.ExecuteScalar();

                if (o == null)
                {
                    
                    //object value = item.GetValue(msg, null);

                    string Attributes = str[0, 0];
                    string values = PropertyList[0].GetValue(msg, null).ToString();
                    for (int i = 1; i < 77; i++)
                    {
                        if (i == 3)
                        {
                            Attributes = Attributes + "," + str[i, 0];
                            values = values + "','" + msg.prodate.ToString("yyyy-MM-dd HH:mm:ss");
                            continue;
                        }
                        Attributes = Attributes + "," + str[i, 0];
                        values = values + "','" + PropertyList[i].GetValue(msg, null).ToString();

                    }
                    string SqlCommand = "insert into T_SJ_SLAG_RESULT(" + Attributes + ") values ('" + values + "')";
                    ShowMsg(SqlCommand, 0);
                    OleDbCommand cmd_insert_sql = new OleDbCommand(SqlCommand, con);
                    int o1 = cmd_insert_sql.ExecuteNonQuery();
                    ShowMsg("影响了" + o1.ToString() + "行", 0);
                    ShowMsg("接受来自该地址的数据完毕\r\n原本不存在该desno:"+msg.desno+"\r\n执行插入", 0);
                }
                else
                {
                    string SqlUpdateCommand = "update T_SJ_SLAG_RESULT set ";
                    for (int i = 0; i < 77; i++)
                    {
                        if (i == 3)
                        {
                            SqlUpdateCommand = SqlUpdateCommand + str[i, 0] + "='" + msg.prodate.ToString("yyyy-MM-dd HH:mm:ss") + "',";
                            continue;
                        }
                        if (i == 76)
                        {
                            SqlUpdateCommand = SqlUpdateCommand + str[i, 0] + "='" + PropertyList[i].GetValue(msg, null).ToString() + "'";
                            continue;
                        }
                        SqlUpdateCommand = SqlUpdateCommand + str[i, 0] + "='" + PropertyList[i].GetValue(msg, null).ToString()+"',";
                        
                    }
                    SqlUpdateCommand += " where desno="+msg.desno;
                    ShowMsg(SqlUpdateCommand, 0);
                    OleDbCommand cmd_update_sql = new OleDbCommand(SqlUpdateCommand, con);
                    int o1 = cmd_update_sql.ExecuteNonQuery();
                    //string SqlCommandDelete = "delete from T_SJ_SLAG_RESULT where desno='" + str[0, 1] + "'";
                    //OleDbCommand cmd_delete_sql = new OleDbCommand(SqlCommandDelete, con);
                    //int o2 = cmd_delete_sql.ExecuteNonQuery();
                    ShowMsg("影响了" + o1.ToString() + "行", 0);
                    ShowMsg("接受来自该地址的数据完毕\r\n原本存在该desno:" + msg.desno + "\r\n执行更新", 0);
                }
            }
            catch (Exception ex)
            {
                ShowMsg(ex.Message, 0);
                ShowMsg(ex.ToString(), 0);

            }
            finally 
            {
                con.Close();
            }
        }
        public void TestDemo()
        {
            OleDbConnection con = new OleDbConnection(Conn1);
            con.Open();

            try
            {
                OleDbCommand cmd_insert_demo = new OleDbCommand(Insert_Test_SQL, con);
                if (cmd_insert_demo.ExecuteNonQuery() == 1)
                {
                    ShowMsg("测试插入成功",0);
                }
                OleDbParameter pp = new OleDbParameter("?", 123456);
                OleDbCommand cmd_select1_sql = new OleDbCommand(SELECT1_SQL, con);
                cmd_select1_sql.Parameters.Add(pp);
                object o = cmd_select1_sql.ExecuteScalar();
                if (o == null)
                {
                    ShowMsg("数据表为空或无法连接数据库",0);
                }
                else
                {
                    ShowMsg("测试select成功",0);
                }
                OleDbCommand cmd_delete_demo = new OleDbCommand(Delete_Test_SQL, con);
                if (cmd_delete_demo.ExecuteNonQuery() == 1)
                {
                    ShowMsg("测试删除成功",0);
                }
                else
                {
                    ShowMsg("测试删除失败", 0);
                }
                
            }
            catch (Exception e)
            {
                ShowMsg("抛出数据库连接异常" + "\r\n",0);
                ShowMsg(e.Message + "\r\n",0);
            }
            finally 
            {
                con.Close();
            }
        }
        public void TestAllRows()
        {
            string[,] str = new string[77, 2];
            {
                #region BuildStr
                // 涉密删除
                #endregion
            }
            OleDbConnection con = new OleDbConnection(Conn1);
            con.Open();
            try
            {
                #region testSelect
                #endregion
                #region testInsert
                #endregion
                #region BuildInsertValue
                string Attributes = str[0, 0];
                string values = str[0, 1];
                for (int i = 1; i < 77; i++)
                {
                    
                    Attributes = Attributes + "," + str[i, 0];
                    values = values + "','" + str[i, 1];
                    string SqlCommand = "insert into T_SJ_SLAG_RESULT(" + Attributes + ") values ('" + values + "')";
                    ShowMsg(SqlCommand, 0);
                    OleDbCommand cmd_select1_sql = new OleDbCommand(SqlCommand, con);
                    int o1 = cmd_select1_sql.ExecuteNonQuery();
                    string SqlCommandDelete = "delete from T_SJ_SLAG_RESULT where desno='" + str[0, 1] + "'";
                    OleDbCommand cmd_delete_sql = new OleDbCommand(SqlCommandDelete, con);
                    int o2 = cmd_delete_sql.ExecuteNonQuery();
                    ShowMsg("影响了" + o1.ToString() + "行," + o2.ToString() + "行", 0);
                }
                #endregion



            }
            catch (Exception e)
            {
                ShowMsg(e.Message + "\r\n", 0);
                throw;
            }
            finally
            {
                con.Close();
            }
        }
    }
}
