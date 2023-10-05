using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace WindowsFormsServer
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool isAppRunning = false;
            //var Run = new System.Threading.Mutex(true, "SmsProgram", out noRun);
            System.Threading.Mutex mutex = new System.Threading.Mutex(true,System.Diagnostics.Process.GetCurrentProcess().ProcessName,out isAppRunning); 
            if (!isAppRunning)
            {
                MessageBox.Show("本程序已经在运行");
                Environment.Exit(1);
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new L2ConnectionProgram());
            }
            
        }
    }
}
