using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DotNetDllPathPatcher
{
    internal static class Program
    {
        private const int MaxPathLength = 1024;

        private static void OutputUsage()
        {
            Console.WriteLine(
@"-s {ExePath} -d [Nullable]{DllPathName} -o [Nullable]{oldDllPathName} -ig [Nullable]{igore path or file}
Example:
-s c:\temp\1.exe -d dll -ig wwwroot,appsettings.json dll <=> bin\1.dll => dll\1.dll");
        }

        private static int Main(string[] args)
        {
            try
            {
                if (args.Length == 0 
                    || args.Length < 2 
                    || args.Length > 8 
                    || args.Contains(@"-h") 
                    || args.Contains(@"--help"))
                {
                    OutputUsage();
                    return 1;
                }

                string exePathArg = GetArgValue(args, "-s");
                if (string.IsNullOrEmpty(exePathArg))
                {
                    OutputUsage();
                    return 1;
                }
                string exePath = Path.GetFullPath(exePathArg);
                string dllPathName = GetArgValue(args, "-d");
                if (string.IsNullOrEmpty(dllPathName))
                {
                    dllPathName = @"bin";
                }
                string oldDllPathName = GetArgValue(args, "-o");
                if (string.IsNullOrEmpty(oldDllPathName))
                {
                    oldDllPathName = string.Empty;
                }
                List<string> igFiles = new List<string>();
                string igFileStringArg = GetArgValue(args, "-ig");
                if (!string.IsNullOrEmpty(igFileStringArg))
                {
                    igFiles = igFileStringArg.Split(',').ToList();
                }
                if (!File.Exists(exePath))
                {
                    throw new FileNotFoundException($@"{exePath} not found!");
                }
                PatchExe(oldDllPathName, dllPathName, exePath);
                MoveDll(oldDllPathName, dllPathName, exePath, igFiles);
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return 1;
            }
        }

        private static string GetArgValue(string[] args, string param)
        {
            if (args == null || args.Length == 0)
            {
                throw new Exception("Arg error.");
            }
            if (string.IsNullOrEmpty(param))
            {
                throw new Exception($"Can not get args by param({param}).");
            }

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == param && i <= args.Length - 2 && !args[i + 1].StartsWith("-"))
                {
                    return args[i + 1];
                }
            }
            return null;
        }

        private static void MoveDll(string oldDllPathName, string dllPathName, string exePath, List<string> igFiles = null)
        {
            var root = Path.GetDirectoryName(exePath);
            if (root == null)
            {
                throw new InvalidOperationException(@"Wrong exe path");
            }

            if (string.IsNullOrEmpty(oldDllPathName))
            {
                var tempPath = Path.Combine(Path.GetDirectoryName(root) ?? throw new InvalidOperationException(@"Wrong exe path"), @"tempBin");
                Directory.Move(root, tempPath);
                var newPath = Path.Combine(root, dllPathName);

                if (!string.IsNullOrEmpty(dllPathName)
                    && !Directory.Exists(root))
                {
                    Directory.CreateDirectory(root);
                }
                Directory.Move(tempPath, newPath);
                File.Move(Path.Combine(newPath, Path.GetFileName(exePath)), exePath);
            }
            else
            {
                var newPath = Path.Combine(root, dllPathName);
                if (Directory.Exists(newPath))
                {
                    Directory.Delete(newPath, true);
                }
                Directory.Move(Path.Combine(root, oldDllPathName), newPath);
            }
            if (igFiles != null && igFiles.Count > 0)
            {
                foreach (var item in igFiles)
                {
                    string path = Path.Combine(root, dllPathName, item);
                    if (File.Exists(path))
                    {
                        File.Move(path, Path.Combine(root, item));
                    }
                    else if (Directory.Exists(path))
                    {
                        Directory.Move(path, Path.Combine(root, item));
                    }
                }
            }
        }

        private static void PatchExe(string oldDllPathName, string dllPathName, string exePath)
        {
            var exeName = Path.GetFileName(exePath);
            var separator = GetSeparator(exeName);

            if (!string.IsNullOrEmpty(oldDllPathName))
            {
                oldDllPathName += separator;
            }
            Span<byte> oldBytes = Encoding.UTF8.GetBytes($"{oldDllPathName}{ChangeExtensionToDll(exeName)}\0");
            if (oldBytes.Length > MaxPathLength)
            {
                throw new PathTooLongException(@"old dll path is too long");
            }

            if (!string.IsNullOrEmpty(dllPathName))
            {
                dllPathName += separator;
            }
            Span<byte> newBytes = Encoding.UTF8.GetBytes($"{dllPathName}{ChangeExtensionToDll(exeName)}\0");
            if (newBytes.Length > MaxPathLength)
            {
                throw new PathTooLongException(@"new dll path is too long");
            }

            Span<byte> bytes = File.ReadAllBytes(exePath);
            var index = bytes.IndexOf(oldBytes);
            if (index < 0)
            {
                throw new InvalidDataException($@"Could not find old dll path {oldDllPathName}");
            }

            if (index + newBytes.Length > bytes.Length)
            {
                throw new PathTooLongException(@"new dll path is too long");
            }

            using var fs = File.OpenWrite(exePath);
            fs.Write(bytes.Slice(0, index));
            fs.Write(newBytes);
            fs.Write(bytes.Slice(index + newBytes.Length));
        }

        private static bool IsWindowsExe(string str)
        {
            return str.EndsWith(@".exe", StringComparison.OrdinalIgnoreCase);
        }

        private static string ChangeExtensionToDll(string exeName)
        {
            return IsWindowsExe(exeName) ? Path.ChangeExtension(exeName, @".dll") : $@"{exeName}.dll";
        }

        private static string GetSeparator(string exeName)
        {
            return IsWindowsExe(exeName) ? @"\" : @"/";
        }
    }
}