using BLL;
using Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using System.IO.MemoryMappedFiles;
using System.Configuration;


namespace SocketComponent
{

    
    public class Receive
    {
        
        /// <summary>
        /// ShowMsg委托，用于接收和承载外部的输出函数
        /// 也就是说外面想要如何输出里面的状态只需要往里面传一个函数就可以了
        /// </summary>
        public Action<string, int> ShowMsg { get; set; }
        /// <summary>
        /// 委托，使得内部函数可以操作外部的变量，这个是为了操作CboUsers这个列表，让主程序可以自由添加用户的IP和Port
        /// </summary>
        public EditCboUsers AddCboUsers { get; set; }
        /// <summary>
        /// 同上的委托
        /// </summary>
        public EditCboUsers DeleteCboUsers { get; set; }
        /// <summary>
        /// 同上，一样的目的
        /// </summary>
        
        public EditCboUsers ChangeCboUsersIndex { get; set; }

        public SetConnectionStatus setConnectionStatus { get; set; }

        /// <summary>
        /// 将远程连接的客户端的IP地址和Socket存入集合中
        /// </summary>
        Socket socketSend;

        /// <summary>
        /// 引擎启动
        /// 给控制台用的，一键开始
        /// </summary>
        /// <param name="Ip">需要输入本地监听IP</param>
        /// <param name="port">和本体监听PORT</param>
        public void Enginstart(String Ip, String port)
        {
            TestDataBaseConnection();
            ShowMsg("Engine Start",1);
            //开一个线程，把程序的运行状态存到共享内存
            CheckMappedFiles.CheckRunning();
            ReceiveSocket(Ip, port);
        }


        /// <summary>
        /// 输入数据包buffer
        /// 输出解析后的报头和报文
        /// </summary>
        /// <param name="r"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public HeaderAndMessage ReceiveFromBuffer(int r, byte[] buffer)
        {
            HeaderAndMessage ham = new HeaderAndMessage();

            if (r == 0)
            {
                return null;
            }
            byte pa = buffer[r - 1];
            //ShowMsg(pa.ToString());

            //int flag = Patity(buffer1);

            if (pa == 0x03)
            {
                ShowMsg("尾部校验成功", 0);
            }
            ShowMsg("开始解析数据", 0);
            ham = ParsingData(buffer);
            ShowMsg("解析数据完成", 0);
            //ShowMsg("读取报文数据完毕并成功储存");
            MessageBLL mb = new MessageBLL();
            mb.ShowMsg =ShowMsg;
            mb.SaveWithoutDllDB2(ham.msg);
            //dwn.Message += dwn.dp.Data;
            //ShowMsg("接受完毕并成功保存");
            ShowMsg("更新数据库完成", 0);
            //ShowMsg("连接数据库并进行存储完成", 0);
            return ham;

        }

        /// <summary>
        /// 用于开头的测试数据库是否能够连接
        /// </summary>
        public void TestDataBaseConnection() 
        {
            ShowMsg("测试数据库连接", 0);
            MessageBLL mb = new MessageBLL();
            mb.ShowMsg = ShowMsg;
            mb.TestDemo();
            //mb.TestAllRows();
        }

        public void ReceiveSocket(string Ip, string Port)
        {
            try
            {
                ShowMsg("创建一个负责监听的socket", 1);
                Socket socketWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                ShowMsg("创建IP地址和端口号对象", 1);

                IPAddress ip = IPAddress.Parse(Ip);
                IPEndPoint point = new IPEndPoint(ip, Convert.ToInt32(Port));

                ShowMsg("point:" + point.ToString(), 1);

                //让负责监听的Socket绑定IP地址跟端口号
                socketWatch.Bind(point);
                
                ShowMsg("监听成功", 0);

                //设置监听队列
                socketWatch.Listen(10);

                ShowMsg("负责监听的Socket 来接受客户端的连接 创建跟客户端通信的Socket", 1);
                Thread th = new Thread(Listen);
                th.Name = "Linstener";
                th.IsBackground = true;
                th.Start(socketWatch);
            }
            catch (Exception ex)
            {
                ShowMsg(ex.Message, 0);
            }
        }


        public void Listen(object o)
        {
            try
            {
                ShowMsg("开始监听", 0);
                Socket socketWatch = o as Socket;
                Thread onlyone = null;
                Socket theonlysocketSend = null;
                while (true)
                {
                    socketSend = socketWatch.Accept();
                    String Ip = socketSend.RemoteEndPoint.ToString();
                    //dicSocket.Add(Ip, socketSend);

                    AddCboUsers(socketSend.RemoteEndPoint.ToString(),socketSend);
                    
                    ShowMsg(Ip + "连接成功", 0);
                    setConnectionStatus(true);


                    if (onlyone != null)
                    {
                        onlyone.Abort();
                        DeleteCboUsers(theonlysocketSend.RemoteEndPoint.ToString(),socketSend);
                        ShowMsg("来了新线程,关闭之前的线程",0);
                    }
                    ChangeCboUsersIndex(socketSend.RemoteEndPoint.ToString(),null);
                    Thread th = new Thread(Recive);
                    th.Name = "Receive";
                    th.IsBackground = true;
                    th.Start(socketSend);
                    onlyone = th;
                    theonlysocketSend = socketSend;
                }
            }
            catch(Exception e)
            {
                ShowMsg(e.Message,0);
            }

        }

        public void Recive(object o)
        {
            ShowMsg("开始接收", 0);
            Socket socketSend = o as Socket;
            Message msg = new Message();


            while (true)
            {
                try
                {

                    byte[] buffer = new byte[1024 * 10];

                    int r = socketSend.Receive(buffer);
                    buffer = buffer.Skip(0).Take(r).ToArray();

                    ShowMsg("接受数据成功", 0);

                    HeaderAndMessage ham = ReceiveFromBuffer(r, buffer);
                    if (ham == null)
                    {
                        ShowMsg("对方断开了链接，主要是拔掉网线，结束线程", 0);
                        setConnectionStatus(false);
                        return;
                    }


                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    if (e.Message == "远程主机强迫关闭了一个现有的连接。")
                    {
                        ShowMsg("远程主机强迫关闭了一个现有的连接，结束线程", 0);
                        setConnectionStatus(false);
                        return;
                    }
                }
            }
        }
        /// <summary>
        /// 输入数据包
        /// 输出解析后的报头和报文
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public HeaderAndMessage ParsingData(byte[] buffer)
        {
            try
            {
                HeaderAndMessage ham = new HeaderAndMessage();
                ham.head = ParsingHeader(buffer);

                byte[] dataBufeer = buffer.Skip(64).Take(int.Parse(ham.head.length) - 65).ToArray();

                ShowMsg(PrintData(dataBufeer), 1);

                ham.msg = ParsingMessage(dataBufeer);
                //ShowMsg(WriteUpdateSql(ham.msg),0);
                return ham;
            }
            catch (Exception e)
            {

                ShowMsg(e.Message+"解析报表出错，输入有错误", 0);
                return null;
            }
        }
        /// <summary>
        /// 专门解析报头的函数
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public Header ParsingHeader(byte[] buffer)
        {
            
            Header head = new Header();
            head.length = Encoding.ASCII.GetString(buffer, 0, 4);

            int DataLength = int.Parse(head.length);
            
            // 涉密删除

            head.ETX = buffer[DataLength - 1];

            // 涉密删除

            return head;
        }
        public Message ParsingMessage(byte[] data)
        {
            try
            {
                Message msg = new Message();

                // 涉密删除

                return msg;
            }
            catch (Exception e)
            {

                ShowMsg(e.Message, 0);
                ShowMsg("处理报文错误", 0);
                return null;
            }
            
        }
        public static int ToInt(byte[] bytes)
        {
            int number = 0;
            for (int i = 0; i < 4; i++)
            {
                number += bytes[i] << i * 8;
            }
            return number;
        }
        public static String PrintData(byte[] data)
        {
            String str = "";
            foreach (var item in data)
            {
                str += String.Format("{0:X2} ", item);
            }
            return str;
        }
        public string WriteUpdateSql(Message msg)
        {
            string result = "";
            Type t = msg.GetType();
            PropertyInfo[] PropertyList = t.GetProperties();
            foreach (PropertyInfo item in PropertyList)
            {
                result += item.Name;
                result += "='";

                object value = item.GetValue(msg, null);
                result += value.ToString();
                result += "',";
            }
            return result;
        }

    }
}
