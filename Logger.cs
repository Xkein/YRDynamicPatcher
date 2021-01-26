using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicPatcher
{
    /// <summary>Represents the log behavior for DynamicPatcher.</summary>
    public class Logger
    {
        /// <summary>Represents the method that will handle log message.</summary>
        public delegate void WriteLineDelegate(string str);
        /// <summary>Invoked when log the message.</summary>
        static public WriteLineDelegate WriteLine { get; set; }
        /// <summary>Write message to logger.</summary>
        static public void Log(string format, params object[] args)
        {
            string str = string.Format(format, args);

            WriteLine.Invoke(str);
        }

        /// <summary>Write message to logger.</summary>
        static public void Log(object obj)
        {
            WriteLine.Invoke(obj.ToString());
        }
    }
}
