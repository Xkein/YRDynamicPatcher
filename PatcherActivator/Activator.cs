using DynamicPatcher;
using System.Reflection;
using System.Runtime.InteropServices;

namespace PatcherActivator
{
    public static class Activator
    {
        [UnmanagedCallersOnly]
        public static void ActivateUnmanaged()
        {
            string dpPath = Path.Combine(Environment.CurrentDirectory, "DynamicPatcher.dll");
            // prevent loading to memory twice
            Assembly.Load("DynamicPatcher, Version=2.0.0.0, Culture=neutral, PublicKeyToken=1a18ce02bf7a1a48");

            Activate();
        }

        private static void Activate()
        {
            PatcherManager.Init();
            PatcherManager.Activate();
        }
    }
}