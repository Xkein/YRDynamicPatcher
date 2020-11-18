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
            var references = json["references"].ToArray();
            Logger.Log("references: ");

            foreach (var token in references)
            {
                Parameters.ReferencedAssemblies.Add(token.ToString());
                Logger.Log(token.ToString());
            }

            Logger.Log("");


            var options = json["compiler_options"].ToArray();
            Logger.Log("compiler_options: ");

            foreach (var token in options)
            {
                Parameters.CompilerOptions += token.ToString();
                Logger.Log(token.ToString());
            }

            Logger.Log("");
        }

        public Assembly Compile(string path)
        {
            Logger.Log("compiling: " + path);
 ;
            
            CompilerResults results = Provider.CompileAssemblyFromFile(Parameters, path);

            if (results.Errors.HasErrors)
            {
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
