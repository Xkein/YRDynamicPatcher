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
        public PackageManager(string workDir)
        {
            workDirectory = workDir;
        }

        public string[] PackedList => packedAssemblies.ToArray();

        private string PackagesDirectory => Path.Combine(workDirectory, "Packages");

        private string GetPackagePath(string path)
        {
            string outputPath = path.Replace(workDirectory, PackagesDirectory);
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

                packedAssemblies.Add(path);
                WritePackedList();
            }
        }
#else
        public void UnPack(string outputPath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
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
        string GetPackedListFilePath()
        {
            return Path.Combine(workDirectory, "Packages", "release.list");
        }

        public void ReadPackedList()
        {
            string mainList = GetPackedListFilePath();
            string[] lines = File.ReadAllLines(mainList);
            packedAssemblies = new HashSet<string>(lines);

            // Read additional packed list
            string[] packedLists = Directory.GetFiles(PackagesDirectory, "*.list");
            foreach (string list in packedLists)
            {
                if(list != mainList)
                {
                    lines = File.ReadAllLines(list);
                    foreach (string line in lines)
                    {
                        packedAssemblies.Add(line);
                    }
                }
            }

            packedAssemblies = packedAssemblies.Select(p => p.Replace("{DP_DIR}", workDirectory)).ToHashSet();
        }

        public void WritePackedList()
        {
            File.WriteAllLines(GetPackedListFilePath(), packedAssemblies.Select(p => p.Replace(workDirectory, "{DP_DIR}")));
        }

        private HashSet<string> packedAssemblies = new();
        string workDirectory;

    }
}
