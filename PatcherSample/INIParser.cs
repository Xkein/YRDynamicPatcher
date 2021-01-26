using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatcherSample
{
    class Parser<T>
    {
        public static int Parse(byte[] buffer, ref T outValue)
        {
            string str = Encoding.UTF8.GetString(buffer);
            str = str.Trim('\0').Trim();

            Type type = typeof(T);
            if(type == typeof(string))
            {
                outValue = (T)Convert.ChangeType(str, typeof(T));
            }

            return 1;
        }
    }
    class INI_EX
    {
        Pointer<CCINIClass> IniFile;
        static byte[] readBuffer = new byte[2048];

        public INI_EX(Pointer<CCINIClass> pIniFile)
        {
            IniFile = pIniFile;
        }

        public byte[] value()
        {
            return readBuffer;
        }

        public int max_size()
        {
            return readBuffer.Length;
        }

        public bool empty()
        {
            return readBuffer[0] == 0;
        }

        // basic string reader
        public int ReadString(string pSection, string pKey)
        {
            return IniFile.Ref.ReadString(pSection, pKey, "", value(), max_size());
        }

        public bool Read<T>(string section, string key, ref T pBuffer, int Count = 1)
        {
            if (ReadString(section, key) > 0)
            {
                return Parser<T>.Parse(value(), ref pBuffer) == Count;
            }
            return false;
        }
    }
}
