using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            var _ = new DynamicPatcher.Program();
            while (true)
            {
                Thread.Sleep(1000);
            }
        }
    }
}
