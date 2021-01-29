using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json.Linq;

namespace DynamicPatcher
{
    class CompilationManager
    {
        CSharpCompilation compilation;

        public CompilationManager()
        {
        }

        public void Load(JObject json)
        {
            var configs = json["compiler"];

            // load references
            List<MetadataReference> metadataReferences = new List<MetadataReference>();

            metadataReferences.Add(MetadataReference.CreateFromFile(Helpers.GetAssemblyPath("DynamicPatcher.dll")));
            metadataReferences.Add(MetadataReference.CreateFromFile(Helpers.GetAssemblyPath("mscorlib.dll")));

            var references = configs["references"].ToArray();
            foreach (var token in references)
            {
                string path = Helpers.GetAssemblyPath(token.ToString());
                MetadataReference metadata = MetadataReference.CreateFromFile(path);

                metadataReferences.Add(metadata);
            }


            CSharpCompilationOptions compilationOptions = new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                allowUnsafe: true,
                optimizationLevel: OptimizationLevel.Debug,
                platform: Platform.AnyCpu,
                warningLevel: 4
                );

            compilation = CSharpCompilation.Create(null, references: metadataReferences, options: compilationOptions);


            ShowCompilerConfig();
        }

        private void ShowCompilerConfig()
        {
            Logger.Log("ReferencedAssemblies: ");
            foreach (MetadataReference metadata in compilation.References)
            {
                Logger.Log(metadata.Display);
            }
            Logger.Log("");

            CSharpCompilationOptions compilationOptions = compilation.Options;
            Logger.Log("CompilerOptions: ");
            Logger.Log("AllowUnsafe: " + compilationOptions.AllowUnsafe);
            Logger.Log("WarningLevel: " + compilationOptions.WarningLevel);
            Logger.Log("Platform: " + compilationOptions.Platform);
            Logger.Log("OptimizationLevel: " + compilationOptions.OptimizationLevel);
            Logger.Log("OutputKind: " + compilationOptions.OutputKind);
            Logger.Log("LanguageVersion: " + compilation.LanguageVersion);

            Logger.Log("");
        }

        public Assembly Compile(string path)
        {
            Logger.Log("compiling: " + path);

            using (FileStream file = File.OpenRead(path))
            {
                SourceText source = SourceText.From(file);
                SyntaxTree tree = CSharpSyntaxTree.ParseText(source);

                string fileName = Path.GetFileNameWithoutExtension(path);
                Compilation compiler = compilation.AddSyntaxTrees(tree).WithAssemblyName(fileName);

                string outputPath = Path.ChangeExtension(path, "tmp");
                var result = compiler.Emit(outputPath);

                Logger.Log("compiler output: ");
                foreach (Diagnostic diagnostic in result.Diagnostics)
                {
                    string message = path + diagnostic.ToString();
                    Logger.Log(message);
                }
                Logger.Log("");

                if (result.Success)
                {
                    Logger.Log("compile succeed!");
                    try
                    {
                        Logger.Log("loading complied assembly");
                        using MemoryStream memory = new MemoryStream();
                        using (FileStream tmpFile = File.OpenRead(outputPath))
                        {
                            tmpFile.CopyTo(memory);
                        }

                        Assembly assembly = Assembly.Load(memory.ToArray());
                        return assembly;
                    }
                    finally
                    {
                        Logger.Log("delete temp file: " + outputPath);
                        File.Delete(outputPath);
                        Logger.Log("");
                    }

                }
            }

            Logger.Log("compiler error!");
            Logger.Log("");
            return null;
        }
    }
}
