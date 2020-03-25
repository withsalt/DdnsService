using DdnsService.Config;
using Logger;
using System;
using System.Threading.Tasks;

namespace DdnsService
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Init();

            await Start();
            while (true)
            {
                if (Console.IsOutputRedirected)
                {
                    await Task.Delay(1000);
                }
                else
                {
                    ConsoleKeyInfo key = Console.ReadKey();
                    if (key.Key == ConsoleKey.Escape || key.Key == ConsoleKey.Q)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Are you want to exit ddns service?(y or other key)");
                        key = Console.ReadKey();
                        if (key.Key == ConsoleKey.Y)
                        {
                            Stop();

                            Environment.Exit(0);
                        }
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }
            }
        }

        static async Task Start()
        {
            Log.Info("Ddns service starting.");

            while (true)
            {
                await Task.Delay(10000);

                Log.Info("运行中....");
            }
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
