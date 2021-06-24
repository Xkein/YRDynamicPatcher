using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicPatcher
{
    /// <summary>Represents the Logo for DynamicPatcher.</summary>
    public class Logo
    {
        /// <summary>Get Logo string.</summary>
        public static string GetLogo()
        {
            using FileStream file = File.OpenRead(Path.Combine("DynamicPatcher", "logo"));
            using StreamReader reader = new StreamReader(file);
            string logo = reader.ReadToEnd();
            return logo;
        }
        /// <summary>Show string Logo.</summary>
        public static void ShowLogo()
        {
            string logo = GetLogo();

            Logger.LogWithColor(logo, ConsoleColor.Black, ConsoleColor.White);
        }
    }
}
