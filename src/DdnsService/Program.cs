using DdnsService.Configs;
using DdnsService.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using System.Collections.Generic;

namespace DdnsService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    //�Ƴ��Ѿ�ע���������־�������
                    logging.ClearProviders();
                    //������С����־����
                    logging.SetMinimumLevel(LogLevel.Trace);  
                })
                .UseNLog()
                .ConfigureServices((hostContext, services) =>
                {
                    //ǿ��������
                    services.ConfigureSettings(hostContext);
                    //����
                    services.AddMemoryCache();
                    //���ݿ�
                    services.AddSqlite();
                    //DDNS����
                    services.AddDdns();
                    //��ʱ����
                    services.AddQuartz();
                    //���ò���������
                    services.AddHostedService<ConfigureService>();
                });
    }
}
