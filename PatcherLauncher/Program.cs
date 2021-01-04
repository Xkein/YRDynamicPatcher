using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DynamicPatcher;
namespace PatcherLoader
{
    class Program
    {
        static void Main(string[] args)
        {
            DynamicPatcher.Program.Active();
            while (true)
            {
                Thread.Sleep(1000);
            }
        }
    }
}
