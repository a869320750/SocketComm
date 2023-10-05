using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestMethods
{
    /// <summary>
    /// 
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            string s = "aaabbccddaabbccddaabbccddabcdaabbccdd";
            Console.WriteLine(s.Count(p=>p=='a'));

            int[] array = { 0, 1, 2, 3, 4, 5, 6, 7 };
            array = array.Skip(1).Take(3).ToArray();
            foreach (var item in array)
            {
                Console.WriteLine(item);
            }
            
            Console.ReadLine();
        }
    }
}
