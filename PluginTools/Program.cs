using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace PluginTools
{
    class Program
    {
        static void Main(string[] args)
        {
            string songs_folder = null;

            while (true)
            {
                Console.Write("请先输入osu! Song文件夹:");
                songs_folder = Console.ReadLine();

                if ((!string.IsNullOrWhiteSpace(songs_folder))&&Directory.Exists(songs_folder))
                    break;

                Console.Clear();
            }

            while (true)
            {
                Console.WriteLine("(1) 枚举现在修改的备份osu文件");
                Console.WriteLine("(2) 自动恢复所有备份的osu文件");

                switch (Console.ReadLine().Trim())
                {
                    case "1":
                        EnumBackupFile(songs_folder);
                        break;
                    case "2":
                        RestoreAll(songs_folder);
                        break;
                    default:
                        Console.WriteLine("未知命令。");
                        break;
                }

                Console.WriteLine("按回车键继续");
                Console.ReadLine();
            }
        }

        private static void EnumBackupFile(string songs_folder)
        {
            foreach (var file in Directory.GetFiles(songs_folder, "*.osu_", SearchOption.AllDirectories))
            {
                var info = new FileInfo(file);

                Console.WriteLine($"{file}");
            }
        }

        private static void RestoreAll(string songs_folder)
        {
            foreach (var file in Directory.GetFiles(songs_folder, "*.osu_", SearchOption.AllDirectories))
            {
                var info = new FileInfo(file);

                if (info.Name[0]=='_')
                {
                    var dist = info.Name.Substring(1, info.Name.Length - 2);
                    File.Copy(file, Path.Combine(info.DirectoryName, dist),true);
                    File.Delete(file);
                    Console.WriteLine($"{file} -> {dist}");
                }
            }
        }
    }
}
