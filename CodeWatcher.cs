using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicPatcher
{
    /// <summary>Listens to the code change notifications and raises events when a code file in a directory, changes.</summary>
    public class CodeWatcher
    {
        /// <summary>The action before watching path.</summary>
        public Action<string> FirstAction;

        /// <summary>Occurs when a code file is created, deleted or changed.</summary>
        public event FileSystemEventHandler OnCodeChanged;

        private List<FileSystemWatcher> watchers = new List<FileSystemWatcher>();

        Dictionary<string, TimeSpan> lastModifications = new Dictionary<string, TimeSpan>();
        Stopwatch stopwatch = new Stopwatch();
        string workDir;

        /// <summary>Initializes a new instance of the CodeWatcher class.</summary>
        public CodeWatcher(string path)
        {
            workDir = path;
            stopwatch.Start();
        }

        /// <summary>Create a thread watching any changes of directory.</summary>
        public Task StartWatchPath()
        {
            Task firstTask = Task.Run(() =>
            {
                FirstAction(workDir);
            });

            Task.Run(() =>
            {
                Logger.Log("waiting for first action to complete");
                firstTask.Wait();
                Logger.Log("first action complete!");

                WatchPath();
            });

            return firstTask;
        }

        /// <summary>Watch any changes of directory.</summary>
        private void WatchPath()
        {
            if (Directory.Exists(workDir) == false)
            {
                Logger.LogError("direction not exists: " + workDir);
                return;
            }

            var watcher = new FileSystemWatcher(workDir, "*.cs");

            watcher.Created += new FileSystemEventHandler(OnFileChanged);
            watcher.Changed += new FileSystemEventHandler(OnFileChanged);
            watcher.Deleted += new FileSystemEventHandler(OnFileChanged);
            //watcher.Renamed += new RenamedEventHandler(OnRenamed);
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;

            watchers.Add(watcher);
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            string path = e.FullPath;
            if (IsFileChanged(path) == false)
            {
                return;
            }

            OnCodeChanged.Invoke(sender, e);
        }

        private bool IsFileChanged(string path)
        {
            if (lastModifications.ContainsKey(path))
            {
                if (stopwatch.Elapsed - lastModifications[path] <= TimeSpan.FromSeconds(3.0))
                {
                    return false;
                }
                lastModifications[path] = stopwatch.Elapsed;
            }
            else
            {
                lastModifications.Add(path, stopwatch.Elapsed);
            }
            return true;
        }
    }
}
