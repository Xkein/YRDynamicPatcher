using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Reflection;
using PeNet;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace DynamicPatcher
{
    static class Helpers
    {
        private static IntPtr processHandle = IntPtr.Zero;

        static IntPtr ProcessHandle
        {
            get
            {
                if (processHandle == IntPtr.Zero)
                {
                    processHandle = FindProcessHandle();
                }
                return processHandle;
            }
            set => processHandle = value;
        }
        static private IntPtr FindProcessHandle()
        {

            Process[] processes = Process.GetProcesses();
            foreach (var process in processes)
            {
                if (process.Id == Process.GetCurrentProcess().Id)
                {
                    var targetProcess = Process.GetCurrentProcess();
                    try
                    {
                        Logger.Log("find process: {0} ({1})", targetProcess.MainWindowTitle, targetProcess.Id);
                        targetProcess.EnableRaisingEvents = true;
                        targetProcess.Exited += (object sender, EventArgs e) =>
                        {
                            ProcessHandle = IntPtr.Zero;
                            Logger.Log("{0} ({1}) exited.", targetProcess.MainWindowTitle, targetProcess.Id);
                        };
                        return targetProcess.Handle;
                    }
                    catch (Exception e)
                    {
                        Logger.LogError("get process handle error: " + e.Message);
                        throw;
                    }
                }
            }

            throw new InvalidOperationException("could not find process handle");
        }

        static public IntPtr GetProcessHandle()
        {
            return ProcessHandle;
        }

        public static List<string> AdditionalSearchPath { get; } = new List<string>();
        public static string GetValidFullPath(string fileName)
        {
            string fullPath = Path.GetFullPath(fileName);

            // found in root directory
            if (File.Exists(fullPath))
            {
                return fullPath;
            }

            string workDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DynamicPatcher");
            string librariesDirectory = Path.Combine(workDir, "Libraries");
            fullPath = SearchFileInDirectory(librariesDirectory, fileName);
            // found in libraries
            if (File.Exists(fullPath))
            {
                return fullPath;
            }

            foreach (string dir in AdditionalSearchPath)
            {
                fullPath = Path.Combine(dir, fileName);
                // found in additional directory
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }

            string directory = RuntimeEnvironment.GetRuntimeDirectory();
            fullPath = SearchFileInDirectory(directory, fileName);
            // found in runtime directory
            if (File.Exists(fullPath))
            {
                return fullPath;
            }

            // IGNORE REASON: avoid loading dll from unity's plasticSCM that make version crash
            //string variable = Environment.GetEnvironmentVariable("Path");
            //string[] dirs = variable.Split(';');

            //foreach (string dir in dirs)
            //{
            //    fullPath = Path.Combine(dir, fileName);
            //    // found in environment variable directory
            //    if (File.Exists(fullPath))
            //    {
            //        return fullPath;
            //    }
            //}

            return null;
        }

        public static string SearchFileInDirectory(string dir, string fileName)
        {
            string[] files = Directory.GetFiles(dir, fileName, SearchOption.AllDirectories);
            string filePath = files.FirstOrDefault();
            if (string.IsNullOrEmpty(filePath))
            {
                return string.Empty;
            }

            if (filePath.Contains(dir))
            {
                return filePath;
            }

            return Path.Combine(dir, filePath);
        }

        public static Assembly GetLoadedAssembly(string name)
        {
            Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in loadedAssemblies)
            {
                AssemblyName assemblyName = assembly.GetName();
                if (string.Equals(assemblyName.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return assembly;
                }
            }

            return null;
        }

        public static string GetAssemblyPath(string fileName)
        {
            string name = Path.GetFileNameWithoutExtension(fileName);

            Assembly assembly = GetLoadedAssembly(name);
            if (assembly != null)
            {
                return assembly.Location;
            }

            string path = Helpers.GetValidFullPath(fileName);
            return path;
        }

        public static ProcessModule GetProcessModule(string moduleName = null)
        {
            Process process = Process.GetCurrentProcess();
            if (string.IsNullOrEmpty(moduleName))
            {
                return process.MainModule;
            }

            foreach (ProcessModule module in process.Modules)
            {
                if (string.Equals(module.ModuleName, moduleName, StringComparison.OrdinalIgnoreCase))
                {
                    return module;
                }
            }

            throw new DllNotFoundException($"Could not find module '{moduleName}'.");
        }

        public static bool GetProcessModuleAt(int address, out ProcessModule processModule)
        {
            processModule = null;
            Process process = Process.GetCurrentProcess();
            foreach (ProcessModule module in process.Modules)
            {
                if (AddressInModule(address, module))
                {
                    processModule = module;
                    return true;
                }
            }

            return false;
        }

        public static bool AddressInModule(int address, ProcessModule module)
        {
            return (int)module.BaseAddress <= address && address <= (int)module.BaseAddress + module.ModuleMemorySize;
        }

        public static unsafe PeFile GetPE(string moduleName)
        {
            var module = GetProcessModule(moduleName);
            var stream = new UnmanagedMemoryStream((byte*)module.BaseAddress.ToPointer(), module.ModuleMemorySize);
            if (PeFile.TryParse(stream, out var peFile, true))
            {
                return peFile;
            }

            throw new Exception($"Could not get PE '{moduleName}'.");
        }

        public static int GetEATAddress(string moduleName, string funcName)
        {
            var pe = Helpers.GetPE(moduleName);
            var function = Array.Find(pe.ExportedFunctions, f => f.Name == funcName);

            if (function == null)
            {
                throw new KeyNotFoundException($"could not find {moduleName} in export table of {funcName}.");
            }

            var exportDir = pe.ImageExportDirectory;
            // int address = (int)(pe.ImageNtHeaders.OptionalHeader.ImageBase + function.Address);
            var pFunctionOffset = pe.ImageNtHeaders.OptionalHeader.ImageBase + exportDir.AddressOfFunctions;
            int address = (int)(pFunctionOffset + sizeof(uint) * (function.Ordinal - exportDir.Base));

            return address;
        }

        public static int GetIATAddress(string moduleName, string funcName)
        {
            var pe = Helpers.GetPE(moduleName);
            var function = Array.Find(pe.ImportedFunctions, f => f.Name == funcName);

            if (function == null)
            {
                throw new KeyNotFoundException($"could not find {moduleName} in import table of {funcName}.");
            }

            var iat = pe.ImageNtHeaders.OptionalHeader.DataDirectory[(int)PeNet.Header.Pe.DataDirectoryType.IAT];
            int address = (int)(pe.ImageNtHeaders.OptionalHeader.ImageBase + iat.VirtualAddress + function.IATOffset);

            return address;
        }

        // System.Linq.Expressions.Compiler.AssemblyGen from System.Core.dll
        private sealed class AssemblyGen
        {
            private static AssemblyGen s_assembly;

            private readonly ModuleBuilder _myModule;

            private int _index;

            private static AssemblyGen Assembly
            {
                get
                {
                    if (s_assembly == null)
                    {
                        Interlocked.CompareExchange(ref s_assembly, new AssemblyGen(), comparand: null);
                    }
                    return s_assembly;
                }
            }

            private AssemblyGen()
            {
                var name = new AssemblyName("Snippets");

                AssemblyBuilder myAssembly = AssemblyBuilder.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
                _myModule = myAssembly.DefineDynamicModule(name.Name!);
            }

            private TypeBuilder DefineType(string name, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type parent, TypeAttributes attr)
            {
                ArgumentNullException.ThrowIfNull(name);
                ArgumentNullException.ThrowIfNull(parent);

                StringBuilder sb = new StringBuilder(name);

                int index = Interlocked.Increment(ref _index);
                sb.Append('$');
                sb.Append(index);

                // An unhandled Exception: System.Runtime.InteropServices.COMException (0x80131130): Record not found on lookup.
                // is thrown if there is any of the characters []*&+,\ in the type name and a method defined on the type is called.
                sb.Replace('+', '_').Replace('[', '_').Replace(']', '_').Replace('*', '_').Replace('&', '_').Replace(',', '_').Replace('\\', '_');

                name = sb.ToString();

                return _myModule.DefineType(name, attr, parent);
            }

            internal static TypeBuilder DefineDelegateType(string name)
            {
                return Assembly.DefineType(
                    name,
                    typeof(MulticastDelegate),
                    TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.AnsiClass | TypeAttributes.AutoClass
                );
            }
        }

        private static readonly Type[] _DelegateCtorSignature = new Type[]
        {
            typeof(object),
            typeof(IntPtr)
        };

        public static Type GetMethodDelegateType(MethodInfo info)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            var parameters = info.GetParameters();
            Type returnType = info.ReturnParameter.ParameterType;
            Type[] parameterTypes = (from p in parameters select p.ParameterType).ToArray();

            TypeBuilder typeBuilder = AssemblyGen.DefineDelegateType($"{info.Name}+Delegate{parameterTypes.Length}(AutoGenerated)");
            typeBuilder.DefineConstructor(MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.RTSpecialName, CallingConventions.Standard,
                _DelegateCtorSignature).SetImplementationFlags(MethodImplAttributes.CodeTypeMask);
            typeBuilder.DefineMethod("Invoke", MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.VtableLayoutMask, returnType, parameterTypes).SetImplementationFlags(MethodImplAttributes.CodeTypeMask);
            return typeBuilder.CreateType();

            ////var parameterExpressions = (from p in parameters select Expression.Parameter(p.ParameterType, p.Name)).ToArray();
            ////var lambda = Expression.Lambda(Expression.Call(info, parameterExpressions), $"{info.Name}'s Lambda(Auto Generated)", parameterExpressions);
            ////var dlg = lambda.Compile();
            ////return dlg.GetType();

            //parameterTypes = (parameterTypes.Concat(new Type[] { info.ReturnParameter.ParameterType })).ToArray();
            //return Expression.GetDelegateType(parameterTypes);
        }
    }
};


