using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DynamicPatcher
{
    class PackageManager
    {
        string workDirectory;

        public PackageManager(string workDir)
        {
            workDirectory = workDir;
        }

        private string GetPackagePath(string path)
        {
            string outputPath = path.Replace(workDirectory, Path.Combine(workDirectory, "Packages"));
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            outputPath = Path.ChangeExtension(outputPath, "pkg");
            return outputPath;
        }

        const string github = "https://github.com/Xkein";
        const string bilibili = "https://space.bilibili.com/84479377/";

        readonly byte[] key = Encoding.Default.GetBytes(github + bilibili);

        public byte[] GetKeyMD5()
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            return md5.ComputeHash(key);
        }

#if DEVMODE
        public void Pack(string path)
        {
            using (FileStream package = File.Create(GetPackagePath(path)))
            {
                using FileStream file = File.OpenRead(path);

                using MemoryStream memory = new MemoryStream();
                file.CopyTo(memory);
                byte[] data = memory.ToArray();

                Logger.Log("packing {0} into {1}", file.Name, package.Name);
                Logger.Log("");

                TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider();
                des.Key = GetKeyMD5();
                des.Mode = CipherMode.ECB;

                using CryptoStream cs = new CryptoStream(package, des.CreateEncryptor(), CryptoStreamMode.Write);
                cs.Write(data, 0, data.Length);
                cs.FlushFinalBlock();
            }
        }
#else
        public void UnPack(string outputPath)
        {
            using (FileStream output = File.Create(outputPath))
            {
                using FileStream package = File.OpenRead(GetPackagePath(outputPath));

                using MemoryStream memory = new MemoryStream();
                package.CopyTo(memory);
                byte[] data = memory.ToArray();
                
                Logger.Log("unpacking {0} into {1}", package.Name, output.Name);
                Logger.Log("");

                TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider();
                des.Key = GetKeyMD5();
                des.Mode = CipherMode.ECB;

                using CryptoStream cs = new CryptoStream(output, des.CreateDecryptor(), CryptoStreamMode.Write);
                cs.Write(data, 0, data.Length);
                cs.FlushFinalBlock();
            }
        }
#endif
    }
}
