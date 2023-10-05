using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    /// <summary>
    /// 报头和解析后的报文的整体
    /// </summary>
    public class HeaderAndMessage
    {
        //public DataPack dp { get; set; }
        /// <summary>
        /// 报头
        /// </summary>
        public Header head { get; set; }
        /// <summary>
        /// 报文
        /// </summary>
        public Message msg { get; set; }
        //public string Message { get; set; }

    }
}
