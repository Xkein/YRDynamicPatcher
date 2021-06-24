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
                    "  ,                                                                                         \n" +
                    "  G@@@@@@@@@@@@@@G,                                                             ti          \n" +
                    "   .1111tCffG1tC@@@f                                                                        \n" +
                    "      .CG L L    G@@i    ,CC   fC    f@;  i@i :   ,1CCL:,,  f@i  i@L   GG ,   :Cf     f@G:  \n" +
                    "     ,@@L L L    ,@@C   ;.C@GL1@@G   t@@L,,C@G    t .t@@t   t@@ti:@@Gf.G@@i   G@C   1@CiCG  \n" +
                    "   .,t@@1 L L.i1i C@@,    C@G  1@C   1@@,  G@G    it  C@1   1@@,  C@G  ,@@,   G@C   1@C     \n" +
                    " itt@@@@L f L.i1i.C@@,    C@G  1@f   1@@,  G@G   Gt  iC@1   1@@,  C@G  ,@@,   G@C   1@C     \n" +
                    "    ,@@G 1: L     G@@,    C@G i@;    1@@,  G@G  L@t   C@1   1@@,  C@G  ,@@,   G@C   1@C     \n" +
                    "    fC:t@@@@@@@CGtG@G.    C@CL,      t@@i  G@C, L@@iiiC@f   t@@i  C@C  ,@@f,  G@C   C@@Gi;t \n" +
                    "  ;1fCti,,,,:1GC@L       fL,          Lf   .Cf   iG,  iCi   .Gf   iC;   tCi   ,CG     1C;   \n" +
                    "  .                     f                                                                   \n" +
                    "                        ,fftL;                                                              \n" +
                    "    ,,,   ,   ;                                                                             \n" +
                    "  ,  f@@G Lf@@@@G                  .,             Gf@1                                      \n" +
                    "      G@@GG   i@@@1               1@,             ,@@,                                      \n" +
                    "     ;C@@1L     C@@,   ,1CCL:,,  G@@CCC    f@G:  LC@@, iCG      1@f    f@i iCi              \n" +
                    "  ,Gt G@@1GLi,iG@@@,   t .t@@t   1@@,    1@CiCG   1@@Li,C@G   C@tG@@f  t@@i,CL              \n" +
                    "   ,C@@@@1L     C@@,   it  C@1   1@@,    1@C      1@@,  G@G   C@1 :t   1@C                  \n" +
                    "     i@@@1GL:,if@@@,  Gt  iC@1   1@@,    1@C      1@@,  G@G   C@Ci     1@C                  \n" +
                    "      G@@1L     G@@, L@t   C@1   1@@,    1@C      1@@,  G@G   C@1      1@C                  \n" +
                    "   1C@@@@1G@@CL:G@t  L@@iiiC@f   G@@G,i  C@@Gi;t  f@@i  G@G  .C@Cf.1t  G@@Git               \n" +
                    " ft,  G@@1L:1GCC,     iG,  iCi     Gf      1C;     Gt  .CC.    ;CG,      Gi                 \n" +
                    "      G@@1L                                          ,GG:                                   \n" +
                    "      fGGLL                                                                                 \n" +
                    "                               .. ;L,                                                       \n" +
                    "                                1@@,                                                        \n" +
                    "                                1@@,  ,      ..    :                                        \n" +
                    "                               1L@@,1@@@i  ,C@@1 G@@i                                       \n" +
                    "                                1@@i  C@G    C@C, f@C                                       \n" +
                    "                                1@@,  C@G    C@G  i@C                                       \n" +
                    "                                1@@,  C@G    C@G  GC.                                       \n" +
                    "                                t@@t  C@G    C@G,Ci                                         \n" +
                    "                                 ,fC@@C1.    CCt                                            \n" +
                    "                                           ;i                                               \n" +
                    "                                           L,  ,G                                           \n" +
                    "             :11:      ;.                                                                   \n" +
                    "           iCtG@@C.   :@@@G   CL@i                   ti                                     \n" +
                    "               :@@C. .1 :f,   1@@,                                                          \n" +
                    "                :@@Cif       LC@@, i@f      1@f    :Cf   f@;  i@i :                         \n" +
                    "              iGGC@@@GGGGf    1@@it,C@C   C@tG@@f  G@C   t@@L,,C@G                          \n" +
                    "              ,i11f@@C;,,     1@@:  L     C@1 :t   G@C   1@@,  G@G                          \n" +
                    "                 .Li@@C.      1@@:G@@G    C@Ci     G@C   1@@,  G@G                          \n" +
                    "             i   G  1@@C      1@@, .C@G   C@1      G@C   1@@,  G@G                          \n" +
                    "            G@@@C    1@@C;i   1@@t tC@C, .C@Cf.1t  G@C   t@@i  G@C,                         \n" +
                    "             .fC      iCC,     .LG   Gf    ;CG,    ,CG    Lf   .Cf                          \n"
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
