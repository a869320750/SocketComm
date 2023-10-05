using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace SocketComponent
{
    /// <summary>
    /// 显示信息
    /// 存储到log文件
    /// </summary>
    /// <param name="v">输出的数据</param>
    /// <param name="type">输出类型0代表写文件和显示信息;1代表只写文件</param>
    ///public delegate void ShowMsgDelegate(String v, int type);
    /// <summary>
    /// 在类库操作界面的元素
    /// </summary>
    /// <param name="SocketString"></param>
    /// <param name="socketSend"></param>
    public delegate void EditCboUsers(String SocketString, Socket socketSend);
    public delegate void SetConnectionStatus(bool flag);
}
