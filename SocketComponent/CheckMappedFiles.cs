using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO.MemoryMappedFiles;
using System.Configuration;

namespace SocketComponent
{
    public static class CheckMappedFiles
    {
        /// <summary>
        /// 开线程的过程，把程序的运行状态存到共享内存
        /// </summary>
        public static void CheckRunning()
        {
            Thread th = new Thread(IsRunning);
            th.Name = "CheckRunning";
            th.IsBackground = true;
            th.Start();
        }
        /// <summary>
        /// 线程内的函数的代码
        /// </summary>
        public static void IsRunning()
        {
            while (true)
            {
                int SpecialCode = int.Parse(DateTime.Now.ToString("ddhhmmss"));
                UpdateStatus("OS", SpecialCode);
                Thread.Sleep(1000);
            }

        }

        public static void UpdateStatus(String filename, bool status)
        {
            long capacity = 1;
            //Console.WriteLine(capacity);
            MemoryMappedFile mmf = MemoryMappedFile.CreateOrOpen(ConfigurationManager.AppSettings.Get(filename), capacity, MemoryMappedFileAccess.ReadWrite);
            //创建或者打开共享内存

            //通过MemoryMappedFile的CreateViewAccssor方法获得共享内存的访问器
            var viewAccessor = mmf.CreateViewAccessor(0, capacity);

            //向共享内存开始位置写入字符串的长度
            viewAccessor.Write(0, status);
            //mmf.Dispose();
        }

        public static void UpdateStatus(String filename, int status)
        {
            long capacity = 4;
            //Console.WriteLine(capacity);
            MemoryMappedFile mmf = MemoryMappedFile.CreateOrOpen(ConfigurationManager.AppSettings.Get(filename), capacity, MemoryMappedFileAccess.ReadWrite);
            //创建或者打开共享内存

            //通过MemoryMappedFile的CreateViewAccssor方法获得共享内存的访问器
            var viewAccessor = mmf.CreateViewAccessor(0, capacity);

            //向共享内存开始位置写入字符串的长度
            viewAccessor.Write(0, status);

            //mmf.Dispose();
        }

    }
}
