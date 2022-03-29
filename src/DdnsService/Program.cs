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
                    //移除已经注册的其他日志处理程序
                    logging.ClearProviders();
                    //设置最小的日志级别
                    logging.SetMinimumLevel(LogLevel.Trace);  
                })
                .UseNLog()
                .ConfigureServices((hostContext, services) =>
                {
                    //强类型配置
                    services.ConfigureSettings(hostContext);
                    //缓存
                    services.AddMemoryCache();
                    //数据库
                    services.AddSqlite();
                    //DDNS服务
                    services.AddDdns();
                    //定时任务
                    services.AddQuartz();
                    //配置并启动服务
                    services.AddHostedService<ConfigureService>();
                });
    }
}
