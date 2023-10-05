using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace BLL
{
    public static class CommonShow
    {
        public static string ShowAllPropertiesAndValues<T>(T t)
        {
            if (t != null)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(typeof(T).Name + ":");
                foreach (var item in typeof(T).GetProperties())
                {
                    if (item.GetValue(t,null) != null)
                    {
                        stringBuilder.AppendLine("    " + item.Name + ":" + item.GetValue(t,null).ToString());
                    }
                }
                return stringBuilder.ToString();
            }
            return "";
        }
    }
}
