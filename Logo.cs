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
            string path = Path.Combine("DynamicPatcher", "logo");
            if (File.Exists(path) == false)
            {
                using StreamWriter write = new StreamWriter(File.OpenWrite(path));
                write.Write(
                    "LLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLL\n" +
                    "LLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLL\n" +
                    "LLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLL\n" +
                    "LLLLLL DynamicPatcher LLLLLLLLLLLLLLLLLLLLLLLLLLLLGLi::;1fGGLLLf1;.,:;1LGLLLLLLLLLLLLLLLLL\n" +
                    "LLLLLL    Activated   LLLLLLLLLLLLLLLLLLLLLLLLGi              tGLLLLGt     LLLLLLLLLLLLLLL\n" +
                    "LLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLG.                  .GLLLLG      GLLLLLLLLLLLL\n" +
                    "LLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLG                      GLLLLG      GLLLLLLLLLLL\n" +
                    "LLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLG          tL1          LLLLLG.     1LLLLLLLLLLL\n" +
                    "LLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLG         GLLLLLf        LLLLLL,      GLLLLLLLLLL\n" +
                    "LLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLL:        tLLLLLLG        LLLLLL:      GLLLLLLLLLL\n" +
                    "LLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLt        :LLLLLLLG        LLLLLL:      GLLLLLLLLLL\n" +
                    "LLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLG         GLLLLLLLG        LLLLLL:      GLLLLLLLLLL\n" +
                    "LLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLG         GLLLLLLLLG        LLLLLL:      GLLLLLLLLLL\n" +
                    "LLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLL.        LLLLLLLLLLG        LLLLLL:      GLLLLLLLLLL\n" +
                    "LLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLL1        iLLLLLLLLLLG        LLLLLL:      GLLLLLLLLLL\n" +
                    "LLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLL        .LLLLLLLLLLLt        LLLLLL:      GLLLLLLLLLL\n" +
                    "LLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLG         GLL:,,,,,,           LLLLLL:      GLLLLLLLLLL\n" +
                    "LLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLG         Gt                    LLLLLL:      GLLLLLLLLLL\n" +
                    "LLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLL;        fG                     GLLLLL:      GLLLLLLLLLL\n" +
                    "LLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLf        ;LLG                   LLLLLLL.      GLLLLLLLLLL\n" +
                    "LLLLLLLLLLLLLLLLG1.        1GLLLG         GLLLLLGGGGGGGGGGGGGGGGLLLLLLL;      :LLLLLLLLLLL\n" +
                    "LLLLLLLLLLLLLG               iLG         GLLLLLLLLffffffffffffffffffi         GLLLLLLLLLLL\n" +
                    "LLLLLLLLLLLG.                 G,        LLLLLLLLG                            GLLLLLLLLLLLL\n" +
                    "LLLLLLLLLLL:                 Gi        iLLLLLLLG                           iLLLLLLLLLLLLLL\n" +
                    "LLLLLLLLLLG        :GGGGGGGLLL        .LLLLLLLG                          iGLLLLLLLLLLLLLLL\n" +
                    "LLLLLLLLLLG        iLLLLLLLLG         GLLLLLLL;                       ;GLLLLLLLLLLLLLLLLLL\n" +
                    "LLLLLLLLLLG        iLLLLLLLG         GLLLLLLLt         iLGGf1;;i1LGGLLLLLLLLLLLLLLLLLLLLLL\n" +
                    "LLLLLLLLLLG        iLLLLLLL;        tLLLLLLLL         ,LLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLL\n" +
                    "LLLLLLLLLLG        iLLLLLLf        ,LLLLLLLG          GLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLL\n" +
                    "LLLLLLLLLLG        iLLLLLG         GLLLLLLG          GLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLL\n" +
                    "LLLLLLLLLLG        iLLLLG         GLLLLLLL,         LLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLL\n" +
                    "LLLLLLLLLLG        iLLLL:        fLLLLLLL1         1LLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLL\n" +
                    "LLLLLLLLLLG        iLLLf        :LLLLLLLL         ,LLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLL\n" +
                    "LLLLLLLLLLG         iL,         GLLLLLLG          GLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLL\n" +
                    "LLLLLLLLLLLi                   GLLLLLLG          GLLLLLLLLLLLLLL By:         LLLLLLLLLLLLL\n" +
                    "LLLLLLLLLLLL1                .GLLLLLLL:         LLLLLLLLLLLLLLLL      Xkein  LLLLLLLLLLLLL\n" +
                    "LLLLLLLLLLLLLLt            .GLLLLLLLLi         GLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLL\n" +
                    "LLLLLLLLLLLLLLLLLGG1;:;tGGLLLLLLLLLLLLLG1;;tGLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLL\n" +
                    "LLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLL\n" +
                    "LLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLL\n" +
                    "LLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLL\n"
                    );
                write.Flush();
            }
            using StreamReader reader = new StreamReader(File.OpenRead(path));
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
