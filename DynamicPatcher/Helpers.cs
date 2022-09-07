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
            private static AssemblyGen Assembly
            {
                get
                {
                    if (AssemblyGen._assembly == null)
                    {
                        System.Threading.Interlocked.CompareExchange<AssemblyGen>(ref AssemblyGen._assembly, new AssemblyGen(), null);
                    }
                    return AssemblyGen._assembly;
                }
            }

            private AssemblyGen()
            {
                AssemblyName assemblyName = new AssemblyName("DPSnippets");
                CustomAttributeBuilder[] assemblyAttributes = new CustomAttributeBuilder[]
                {
                new CustomAttributeBuilder(typeof(System.Security.SecurityTransparentAttribute).GetConstructor(Type.EmptyTypes), new object[0])
                };
                this._myAssembly = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run, assemblyAttributes);
                this._myModule = this._myAssembly.DefineDynamicModule(assemblyName.Name, false);
                this._myAssembly.DefineVersionInfoResource();
            }

            private TypeBuilder DefineType(string name, Type parent, TypeAttributes attr)
            {
                if (name == null)
                {
                    throw new ArgumentNullException(nameof(name));
                }
                if (parent == null)
                {
                    throw new ArgumentNullException(nameof(parent));
                }

                StringBuilder stringBuilder = new StringBuilder(name);
                int value = System.Threading.Interlocked.Increment(ref this._index);
                stringBuilder.Append("$");
                stringBuilder.Append(value);
                stringBuilder.Replace('+', '_').Replace('[', '_').Replace(']', '_').Replace('*', '_').Replace('&', '_').Replace(',', '_').Replace('\\', '_');
                name = stringBuilder.ToString();
                return this._myModule.DefineType(name, attr, parent);
            }

            internal static TypeBuilder DefineDelegateType(string name)
            {
                return AssemblyGen.Assembly.DefineType(name, typeof(MulticastDelegate), TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.AutoClass);
            }

            private static AssemblyGen _assembly;
            private readonly AssemblyBuilder _myAssembly;
            private readonly ModuleBuilder _myModule;
            private int _index;
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


