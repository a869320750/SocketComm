using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SocketComponent
{
    public class SocketClient
    {
        public Socket SendSocket { get; set; }
        public string TargetIp { get; set; }
        public int TargetPort { get; set; }
        public string LocalIp { get; set; }
        public Action<string> OnMessage { get; set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="TargetIp"></param>
        /// <param name="TargetPort"></param>
        /// <param name="LocalIp"></param>
        public SocketClient(string TargetIp,int TargetPort,string LocalIp)
        {
            this.TargetIp = TargetIp;
            this.TargetPort = TargetPort;
            this.LocalIp = LocalIp;
        }
        /// <summary>
        /// 开启链接
        /// </summary>
        public void Start()
        {
            OnMessage("打开链接");
            if (SendSocket == null)
            {
                SendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress iPAddress1 = IPAddress.Parse(LocalIp);
                /*
                 * 这里给Port赋值0，意味着自动分配一个端口
                 */
                IPEndPoint iPEndPoint1 = new IPEndPoint(iPAddress1, Convert.ToInt32(0));
                SendSocket.Bind(iPEndPoint1);
            }
            if (!SendSocket.Connected)
            {
                try
                {
                    SendSocket.Connect(TargetIp, TargetPort);
                }
                catch (Exception ex)
                {
                    OnMessage(ex.Message);
                    OnMessage(ex.ToString());
                }
            }
        }
        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Stop()
        {
            OnMessage("关闭连接");
            if (SendSocket != null)
            {
                SendSocket.Close();
            }
            SendSocket = null;
        }
        /// <summary>
        /// 发送一条消息
        /// </summary>
        /// <param name="data">要发送的数据</param>
        /// <param name="socket">用来发数据的socket对象</param>
        /// <param name="head">数据头尾</param>
        public void SendDataPack(byte[] data,Header head)
        {
            
            //字符串改成byte[]形式准备组装
            //byte[] DataBuffer = System.Text.Encoding.UTF8.GetBytes(data);

            ///计算总体长度
            byte[] bufferlength = new byte[4];
            bufferlength = System.Text.Encoding.UTF8.GetBytes((data.Length + 65).ToString("0000"));
            //bufferlength = BitConverter.GetBytes(bufferdata.Length);

            //开始组装
            List<byte> list = new List<byte>();
            // 涉密删除
            byte[] bufferaEtx = new byte[1];
            bufferaEtx[0] = head.ETX;
            list.AddRange(bufferaEtx);
            byte[] newBuffer = list.ToArray();
            string result = "";
            foreach (byte item in newBuffer)
            {
                result += item.ToString("X").PadLeft(2, '0');
            }
            OnMessage(result);
            Start();
            SendSocket.Send(newBuffer);
            Stop();
            OnMessage("发送成功");
        }
        public void SendBuffer(byte[] data)
        {
            Start();
            SendSocket.Send(data);
            OnMessage("发送成功");
            Stop();
        }
    }
}
