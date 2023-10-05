using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;

namespace BLL
{
    public class LogFiles
    {
        private string _FileName;
        private String sPath = "../log/";
        private string FileNameSuffix= "MESConnectLog";
        
        public LogFiles()
        {

        }
        public LogFiles(string FileNameSuffix)
        {
            this.FileNameSuffix = FileNameSuffix;
        }
        
        public void UpdateFileName()
        {
            _FileName= sPath + DateTime.Now.ToString("yyyy_MM_dd_") + FileNameSuffix + ".txt";
        }
        public void InitialLize()
        {
            #region 初始化日志文件
            if (!Directory.Exists(sPath))
            {
                Directory.CreateDirectory(sPath);
            }
            //ConfigurationManager.AppSettings.Get("FileName");
            UpdateFileName();
            FileStream logfile = new FileStream(_FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite,FileShare.Write);
            StreamWriter sw = new StreamWriter(logfile);
            sw.WriteLine(DateTime.Now.ToString("yyyy年MM月dd日 HH时mm分ss秒") + ":开始写日志");
            sw.Close();
            logfile.Close();
            #endregion
        }
        public void WriteIntoFile(string Message)
        {
            new Task(() =>
            {
                UpdateFileName();
                if (!File.Exists(_FileName))
                {
                    InitialLize();
                }
                FileStream logfile = new FileStream(_FileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                StreamWriter sw = new StreamWriter(logfile);

                sw.WriteLine(DateTime.Now.ToString("HH时mm分ss秒") + ":" + Message);
                sw.Close();
                logfile.Close();
            }).Start();
        }
    }
}
