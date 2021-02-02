using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DynamicPatcher
{
    class CompilationManager
    {
        // for undependent file
        CSharpCompilation compilation;
        CSharpCompilationOptions compilationOptions;

        Solution solution;
        AdhocWorkspace workspace;

        string workDirectory;

        public CompilationManager(string workDir)
        {
            workDirectory = workDir;

            compilationOptions = new CSharpCompilationOptions(
                            OutputKind.DynamicallyLinkedLibrary,
                            allowUnsafe: true,
                            optimizationLevel: OptimizationLevel.Debug,
                            platform: Platform.AnyCpu,
                            warningLevel: 4
                            );

            var dirInfo = new DirectoryInfo(workDir);
            var dirList = dirInfo.GetDirectories("*.*", SearchOption.AllDirectories).ToList();
            var solutionlist = dirInfo.GetFiles("*.sln", SearchOption.AllDirectories).ToList();

            if (solutionlist.Count >= 0)
            {
                workspace = new AdhocWorkspace();

                LoadSolution(solutionlist[0].FullName);

                //foreach (WorkspaceDiagnostic diagnostic in workspace)
                //{
                //    Logger.Log(diagnostic.ToString());
                //}
                //Logger.Log("");

                // compile all project first to make sure all undependent file has references
                ProjectDependencyGraph dependencyGraph = solution.GetProjectDependencyGraph();
                foreach (ProjectId projectId in dependencyGraph.GetTopologicallySortedProjects())
                {
                    Project project = solution.GetProject(projectId);
                    CompileProject(project);
                }
            }
            else
            {
                Logger.Log("solution not found");
            }

            LoadConfig(workDir);
        }

        private void LoadConfig(string workDir)
        {
            StreamReader file = File.OpenText(Path.Combine(workDir, "compiler.config.json"));
            JsonTextReader reader = new JsonTextReader(file);
            JObject json = JObject.Load(reader);
            var configs = json;

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

            compilation = CSharpCompilation.Create(null, references: metadataReferences, options: compilationOptions);

            ShowCompilerConfig();
        }

        private void LoadSolution(string path)
        {
            Logger.Log("loading solution: " + path);
            string dir = Path.GetDirectoryName(path);

            SolutionId solutionId = SolutionId.CreateNewId();
            VersionStamp version = VersionStamp.Create();
            SolutionInfo solutionInfo = SolutionInfo.Create(solutionId, version, path);
            solution = workspace.AddSolution(solutionInfo);

            using (FileStream file = File.OpenRead(path))
            {
                using StreamReader reader = new StreamReader(file);
                while (reader.EndOfStream == false)
                {
                    string line = reader.ReadLine();
                    if (line.StartsWith("Project"))
                    {
                        string pattern = @"^Project\(""\{.+?\}""\) = ""(\w+?)"", ""(.+?)"", ""\{(.+?)\}""";
                        Match match = Regex.Match(line, pattern);

                        string projectName = match.Groups[1].Value;
                        string projectPath = Path.Combine(dir, match.Groups[2].Value);
                        string projectGuid = match.Groups[3].Value;

                        LoadProject(projectName, projectPath, ProjectId.CreateFromSerialized(Guid.Parse(projectGuid)));
                    }
                }
            }
        }

        private void LoadProject(string name, string path, ProjectId projectId)
        {
            Logger.Log("loading project: " + path);

            string projectDirectory = Path.GetDirectoryName(path);

            VersionStamp version = VersionStamp.Create();
            ProjectInfo projectInfo = ProjectInfo.Create(projectId, version, name, name, LanguageNames.CSharp, filePath: path, compilationOptions: compilationOptions);
            Project project = workspace.AddProject(projectInfo);

            XmlDocument doc = new XmlDocument();
            doc.Load(path);

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("ms", "http://schemas.microsoft.com/developer/msbuild/2003");

            List<MetadataReference> metadataReferences = new List<MetadataReference>();
            metadataReferences.Add(MetadataReference.CreateFromFile(Helpers.GetAssemblyPath("mscorlib.dll")));
            foreach (XmlElement reference in doc.SelectNodes(@"ms:Project/ms:ItemGroup/ms:Reference", nsmgr))
            {
                var hintPathElement = reference.GetElementsByTagName("HintPath");
                string hintPath = hintPathElement.Count > 0 ? Path.Combine(projectDirectory, hintPathElement[0].InnerText) : Helpers.GetAssemblyPath(reference.GetAttribute("Include") + ".dll");
                MetadataReference metadata = MetadataReference.CreateFromFile(hintPath);
                metadataReferences.Add(metadata);
            }
            project = project.WithMetadataReferences(metadataReferences);

            List<ProjectReference> projectReferences = new List<ProjectReference>();
            foreach (XmlElement reference in doc.SelectNodes(@"ms:Project/ms:ItemGroup/ms:ProjectReference", nsmgr))
            {
                string referenceProjectPath = reference.GetAttribute("Include");
                string guid = reference.GetElementsByTagName("Project")[0].InnerText;
                ProjectId id = ProjectId.CreateFromSerialized(Guid.ParseExact(guid, "B"));

                ProjectReference projectReference = new ProjectReference(id);
                projectReferences.Add(projectReference);
            }
            project = project.WithProjectReferences(projectReferences);

            foreach (XmlElement compile in doc.SelectNodes(@"ms:Project/ms:ItemGroup/ms:Compile", nsmgr))
            {
                string documentName = compile.GetAttribute("Include");
                string documentPath = Path.Combine(projectDirectory, documentName);
                using (FileStream file = File.OpenRead(documentPath))
                {
                    SourceText source = SourceText.From(file);
                    Document document = project.AddDocument(documentName, source, filePath: documentPath);
                    project = document.Project;
                }
            }


            workspace.TryApplyChanges(project.Solution);
            solution = workspace.CurrentSolution;

            string buildPath = GetOutputPath(projectDirectory);
            Helpers.AdditionalSearchPath.Add(buildPath);
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

        public Project GetProjectFromFile(string path)
        {
            foreach (Project project in solution.Projects)
            {
                foreach (Document document in project.Documents)
                {
                    if (document.FilePath == path)
                    {
                        return project;
                    }
                }
            }

            return null;
        }

        public Assembly Compile(string path)
        {
            Project project = GetProjectFromFile(path);
            if(project != null)
            {
                return CompileProject(project);
            }

            return CompileFile(path);
        }

        private string GetOutputPath(string path)
        {
            string outputPath = path.Replace(workDirectory, Path.Combine(workDirectory, "Build"));
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            return outputPath;
        }

        public Assembly CompileFile(string path)
        {
            Logger.Log("compiling: " + path);

            using (FileStream file = File.OpenRead(path))
            {
                SourceText source = SourceText.From(file);
                SyntaxTree tree = CSharpSyntaxTree.ParseText(source);

                string fileName = Path.GetFileNameWithoutExtension(path);
                Compilation compiler = compilation.AddSyntaxTrees(tree).WithAssemblyName(fileName);

                string outputPath = GetOutputPath(Path.ChangeExtension(path, "tmp"));
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
                    Logger.Log("compile file succeed!");
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

        public Assembly CompileProject(Project project)
        {
            Logger.Log("compiling project: " + project.FilePath);

            Compilation projectCompilation = project.GetCompilationAsync().Result;

            string outputPath = GetOutputPath(Path.ChangeExtension(project.FilePath, "dll"));
            var result = projectCompilation.Emit(outputPath);

            Logger.Log("compiler output: ");
            foreach (Diagnostic diagnostic in result.Diagnostics)
            {
                string message = "" + diagnostic.ToString();
                Logger.Log(message);
            }
            Logger.Log("");

            if (result.Success)
            {
                Logger.Log("compile project '{0}' succeed!", project.Name);
                Logger.Log("loading complied assembly");
                Logger.Log("");
                Assembly assembly = Assembly.LoadFrom(outputPath);
                return assembly;
            }

            Logger.Log("compiler error!");
            Logger.Log("");
            return null;
        }
    }
}
