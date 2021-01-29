using Microsoft.CSharp;
using Newtonsoft.Json.Linq;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DynamicPatcher
{
    class Compiler
    {
        CodeDomProvider Provider { get; set; }
        public CompilerParameters Parameters { get; set; }
        public Compiler()
        {
            string language = "CSharp";
            Provider = CodeDomProvider.CreateProvider(language);
            CompilerInfo langCompilerInfo = CodeDomProvider.GetCompilerInfo(language);
            Parameters = langCompilerInfo.CreateDefaultCompilerParameters();

            Parameters.ReferencedAssemblies.Add(GetType().Assembly.Location);
            Parameters.GenerateExecutable = false;
            Parameters.GenerateInMemory = true;
            Parameters.IncludeDebugInformation = false;
            Parameters.WarningLevel = 4;
        }

        public void Load(JObject json)
        {
            var configs = json["compiler"];
            var references = configs["references"].ToArray();

            foreach (var token in references)
            {
                Parameters.ReferencedAssemblies.Add(token.ToString());
            }

            var options = configs["compiler_options"].ToArray();

            foreach (var token in options)
            {
                Parameters.CompilerOptions += token.ToString() + " ";
            }

            ShowCompilerConfig();
        }

        private void ShowCompilerConfig()
        {
            Logger.Log("ReferencedAssemblies: ");
            foreach (var assembly in Parameters.ReferencedAssemblies)
            {
                Logger.Log(assembly);
            }
            Logger.Log("");

            Logger.Log("CompilerOptions: ");
            Logger.Log(Parameters.CompilerOptions);
            Logger.Log("");

            Logger.Log("IncludeDebugInformation: ");
            Logger.Log(Parameters.IncludeDebugInformation);
            Logger.Log("");

            Logger.Log("TreatWarningsAsErrors: ");
            Logger.Log(Parameters.TreatWarningsAsErrors);
            Logger.Log("");

            Logger.Log("WarningLevel: ");
            Logger.Log(Parameters.WarningLevel);
            Logger.Log("");

            Logger.Log("CoreAssemblyFileName: ");
            Logger.Log(Parameters.CoreAssemblyFileName);
            Logger.Log("");

            Logger.Log("EmbeddedResources: ");
            foreach (var resource in Parameters.EmbeddedResources)
            {
                Logger.Log(resource);
            }
            Logger.Log("");

            Logger.Log("LinkedResources: ");
            foreach (var resource in Parameters.LinkedResources)
            {
                Logger.Log(resource);
            }
            Logger.Log("");

            //Logger.Log("Win32Resource: ");
            //Logger.Log(Parameters.Win32Resource);
            //Logger.Log("");

            //Logger.Log("MainClass: ");
            //Logger.Log(Parameters.MainClass);
            //Logger.Log("");

            //Logger.Log("OutputAssembly: ");
            //Logger.Log(Parameters.OutputAssembly);
            //Logger.Log("");
        }

        public Assembly Compile(string path)
        {
            Logger.Log("compiling: " + path);
            
            CompilerResults results = Provider.CompileAssemblyFromFile(Parameters, path);

            Logger.Log("compiler output: ");
            foreach (string str in results.Output)
            {
                Logger.Log(str);
            }
            Logger.Log("");

            if (results.Errors.HasErrors)
            {
                Logger.Log("compiler errors: ");
                foreach (CompilerError e in results.Errors)
                {
                    Logger.Log(e.ErrorText);
                }
                return null;
            }

            Logger.Log("compile succeed: " + path);

            return results.CompiledAssembly;
        }
    }
}
