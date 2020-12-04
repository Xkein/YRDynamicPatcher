using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicPatcher
{
    public class Logger
    {
        public delegate void WriteLineDelegate(string str);
        static public WriteLineDelegate WriteLine { get; set; }
        static public void Log(string format, params object[] args)
        {
            string str = string.Format(format, args);

            WriteLine.Invoke(str);
        }
    }
}
