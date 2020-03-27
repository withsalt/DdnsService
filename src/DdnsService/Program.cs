using DdnsSDK;
using DdnsSDK.Interface;
using DdnsSDK.Model;
using DdnsService.ApiService;
using DdnsService.Config;
using DdnsService.Data;
using DdnsService.Model;
using Logger;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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
                        UpdateDomainInfo(ip);
                        isFirst = false;
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

            if (!DomainInfoVal(ConfigManager.Now.DdnsConfig.Domain))
            {
                throw new Exception(Log.Error("错误，DDNS域名格式不正确，正确的域名格式参考：xxx.xxx.com。"));
            }

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
                if (string.IsNullOrEmpty(ConfigManager.Now.DdnsConfig.Domain)
                    || !DomainInfoVal(ConfigManager.Now.DdnsConfig.Domain)
                    || string.IsNullOrEmpty(ConfigManager.Now.DdnsConfig.AccessKeyId)
                    || string.IsNullOrEmpty(ConfigManager.Now.DdnsConfig.AccessKeySecret)
                    || ConfigManager.Now.DdnsConfig.TTL < 60)
                {
                    Log.Info($"DDNS配置不正确，已跳过。");
                }
                else
                {
                    try
                    {
                        bool updateState = UpdateDomainInfo(ip);
                        if (updateState)
                            Log.Info($"域名[{ConfigManager.Now.DdnsConfig.Domain}]DDNS信息已变更，当前IP：{ip}");
                        else
                            Log.Info($"域名[{ConfigManager.Now.DdnsConfig.Domain}]DDNS信息未变更，当前IP：{ip}");
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

        static bool UpdateDomainInfo(string ip)
        {
            try
            {
                IDdnsService ddns = new AliyunDdns(ConfigManager.Now.DdnsConfig.AccessKeyId, ConfigManager.Now.DdnsConfig.AccessKeySecret);
                List<DomainRecord> records = ddns.DescribeSubDomainRecords(ConfigManager.Now.DdnsConfig.Domain);
                DomianInfo configDomainInfo = DomianInfo(ConfigManager.Now.DdnsConfig.Domain);
                DomainRecord domainInfo = null;
                foreach (var item in records)
                {
                    if ($"{item.RR}.{item.DomainName}".ToLower() == ConfigManager.Now.DdnsConfig.Domain.ToLower())
                    {
                        domainInfo = item;
                        break;
                    }
                }
                if (records.Count > 1)
                {
                    ddns.DeleteSubDomainRecords(new DeleteDomainRecordParam()
                    {
                        RR = domainInfo.RR,
                        DomainName = domainInfo.DomainName
                    });
                    domainInfo = null;
                }
                if (domainInfo == null)
                {
                    ddns.AddDomainRecord(new AddDomainRecordParam()
                    {
                        DomainName = configDomainInfo.DomainName,
                        RR = configDomainInfo.RR,
                        Type = DdnsSDK.Model.Enum.DomainRecordType.A,
                        Value = ip,
                        TTL = ConfigManager.Now.DdnsConfig.TTL
                    });
                }
                else
                {
                    if (domainInfo.RR == configDomainInfo.RR
                        && domainInfo.TTL == ConfigManager.Now.DdnsConfig.TTL
                        && domainInfo.Value == ip)
                    {
                        return false;
                    }
                    ddns.UpdateDomainRecord(new UpdateDomainRecordParam()
                    {
                        RecordId = domainInfo.RecordId,
                        RR = configDomainInfo.RR,
                        Type = DdnsSDK.Model.Enum.DomainRecordType.A,
                        Value = ip,
                        TTL = ConfigManager.Now.DdnsConfig.TTL
                    });
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        static DomianInfo DomianInfo(string domain)
        {
            DomianInfo info = new DomianInfo();
            string[] olds = domain.Split('.');
            if (olds.Length < 3)
            {
                throw new Exception("域名格式不正确，正确的域名格式参考：xxx.xxx.com");
            }
            info.RR = domain.Substring(0, domain.IndexOf('.'));
            info.DomainName = domain.Substring(domain.IndexOf('.') + 1);
            return info;
        }

        static bool DomainInfoVal(string domain)
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

        static async Task Delay()
        {
            await Task.Delay(ConfigManager.Now.AppSettings.IntervalTime * 1000, token.Token);
        }

        #endregion
    }
}
