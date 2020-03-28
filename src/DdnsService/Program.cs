using System;
using System.Threading;
using System.Threading.Tasks;

using DdnsService.ApiService;
using DdnsService.Config;
using DdnsService.Data;
using DdnsService.Model;
using Logger;

namespace DdnsService
{
    class Program
    {
        readonly static IpDataManager dataManager = new IpDataManager();

        readonly static CancellationTokenSource token = new CancellationTokenSource();

        static Task task;

        static void Main(string[] args)
        {
            try
            {
                Init();
                task = Task.Run(() => Start());

                if (!Console.IsOutputRedirected)
                {
                    Console.CancelKeyPress += (sender, e) =>
                    {
                        Stop();
                    };
                }
                while (true)
                {
                    if (Console.IsOutputRedirected)
                    {
                        Thread.Sleep(1000);
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
                            }
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        static async void Start()
        {
            Log.Info("Ddns service starting.");
            bool isFirst = ConfigManager.Now.DdnsConfig.IsEnableDdns;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    LocalIPInfo ipHelper = new LocalIPInfo();
                    string ip = await ipHelper.GetLocalIp();
                    Log.Info($"当前网络环境外网IP：{ip}");
                    if (isFirst)
                    {
                        isFirst = false;
                        new DomainDdnsService().UpdateDomainInfo(ip);
                    }
                    LocalIpHistory lastIp = await dataManager.GetLastIpInfo();
                    if (lastIp == null || lastIp.IP != ip)
                        await DoDifferentAction(ip, lastIp == null ? "0.0.0.0" : lastIp.IP);
                    else
                        Log.Info($"当前网络环境外网IP[{ip}]未发生变化。");

                    if (await dataManager.SaveIpInfo(ip, lastIp == null ? "0.0.0.0" : lastIp.IP) != null)
                        Log.Info($"当前IP[{ip}]已记录。");

                    await Delay();
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Log.Warn(ex.Message, ex);
                    await Delay();
                }
            }
        }

        static void Stop()
        {
            Log.Info("Ddns service stopping.");
            token.Cancel();
            while (task != null && task.Status != TaskStatus.RanToCompletion)
            {
                Thread.Sleep(10);
            }
            Log.Info("Ddns service has stopped.");
            Environment.Exit(0);
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

            //if (!DomainInfoVal(ConfigManager.Now.DdnsConfig.Domain))
            //{
            //    throw new Exception(Log.Error("错误，DDNS域名格式不正确，正确的域名格式参考：xxx.xxx.com。"));
            //}

            if (!ConfigManager.Now.AppSettings.IsDebug && ConfigManager.Now.AppSettings.IntervalTime < 30)
            {
                throw new Exception(Log.Error("错误，检测间隔不能小于30秒。"));
            }
            return true;
        }

        #region Helper

        static async Task DoDifferentAction(string ip, string lastIp)
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
                    try
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
                    catch (Exception ex)
                    {
                        Log.Error(ex.Message, ex);
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
                    try
                    {
                        MessageNotice message = new MessageNotice();
                        (bool, string) result = await message.Send(ip, lastIp);
                        if (result.Item1)
                        {
                            Log.Info($"IP变更短信提醒已发送，接收电话：{ConfigManager.Now.AppSettings.MessageApiConfig.Mobile}。");
                        }
                        else
                        {
                            Log.Info($"IP变更短信提醒发送失败，错误：{result.Item2}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.Message, ex);
                    }
                }
            }

            if (ConfigManager.Now.DdnsConfig.IsEnableDdns)
            {
                if (ConfigManager.Now.DdnsConfig.Domains == null
                    || ConfigManager.Now.DdnsConfig.Domains.Count == 0)
                {
                    Log.Info($"未发现要设置DDNS的域名配置，DDNS已跳过。");
                }
                if (ConfigManager.Now.DdnsConfig.DdnsServiceProviders == null
                    || ConfigManager.Now.DdnsConfig.DdnsServiceProviders.Count == 0)
                {
                    Log.Info($"未发现DDNS服务提供配置，DDNS已跳过。");
                }
                else
                {
                    try
                    {
                        new DomainDdnsService().UpdateDomainInfo(ip);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.Message, ex);
                    }
                }
            }
            else
            {
                Log.Info($"DDNS已关闭，跳过域名DDNS。");
            }
        }

        static async Task Delay()
        {
            await Task.Delay(ConfigManager.Now.AppSettings.IntervalTime * 1000, token.Token);
        }

        #endregion
    }
}
