using DdnsService.ApiService;
using DdnsService.Config;
using DdnsService.Data;
using DdnsService.Model;
using Logger;
using System;
using System.Threading.Tasks;

namespace DdnsService
{
    class Program
    {
        static IpDataManager dataManager = new IpDataManager();

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
                try
                {
                    LocalIPInfo ipHelper = new LocalIPInfo();
                    string ip = await ipHelper.GetLocalIp();

                    Log.Info($"当前网络环境外网IP：{ip}");
                    LocalIpHistory lastIp = await dataManager.GetLastIpInfo();
                    if (lastIp == null || lastIp.IP != ip)
                        DoDifferentAction(ip, lastIp == null ? "0.0.0.0" : lastIp.IP);
                    else
                        Log.Info($"当前网络环境外网IP[{ip}]未发生变化。");

                    if (await dataManager.SaveIpInfo(ip, lastIp == null ? "0.0.0.0" : lastIp.IP) != null)
                        Log.Info($"当前IP[{ip}]已记录。");

                    await Task.Delay(10000);
                }
                catch (Exception ex)
                {
                    Log.Warn(ex.Message, ex);
                }
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

            if (!CustumDbInit.Initialize())
            {
                throw new Exception(Log.Error("错误，无法初始化数据库。"));
            }
            return true;
        }

        static void DoDifferentAction(string ip, string lastIp)
        {
            Log.Info($"检测到外网IP已变更，当前IP：{ip}，历史IP：{lastIp}");
            if (ConfigManager.Now.AppSettings.IsEnableEmailNotice)
            {
                if (string.IsNullOrEmpty(ConfigManager.Now.AppSettings.EmailApiConfig.Host)
                    || string.IsNullOrEmpty(ConfigManager.Now.AppSettings.EmailApiConfig.Account)
                    || string.IsNullOrEmpty(ConfigManager.Now.AppSettings.EmailApiConfig.Passwd))
                {
                    Log.Info($"邮件配置不正确，已跳过。");
                }
                else
                {
                    EmailNotice email = new EmailNotice();
                    (bool, string) result = email.Send(ip, lastIp);
                    if (result.Item1)
                    {
                        Log.Info($"IP变更邮件提醒已发送，接收邮箱：{ConfigManager.Now.AppSettings.EmailApiConfig.ReceiveUser}。");
                    }
                    else
                    {
                        Log.Info($"IP变更邮件提醒发送失败，错误：{result.Item2}");
                    }
                }
            }

            if (ConfigManager.Now.AppSettings.IsEnableMessageNotice)
            {
                if (string.IsNullOrEmpty(ConfigManager.Now.AppSettings.MessageApiConfig.AppKey)
                    || string.IsNullOrEmpty(ConfigManager.Now.AppSettings.MessageApiConfig.MessageTemplate)
                    || string.IsNullOrEmpty(ConfigManager.Now.AppSettings.MessageApiConfig.URL)
                    || string.IsNullOrEmpty(ConfigManager.Now.AppSettings.MessageApiConfig.Mobile))
                {
                    Log.Info($"短信配置不正确，已跳过。");
                }
                else
                {
                    MessageNotice message = new MessageNotice();
                    (bool, string) result = message.Send(ip, lastIp);
                    if (result.Item1)
                    {
                        Log.Info($"IP变更短信提醒已发送，接收电话：{ConfigManager.Now.AppSettings.MessageApiConfig.Mobile}。");
                    }
                    else
                    {
                        Log.Info($"IP变更短信提醒发送失败，错误：{result.Item2}");
                    }
                }
            }

            if (ConfigManager.Now.DdnsConfig.IsEnableDdns)
            {
                if (string.IsNullOrEmpty(ConfigManager.Now.DdnsConfig.Domain)
                    || !DomainVal(ConfigManager.Now.DdnsConfig.Domain)
                    || string.IsNullOrEmpty(ConfigManager.Now.DdnsConfig.AccessKeyId)
                    || string.IsNullOrEmpty(ConfigManager.Now.DdnsConfig.AccessKeySecret)
                    || ConfigManager.Now.DdnsConfig.TTL < 60)
                {
                    Log.Info($"DDNS配置不正确，已跳过。");
                }
                else
                {

                }
            }
            else
            {
                Log.Info($"DDNS已关闭，跳过域名DDNS。");
            }
        }

        static bool DomainVal(string domain)
        {
            if (string.IsNullOrEmpty(domain))
            {
                return false;
            }
            string[] dos = domain.Split('.');
            if (dos.Length < 3)
            {
                return false;
            }
            return true;
        }
    }
}
