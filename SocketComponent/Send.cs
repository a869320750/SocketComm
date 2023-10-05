using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using BLL;
using System.Threading;

namespace SocketComponent
{
    public class Send
    {
        private List<byte[]> LastDatas = new List<byte[]>() { new byte[] { 0x00 } , new byte[] { 0x01 } , new byte[] { 0x02 } };
        private List<int> LastDatas1 = new List<int>() {1,2,3 };
        private bool NeedReSend = false;
        public Socket SendSocket=null;
        public Send(string LocalIP,int LocalPort,string TargetIP, int TargetPort, Action<string,int> ShowMsg)
        {
            this.LocalIP = LocalIP;
            this.LocalPort = LocalPort;
            this.TargetIP = TargetIP;
            this.TargetPort = TargetPort;
            this.ShowMsg = ShowMsg;
            IsRun = true;
            Start();
            //LinkTest();
        }
        public Send(string LocalIP, int LocalPort, string TargetIP, int TargetPort, Action<string, int> ShowMsg,bool AutoStart)
        {
            this.LocalIP = LocalIP;
            this.LocalPort = LocalPort;
            this.TargetIP = TargetIP;
            this.TargetPort = TargetPort;
            this.ShowMsg = ShowMsg;
            if (AutoStart)
            {
                IsRun = true;
                Start();
            }
            
            //LinkTest();
        }
        private bool IsConnect=false;
        private bool IsRun=false;
        private string LocalIP;
        private int LocalPort;
        private Action<string, int> ShowMsg;
        private string TargetIP;
        private int TargetPort;
        public void InitializeSendSocket()
        {
            if (SendSocket==null)
            {
                SendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress iPAddress1 = IPAddress.Parse(LocalIP);
                /*
                 * 这里给Port赋值0，意味着自动分配一个端口
                 */
                IPEndPoint iPEndPoint1 = new IPEndPoint(iPAddress1, Convert.ToInt32(0));
                SendSocket.Bind(iPEndPoint1);
                IsConnect = true;
            }
        }
        public void DisposeSendSocket()
        {
            if (SendSocket!=null)
            {
                SendSocket.Close();
                SendSocket.Dispose();

                SendSocket = null;

            }
            
        }
        public void LinkStart()
        {
            if (!SendSocket.Connected)
            {
                try
                {
                    SendSocket.Connect(TargetIP, TargetPort);
                }
                catch (Exception ex)
                {
                    ShowMsg(ex.Message, 0);
                }
                
            }
        }
        public void LinkEnd()
        {
            if (SendSocket.Connected)
            {
                
                SendSocket.Shutdown(SocketShutdown.Both);
                SendSocket.Disconnect(true);
            }

        }

        /// <summary>
        /// 开启Socket连接
        /// </summary>
        public void Start()
        {
            InitializeSendSocket();
            LinkStart();
        }
        /// <summary>
        /// 关闭Socket连接
        /// </summary>
        public void Close()
        {
            if (SendSocket != null)
            {
                SendSocket.Close();
            }
            
            SendSocket = null;
            IsConnect = false;
            
        }
        ~Send()
        {
            IsRun = false;
            this.Close();
        }
        public void Restart()
        {
            this.Close();
            this.InitializeSendSocket();
            this.LinkStart();
        }
        public void LinkTest()
        {
            new Task(() =>
            {
                while (IsRun)
                {
                    if (SendSocket!=null)
                    {
                        if (!SendSocket.Poll(10, SelectMode.SelectWrite))
                        {
                            Restart();
                        }
                        try
                        {
                            SendSocket.Send(new byte[] {  });
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message.Equals("你的主机中的软件中止了一个已建立的连接。"))
                            {
                                ShowMsg("对方断了，重连",0); 
                                Restart();
                            }
                            //ShowMsg(ex.Message, 0);
                            
                        }
                        
                    }
                    Thread.Sleep(1000);
                    ShowMsg("Yes",0);
                }
            }).Start();
            
        }
        public void PoolingAndSend()
        {
            TSendL2DataBLL sendL2DataBLL = new TSendL2DataBLL(ShowMsg);
            List<TSendL2DataModel> sendL2DataModels = sendL2DataBLL.ToList();
            Header head = sendL2DataBLL.GenerateHead();

            foreach (TSendL2DataModel item in sendL2DataModels)
            {
                List<byte> ByteList = new List<byte>();
                ByteList.AddRange(Int2Bytes(item.Desno));
                ByteList.AddRange(Int2Bytes(item.AutoSlagBefore));
                ByteList.AddRange(Int2Bytes(item.AutoSlagAfter));
                LastDatas[2] = LastDatas[1];
                LastDatas[1] = LastDatas[0];
                LastDatas[0] = ByteList.ToArray();

                try
                {
                    if (IsConnect)
                    {
                        if (SendSocket != null)
                        {
                            ShowMsg(ByteList.ToArray().ToString() + "\t" + LastDatas[0] + "\t" + LastDatas[1] + "\t" + LastDatas[2], 0);

                            if (NeedReSend)
                            {
                                this.SendDataPack(LastDatas[2], SendSocket, head);
                                Thread.Sleep(100);
                                this.SendDataPack(LastDatas[1], SendSocket, head);
                                Thread.Sleep(100);
                                this.SendDataPack(LastDatas[0], SendSocket, head);
                                Thread.Sleep(100);
                                NeedReSend = false;
                            }
                            else
                            {
                                this.SendDataPack(ByteList.ToArray(), SendSocket, head);
                                ShowMsg("已发送给" + TargetIP + ":" + TargetPort + "消息:" +
                                "\r\n\t\tDESNO:" + item.Desno +
                                "\r\n\t\tAutoSlagBefore:" + item.AutoSlagBefore +
                                "\r\n\t\tAutoSlagAfter:" + item.AutoSlagAfter, 0);
                                //ShowMsg("消息的内容为:" + CommonShow.ShowAllPropertiesAndValues(item), 1);
                                ShowMsg("消息的内容为:" + CommonShow.ShowAllPropertiesAndValues(item), 1);
                                //textMsg.Text = null;
                                item.MsgStatus = 1;
                                sendL2DataBLL.Update(item);
                            }



                        }
                    }

                }
                catch (Exception ex)
                {
                    if (ex.Message.Equals("您的主机中的软件中止了一个已建立的连接。"))
                    {
                        NeedReSend = true;
                        ShowMsg("对方断了，重连", 0);
                        Restart();
                    }
                    else
                    {
                        ShowMsg(ex.Message, 0);
                    }
                }
            }
        }
        public void PoolingAndSendNew()
        {
            TSendL2DataBLL sendL2DataBLL = new TSendL2DataBLL(ShowMsg);
            List<TSendL2DataModel> sendL2DataModels = sendL2DataBLL.ToList();
            Header head = sendL2DataBLL.GenerateHead();

            foreach (TSendL2DataModel item in sendL2DataModels)
            {
                
                try
                {
                    this.Start();
                    ShowMsg("已发送给" + TargetIP + ":" + TargetPort + "消息:" +
                                "\r\n\t\tDESNO:" + item.Desno +
                                "\r\n\t\tAutoSlagBefore:" + item.AutoSlagBefore +
                                "\r\n\t\tAutoSlagAfter:" + item.AutoSlagAfter, 0);
                    //ShowMsg("消息的内容为:" + CommonShow.ShowAllPropertiesAndValues(item), 1);
                    ShowMsg("消息的内容为:" + CommonShow.ShowAllPropertiesAndValues(item), 1);
                    //textMsg.Text = null;
                    item.MsgStatus = 1;
                    sendL2DataBLL.Update(item);
                    this.Close();

                }
                catch (Exception ex)
                {
                    if (ex.Message.Equals("您的主机中的软件中止了一个已建立的连接。"))
                    {
                        NeedReSend = true;
                        ShowMsg("对方断了，重连", 0);
                        Restart();
                    }
                    else
                    {
                        ShowMsg(ex.Message, 0);
                    }
                }
            }
        }
        public void PoolingAndSendTest1()
        {
            TSendL2DataBLL sendL2DataBLL = new TSendL2DataBLL(ShowMsg);
            Header head = sendL2DataBLL.GenerateHead();
            string MessageData = DateTime.Now.ToString("ss");
            LastDatas1[2] = LastDatas1[1];
            LastDatas1[1] = LastDatas1[0];
            LastDatas1[0] = int.Parse(MessageData);

            try
            {
                if (IsConnect)
                {
                    if (SendSocket != null)
                    {
                        ShowMsg(MessageData + "\t" + LastDatas[0] + "\t" + LastDatas[1] + "\t" + LastDatas[2], 0);

                        if (NeedReSend)
                        {
                            this.SendDataPack(handle(LastDatas1[2].ToString(),2), SendSocket, head);
                            Thread.Sleep(100);
                            this.SendDataPack(handle(LastDatas1[1].ToString(), 2), SendSocket, head);
                            Thread.Sleep(100);
                            this.SendDataPack(handle(LastDatas1[0].ToString(), 2), SendSocket, head);
                            Thread.Sleep(100);
                            NeedReSend = false;
                        }
                        else
                        {
                            this.SendDataPack(handle(MessageData,2), SendSocket, head);
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                if (ex.Message.Substring(1).Equals("的主机中的软件中止了一个已建立的连接。"))
                {
                    NeedReSend = true;
                    ShowMsg("对方主动断开了连接", 0);
                    Restart();
                }
                else if (ex.Message.Equals("远程主机强迫关闭了一个现有的连接。"))
                {
                    NeedReSend = true;
                    ShowMsg("远程主机可能直接把软件关了", 0);
                    Restart();
                }
                else
                {
                    NeedReSend = true;
                    ShowMsg(ex.Message, 0);
                    Restart();
                }
            }



        }
        public void PoolingAndSendTest(string TargetIP, int TargetPort)
        {
            TSendL2DataBLL sendL2DataBLL = new TSendL2DataBLL(ShowMsg);
            Header head = sendL2DataBLL.GenerateHead();
            for (int i = 0; i < 1; i++)
            {
                int temp=int.Parse(DateTime.Now.ToString("ss"));

                LastDatas1[2] = LastDatas1[1];
                LastDatas1[1] = LastDatas1[0];
                LastDatas1[0] = temp;
                try
                {
                    if (IsConnect)
                    {
                        //Start();
                        if (SendSocket != null)
                        {
                            ShowMsg(temp.ToString() + "\t" + LastDatas1[0] + "\t" + LastDatas1[1] + "\t" + LastDatas1[2], 0);
                            if (NeedReSend)
                            {
                                this.SendDataPack(Int2Bytes(LastDatas1[2]), SendSocket, head);
                                Thread.Sleep(100);
                                this.SendDataPack(Int2Bytes(LastDatas1[1]), SendSocket, head);
                                Thread.Sleep(100);
                                this.SendDataPack(Int2Bytes(LastDatas1[0]), SendSocket, head);
                                Thread.Sleep(100);
                                NeedReSend = false;
                            }

                            this.SendDataPack(Int2Bytes(temp), SendSocket, head);
                        }
                    }


                }
                catch (Exception ex)
                {
                    if (ex.Message.Equals("你的主机中的软件中止了一个已建立的连接。"))
                    {
                        NeedReSend = true;
                        ShowMsg("对方断了，重连", 0);
                        Restart();
                    }
                    else
                    {
                        ShowMsg(ex.Message, 0);
                    }
                }
            }
        }
        public void PoolingAndSendTest2()
        {

            TSendL2DataBLL sendL2DataBLL = new TSendL2DataBLL(ShowMsg);
            Header head = sendL2DataBLL.GenerateHead();
            try
            {
                Start();
                this.SendDataPack(Int2Bytes(int.Parse(DateTime.Now.ToString("ss"))), SendSocket, head);
                Close();
            }
            catch (Exception ex)
            {
                if (ex.Message.Equals("你的主机中的软件中止了一个已建立的连接。"))
                {
                    NeedReSend = true;
                    ShowMsg("对方断了，重连", 0);
                    Restart();
                }
                else
                {
                    ShowMsg(ex.Message, 0);
                }
            }
        }


        public void SendDataPack(Socket socket, HeaderAndMessage ham)
        {
            byte[] bufferlength = new byte[4];

            String Data=ham.msg.ToString();

            byte[] DataBuffer = System.Text.Encoding.UTF8.GetBytes(Data);

            bufferlength = System.Text.Encoding.UTF8.GetBytes((DataBuffer.Length + 65).ToString("0000"));
            //bufferlength = BitConverter.GetBytes(bufferdata.Length);


            List<byte> list = new List<byte>();
            list.AddRange(bufferlength);

            list.AddRange(DataBuffer);

            byte[] bufferaEtx = new byte[1];
            bufferaEtx[0] = ham.head.ETX;
            list.AddRange(bufferaEtx);

            //将泛型集合转换为数组

            byte[] newBuffer = list.ToArray();

            //byte parity = BitConverter.GetBytes(Patity(newBuffer))[0];
            //list.Add(parity);
            //newBuffer = list.ToArray();
            //    socketSend.Send(buffer);
            //获得用户在下拉框中选中的IP地址

            Console.WriteLine(socket);
            socket.Send(newBuffer);
        }
        public void SendDataPack(string data, Socket socket,Header head)
        {
            //字符串改成byte[]形式准备组装
            byte[] DataBuffer = System.Text.Encoding.UTF8.GetBytes(data);

            ///计算总体长度
            byte[] bufferlength = new byte[4];
            bufferlength = System.Text.Encoding.UTF8.GetBytes((DataBuffer.Length + 65).ToString("0000"));
            //bufferlength = BitConverter.GetBytes(bufferdata.Length);

            //开始组装
            List<byte> list = new List<byte>();
            list.AddRange(bufferlength);

            list.AddRange(DataBuffer);

            byte[] bufferaEtx = new byte[1];
            bufferaEtx[0] = head.ETX;
            list.AddRange(bufferaEtx);

            byte[] newBuffer = list.ToArray();
            socket.Send(newBuffer);
        }
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="data">要发送的数据</param>
        /// <param name="socket">发送数据使用的Socket对象</param>
        /// <param name="head">要发送的数据头尾</param>
        public void SendDataPack(byte[] data, Socket socket, Header head)
        {
            //字符串改成byte[]形式准备组装
            //byte[] DataBuffer = System.Text.Encoding.UTF8.GetBytes(data);

            ///计算总体长度
            byte[] bufferlength = new byte[4];
            bufferlength = System.Text.Encoding.UTF8.GetBytes((data.Length + 65).ToString("0000"));
            //bufferlength = BitConverter.GetBytes(bufferdata.Length);

            //开始组装
            List<byte> list = new List<byte>();
            list.AddRange(bufferlength);

            list.AddRange(data);

            byte[] bufferaEtx = new byte[1];
            bufferaEtx[0] = head.ETX;
            list.AddRange(bufferaEtx);
            byte[] newBuffer = list.ToArray();
            string result = "";
            foreach (byte item in newBuffer)
            {
                result += item.ToString("X").PadLeft(2,'0');
            }
            ShowMsg(result, 1);
            socket.Send(newBuffer);
            ShowMsg("发送成功", 0);
        }
        public void SendDataPack(string data, Socket socket)
        {
            Header head = new Header();

            byte[] DataBuffer = System.Text.Encoding.UTF8.GetBytes(data);

            byte[] bufferlength = new byte[4];
            bufferlength = System.Text.Encoding.UTF8.GetBytes((DataBuffer.Length + 65).ToString("0000"));


            List<byte> list = new List<byte>();
            list.AddRange(bufferlength);

            list.AddRange(DataBuffer);

            byte[] bufferaEtx = new byte[1];
            bufferaEtx[0] = head.ETX;
            list.AddRange(bufferaEtx);

            //将泛型集合转换为数组

            byte[] newBuffer = list.ToArray();

            socket.Send(newBuffer);
        }

        public byte[] handle(string a, int length)
        {
            var b = System.Text.Encoding.UTF8.GetBytes(a);
            if (b.Length >= length)
            {
                return b.Skip(0).Take(length).ToArray();
            }
            else
            {
                for (int i = 0; i < length - b.Length; i++)
                {
                    a += "0";
                }
                return System.Text.Encoding.UTF8.GetBytes(a);
            }

        }
        public static byte[] Int2Bytes(Int32 intValue)
        {
            byte[] intBytes = BitConverter.GetBytes(intValue);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(intBytes);
            byte[] result = intBytes;
            return result;
        }
    }

}
