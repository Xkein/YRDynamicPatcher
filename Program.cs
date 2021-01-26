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
    /// <summary>Provides activation way for DynamicPatcher.</summary>
    [ComVisible(true)]
    public interface IPatcher
    {
    }

    /// <summary>The class to activate DynamicPatcher</summary>
    [ClassInterface(ClassInterfaceType.None)]
    [ComVisible(true), Guid("4BC759CC-5BB6-4E10-A14E-C813C869CE2F")]
    [ProgId("DynamicPatcher")]
    public class Program : IPatcher
    {
        /// <summary>The instance of DynamicPatcher.</summary>
        public static Patcher Patcher { get; private set; }

        static Program()
        {
            DllMain();
        }

        private static void DllMain()
        {
            Patcher = new Patcher();
            string workDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DynamicPatcher");
            Patcher.Init(workDir);
            Patcher.StartWatchPath(workDir);
        }

        /// <summary>Init the class Program and invoke ctor.</summary>
        static public void Activate() { }
        /// <summary>Init the class Program and invoke ctor.</summary>
        static public int ActivateFromCLR(string _)
        {
            return 1919810;
        }
    }
}
