using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketComponent
{
    public class SocketServer
    {
        public Socket SocketWatch { get; set; }
        public string LocalIp { get; set; }
        public int LocalPort { get; set; }
        public Action<string> OnMessage { get; set; }
        public Action<bool> OnConnectionStatusChanged { get; set; }
        private Task _StartReceive; 

        /// <summary>
        /// 刚刚链接过来的准备发送的Socket
        /// </summary>
        public Socket NewSocketSend { get; set; }
        /// <summary>
        /// 之前链接过来的准备发送的Socket
        /// </summary>
        public Socket SocketSend { get; set; }
        public SocketServer()
        {
            SocketWatch=new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            OnMessage("创建IP地址和端口号对象");
            IPAddress iPAddress = IPAddress.Parse(LocalIp);
            IPEndPoint iPEndPoint= new IPEndPoint(iPAddress, LocalPort);
            OnMessage("point:" + iPEndPoint.ToString());
            SocketWatch.Bind(iPEndPoint);
            OnMessage("绑定IP端口完成");
            SocketWatch.Listen(10);
            OnMessage("设置监听队列完成");
        }
        private void Listen()
        {
            try
            {
                OnMessage("开始监听");
                while (true)
                {
                    NewSocketSend = SocketWatch.Accept();
                    String Ip = SocketSend.RemoteEndPoint.ToString();
                    OnMessage(Ip + "连接成功");
                    OnConnectionStatusChanged(true);

                    if (_StartReceive != null)
                    {
                        _StartReceive.Dispose();
                        SocketSend = NewSocketSend;
                        OnMessage("来了新线程,关闭之前的线程");
                    }
                    _StartReceive = new Task(()=>
                    {

                    });
                    _StartReceive.Start();
                }
            }
            catch (Exception e)
            {
                OnMessage(e.Message);
            }
        }
    }
}
