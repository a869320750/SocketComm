using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocketComponent
{
    public static class DataTransform
    {
        /// <summary>
        /// 字符串转十六进制数据
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="length">目标长度，不足右边补空格</param>
        /// <returns></returns>
        public static byte[] StringToBytes(this string str,int length)
        {
            byte[] result = System.Text.Encoding.UTF8.GetBytes(str);
            if (result.Length >= length)
            {
                return result.Skip(0).Take(length).ToArray();
            }
            else
            {
                for (int i = 0; i < length - result.Length; i++)
                {

                    str += "0";
                }
                return System.Text.Encoding.UTF8.GetBytes(str);
            }
        }
        /// <summary>
        /// 数字转buffer
        /// </summary>
        /// <param name="intValue"></param>
        /// <returns></returns>
        public static byte[] IntToBtyes(this int intValue)
        {
            byte[] intBytes = BitConverter.GetBytes(intValue);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(intBytes);
            byte[] result = intBytes;
            result.Reverse();
            return result;
        }
    }
}
