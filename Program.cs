using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DynamicPatcher
{
    [ComVisible(true)]
    public interface IPatcher
    {
    }

    [ClassInterface(ClassInterfaceType.None)]
    [ComVisible(true), Guid("4BC759CC-5BB6-4E10-A14E-C813C869CE2F")]
    [ProgId("DynamicPatcher")]
    public class Program : IPatcher
    {
        static Program()
        {
            DllMain();
        }

        private static void DllMain()
        {
            var patcher = new Patcher();
            string workDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DynamicPatcher");
            patcher.Init(workDir);
            patcher.StartWatchPath(workDir);
        }

        // init the class Program and invoke ctor
        static public void Active() { }
        static public int ActiveFromCLR(string _)
        {
            return 1919810;
        }
    }
}
