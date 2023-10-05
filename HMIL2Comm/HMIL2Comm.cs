using SocketComponent;
using Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.IO.MemoryMappedFiles;
using BLL;

namespace WindowsFormsServer
{
    public partial class L2ConnectionProgram : Form
    {
        private static readonly object LOCK = new object(); 
        private LogFiles _logFiles;
        public String FileName { get; set; }
        public bool ConnectionStatus = false;
        public SocketClient socketClient;
        public TSendL2DataBLL sendL2DataBLL;
        public void setConnectionStatus(bool flag)
        {
            ConnectionStatus = flag;
        }

        public L2ConnectionProgram()
        {
            
            InitializeComponent();
            _logFiles = new LogFiles("SecondSteelmakingConn");
            _logFiles.InitialLize();
            
            String sPath = "../log/";

            if (!Directory.Exists(sPath))
            {
                Directory.CreateDirectory(sPath);
            }
            FileName = sPath + DateTime.Now.ToString("yyyyMMddhhmmss") + "logfiles.txt";
            FileStream logfile = new FileStream(FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            StreamWriter sw = new StreamWriter(logfile);
            sw.WriteLine(DateTime.Now.ToString("yyyyMMddhhmmss") + "开始写日志");
            sw.Close();
            logfile.Close();
            //该Receive类即为中间件
            String IP = ConfigurationManager.AppSettings.Get("IP");
            String PORT = ConfigurationManager.AppSettings.Get("PORT");
            
            txtServer.Text = IP;
            txtPort.Text = PORT;
            ShowMsg("从配置文件读取IP和PORT分别为" + IP + ",and," + PORT,0);
            CheckConnection();
            sendL2DataBLL = new TSendL2DataBLL(ShowMsg);
            InitSocketClient();
        }
        public void InitSocketClient()
        {
            if (socketClient == null)
            {
                socketClient = new SocketClient(
                        ConfigurationManager.AppSettings["TargetIp"],
                        int.Parse(ConfigurationManager.AppSettings["TargetPort"]),
                        ConfigurationManager.AppSettings["LocalIp"]
                        );
            }
            socketClient.OnMessage += (string str) =>
            {
                ShowMsg(str,0);
            };
        }
        public void ContinueUpdateConnectionStatus()
        {
            while (true)
            {
                CheckMappedFiles.UpdateStatus("CS", ConnectionStatus);
                Thread.Sleep(1000);
            }
        }

        public void CheckConnection()
        {
            Thread th = new Thread(ContinueUpdateConnectionStatus);
            th.Name = "th1";
            th.IsBackground = true;
            th.Start();
        }

        private void L2ConnectionProgram_FormClosed(Object sender, FormClosedEventArgs e)
        {
            setConnectionStatus(false);
        }
        private void ListenBegin() 
        {
            Receive rece = new Receive();
            rece.ShowMsg = ShowMsg;
            rece.AddCboUsers = new EditCboUsers(AddinCboUsers);
            rece.DeleteCboUsers = new EditCboUsers(DeleteFromCboUsers);
            rece.ChangeCboUsersIndex = new EditCboUsers(ChangeSelectedIndex);
            rece.setConnectionStatus = new SetConnectionStatus(setConnectionStatus);
            try
            {
                rece.Enginstart(txtServer.Text,txtPort.Text);
            }
            catch (Exception ex)
            {
                ShowMsg(ex.Message, 0);
                //return null;
            }
            finally 
            {
                this.btnStart.ForeColor = System.Drawing.Color.Gray;
                this.btnStart.Enabled = false;
            }
        }
        private void btnStart_Click(object sender, EventArgs e)
        {
            ListenBegin();
        }

        Dictionary<string, Socket> dicSocket = new Dictionary<string, Socket>();

        void AddinCboUsers(String SocketString,Socket socketSend)
        {
            dicSocket.Add(SocketString, socketSend);
            cboUsers.Items.Add(SocketString);
        }

        void DeleteFromCboUsers(String SocketString,Socket socketSend) 
        {
            cboUsers.Items.Remove(SocketString);
            dicSocket.Remove(SocketString);
        }
        void ChangeSelectedIndex(String SocketString, Socket socketSend)
        {
            cboUsers.SelectedIndex = cboUsers.Items.IndexOf(SocketString);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="v">要输出的数据</param>
        /// <param name="type">显示类型0-->输出并且写入文件1-->只写入文件</param>
        public void ShowMsg(String v,int type)
        {
            lock (LOCK)
            {
                //输出并且写入文件
                if (type == 0)
                {

                    int length = int.Parse(ConfigurationManager.AppSettings.Get("InformationDisplayAreaLength")); ;
                    textLog.Text = updateString(textLog.Text, length);
                    textLog.AppendText(DateTime.Now.ToString("yyyy年MM月dd日hh时mm分ss秒") + ":" + v + "\r\n");

                    _logFiles.WriteIntoFile(v);
                }
                //只写入文件
                if (type == 1)
                {
                    _logFiles.WriteIntoFile(v);
                }
            }
            
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            this.FormClosed += new FormClosedEventHandler(L2ConnectionProgram_FormClosed);
            ListenBegin();

            
        }

        public string updateString(string s,int length) 
        {
            if (s.Count(p => p == '\n') >= length)
            {
                while (s.Count(p => p == '\n') >= length)
                {
                    s = s.Remove(0, s.IndexOf("\n") + 1);
                }
            }
            //textLog.ScrollToCaret();
            textLog.Select(textLog.Text.Length, 0);
            return s;
        }
        //public Send send=null;
        private void timer1_Tick(object sender, EventArgs e)
        {
            new Task(() =>
            {
                GetAndSend();
            }).Start();
        }
        /// <summary>
        /// 一次循环
        /// 从数据库中读取数据，然后发送出去
        /// </summary>
        public void GetAndSend()
        {
            List<TSendL2DataModel> sendL2DataModels = sendL2DataBLL.ToList();
            Header header = sendL2DataBLL.GenerateHead();
            foreach (TSendL2DataModel item in sendL2DataModels)
            {

                try
                {
                    List<byte> ByteList = new List<byte>();
                    ByteList.AddRange(item.Desno.IntToBtyes());
                    ByteList.AddRange(item.AutoSlagBefore.IntToBtyes());
                    ByteList.AddRange(item.AutoSlagAfter.IntToBtyes());
                    socketClient.SendDataPack(ByteList.ToArray(), header);
                    ShowMsg("消息的内容为:" + CommonShow.ShowAllPropertiesAndValues(item), 1);
                    //textMsg.Text = null;
                    item.MsgStatus = 1;
                    sendL2DataBLL.Update(item);

                }
                catch (Exception ex)
                {
                    ShowMsg(ex.Message, 0);
                    ShowMsg(ex.ToString(), 1);
                }
            }
        }
        private void BtnSendOneMessage_Click(object sender, EventArgs e)
        {
            try
            {
                socketClient.SendBuffer(new byte[] { 0x01, 0x02, 0x03 });
            }
            catch (Exception ex)
            {
                ShowMsg(ex.Message, 0);
                ShowMsg(ex.ToString(), 1);
            }
            
        }
    }
}
