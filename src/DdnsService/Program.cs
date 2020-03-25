using DdnsService.Config;
using Logger;
using System;

namespace DdnsService
{
    class Program
    {
        static void Main(string[] args)
        {
            Init();

            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey();
                if(key.Key == ConsoleKey.Escape || key.Key == ConsoleKey.Q)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Are you want to exit ddns service?(y or other key)");
                    key = Console.ReadKey();
                    if(key.Key == ConsoleKey.Y)
                    {
                        Stop();

                        Environment.Exit(0);
                    }
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
        }

        static void Start()
        {
            Log.Info("Ddns service starting.");
        }

        static void Stop()
        {
            Log.Info("Ddns service stopping.");
        }

        static bool Init()
        {
            if (!ConfigManager.LoadConfig())
            {
                throw new Exception(Log.Error("错误，无法加载配置文件。"));
            }
            return true;
        }
    }
}
