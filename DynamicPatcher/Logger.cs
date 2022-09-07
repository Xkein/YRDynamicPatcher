using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicPatcher
{
    /// <summary>Represents the log behavior for DynamicPatcher.</summary>
    public class Logger
    {
        /// <summary>Represents the method that will handle log message.</summary>
        public delegate void WriteLineDelegate(string str);
        /// <summary>Invoked when log the message.</summary>
        static public WriteLineDelegate WriteLine { get; set; }
        /// <summary>Write format string to logger.</summary>
        static public void Log(string format, params object[] args)
        {
            string str = string.Format(format, args);

            Logger.Log(str);
        }

        /// <summary>Write string to logger.</summary>
        static public void Log(string str)
        {
            WriteLine?.Invoke(str);
        }

        /// <summary>Write object to logger.</summary>
        static public void Log(object obj)
        {
            Logger.Log(obj.ToString());
        }

        /// <summary>Get if PrintException has invoked.</summary>
        public static bool HasException { get; set; } = false;

        /// <summary>Print exception and its InnerException recursively.</summary>
        public static void PrintException(Exception e)
        {
            HasException = true;

            Logger.LogWithColor("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<", ConsoleColor.DarkRed);
            PrintExceptionBase(e);
            Logger.LogWithColor("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<", ConsoleColor.DarkRed);
        }

        private static void PrintExceptionBase(Exception e)
        {
            Logger.LogError("{0} info: ", e.GetType().FullName);
            Logger.LogError("Message: " + e.Message);
            Logger.LogError("Source: " + e.Source);
            Logger.LogError("TargetSite.Name: " + e.TargetSite?.Name);
            Logger.LogError("Stacktrace: " + e.StackTrace);

            if (e is System.Reflection.ReflectionTypeLoadException rtle)
            {
                foreach (var le in rtle.LoaderExceptions)
                {
                    Logger.LogWithColor("--------------------------------------------------------", ConsoleColor.DarkRed);
                    PrintExceptionBase(le);
                }
            }

            if (e.InnerException != null)
            {
                Logger.LogWithColor("--------------------------------------------------------", ConsoleColor.DarkRed);
                PrintExceptionBase(e.InnerException);
            }
        }

        private static object color_locker = new object();
        /// <summary>Write string to logger with color.</summary>
        public static void LogWithColor(string str, ConsoleColor color)
        {
            LogWithColor(str, color, Console.BackgroundColor);
        }

        /// <summary>Write string to logger with ForegroundColor and BackgroundColor.</summary>
        public static void LogWithColor(string str, ConsoleColor fgColor, ConsoleColor bgColor)
        {
            lock (color_locker)
            {
                ConsoleColor originFgColor = Console.ForegroundColor;
                ConsoleColor originBgColor = Console.BackgroundColor;
                Console.ForegroundColor = fgColor;
                Console.BackgroundColor = bgColor;

                Logger.Log(str);

                Console.ForegroundColor = originFgColor;
                Console.BackgroundColor = originBgColor;
            }
        }

        /// <summary>Write format string to logger with error state.</summary>
        static public void LogError(string format, params object[] args)
        {
            string str = string.Format(format, args);

            Logger.LogError(str);
        }

        /// <summary>Write string to logger with error state.</summary>
        static public void LogError(string str)
        {
            Logger.LogWithColor("[Error] " + str, ConsoleColor.Red);
        }

        /// <summary>Write object to logger with error state.</summary>
        static public void LogError(object obj)
        {
            Logger.LogError(obj.ToString());
        }

        /// <summary>Write format string to logger with warning state.</summary>
        static public void LogWarning(string format, params object[] args)
        {
            string str = string.Format(format, args);

            Logger.LogWarning(str);
        }

        /// <summary>Write string to logger with warning state.</summary>
        static public void LogWarning(string str)
        {
            Logger.LogWithColor("[Warning] " + str, ConsoleColor.Yellow);
        }

        /// <summary>Write object to logger with warning state.</summary>
        static public void LogWarning(object obj)
        {
            Logger.LogWarning(obj.ToString());
        }
    }
}
