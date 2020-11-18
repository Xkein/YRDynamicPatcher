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
        static StreamWriter streamWriter;
        static public Stream OutputStream {
            get => outputStream;
            set {
                outputStream = value;
                streamWriter = new StreamWriter(outputStream);
            }
        }

        private static Stream outputStream;
        static public void Log(string format, params object[] args)
        {
            string str = string.Format(format, args);

            if (streamWriter != null)
            {
                streamWriter.WriteLine(str);
                streamWriter.Flush();
            }
            Console.WriteLine(str);
        }
    }
}
