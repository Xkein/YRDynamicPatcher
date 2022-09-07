using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

        private static List<FileSystemWatcher> _watchers = new List<FileSystemWatcher>();

        /// <summary>Initializes a new instance of the CodeWatcher class.</summary>
        public CodeWatcher(string path, string filter = "*.cs")
        {
            _workDir = path;
            _filter = filter;
            _stopwatch.Start();
        }

        /// <summary>Create a thread watching any changes of directory.</summary>
        public Task StartWatchPath()
        {
            Task firstTask = Task.Run(() =>
            {
                FirstAction?.Invoke(_workDir);
            });

            Task.Run(() =>
            {
                Logger.Log("{0}: waiting for first action to complete.", Path.Combine(_workDir, "**", _filter));
                firstTask.Wait();
                Logger.Log("{0}: first action complete!", Path.Combine(_workDir, "**", _filter));

                WatchPath();
            });

            return firstTask;
        }

        /// <summary>Stop watching directory.</summary>
        public void Stop()
        {
            if (_watcher != null)
            {
                _watcher.Dispose();
                _watchers.Remove(_watcher);
            }
        }

        /// <summary>Watch any changes of directory.</summary>
        private void WatchPath()
        {
            if (Directory.Exists(_workDir) == false)
            {
                Logger.LogError("direction not exists: " + _workDir);
                return;
            }

            var watcher = _watcher = new FileSystemWatcher(_workDir, _filter);

            watcher.Created += OnFileChanged;
            watcher.Changed += OnFileChanged;
            watcher.Deleted += OnFileChanged;
            watcher.Renamed += OnFileChanged;
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;

            _watchers.Add(_watcher);
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            string path = e.FullPath;
            if (IsFileChanged(path) == false)
            {
                return;
            }

            OnCodeChanged.Invoke(sender, e);
            _lastModifications[path] = _stopwatch.Elapsed;
        }

        private bool IsFileChanged(string path)
        {
            if (_lastModifications.ContainsKey(path))
            {
                if (_stopwatch.Elapsed - _lastModifications[path] <= TimeSpan.FromSeconds(3.0))
                {
                    return false;
                }
                _lastModifications[path] = _stopwatch.Elapsed;
            }
            else
            {
                _lastModifications.Add(path, _stopwatch.Elapsed);
            }
            return true;
        }

        Dictionary<string, TimeSpan> _lastModifications = new Dictionary<string, TimeSpan>();
        Stopwatch _stopwatch = new Stopwatch();
        string _workDir;
        string _filter;
        private FileSystemWatcher _watcher;
    }
}
