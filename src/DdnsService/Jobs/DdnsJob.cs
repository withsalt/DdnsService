using DdnsService.Configs;
using DdnsService.Entity;
using DdnsService.Services;
using DdnsService.Utils;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DdnsService.Jobs
{
    [DisallowConcurrentExecution]
    public class DdnsJob : IJob
    {
        private readonly ILogger<DdnsJob> _logger;
        private readonly IMemoryCache _cache;
        private readonly IDomainDdnsService _ddnsService;
        private readonly List<ApisNode> _apis;

        public DdnsJob(ILogger<DdnsJob> logger
            , IMemoryCache cache
            , IOptions<List<ApisNode>> apisOptions
            , IDomainDdnsService ddnsService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _ddnsService = ddnsService ?? throw new ArgumentNullException(nameof(ddnsService));
            _apis = apisOptions.Value ?? throw new ArgumentNullException(nameof(apisOptions));
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                //检查网络
                if (!await NetworkTools.Ping())
                {
                    _logger.LogWarning("The current network is not available. Wait for next run.");
                    return;
                }
                //获取外网IP
                if (_apis == null || !_apis.Any())
                {
                    throw new Exception("获取外网IP接口不存在，请先添加获取IP接口。");
                }
                Stopwatch st = Stopwatch.StartNew();
                (bool state, string data) result = await NetworkTools.TryGetNetworkIp(_apis);
                if (!result.state)
                {
                    _logger.LogWarning($"Get network ip address failed. {result.data}");
                    return;
                }
                st.Stop();
                _logger.LogInformation($"获取外网IP成功，IP地址：{result.data}，耗时：{st.ElapsedMilliseconds}ms");
                //检查变更
                await CheckforIpChange(result.data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred during DDNS operation, {ex.Message}");
            }
        }

        private async Task CheckforIpChange(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
            {
                throw new ArgumentNullException(nameof(ip));
            }
            //与上次获取的值进行对比
            IpHistoryCache history = _cache.Get<IpHistoryCache>(ConfigConstant.LASTIPCACHEKEY);
            if (history == null)
            {
                //标识第一次运行IP更新服务，强制更新域名解析记录
                await _ddnsService.Update(ip);
                //保存到缓存中
                history = new IpHistoryCache()
                {
                    IP = ip,
                    Count = 1,
                    UpdateTime = DateTime.Now,
                };
                _cache.Set(ConfigConstant.LASTIPCACHEKEY, history);
            }
            else
            {
                //对比记录IP是否变更或者半个小时强制更新
                if (history.IP != ip || DateTime.Now - history.UpdateTime >= TimeSpan.FromMinutes(30))
                {
                    _logger.LogInformation($"IP地址检查，当前外网IP地址：{ip}。强制检查记录是否变更...");
                    //记录变更，更新域名解析记录
                    await _ddnsService.Update(ip);
                    //重新保存到缓存中
                    history.IP = ip;
                    history.Count = 1;
                    history.UpdateTime = DateTime.Now;
                    _cache.Set(ConfigConstant.LASTIPCACHEKEY, history);
                }
                else
                {
                    _logger.LogInformation($"IP地址检查，当前外网IP地址：{ip}。与缓存中地址一致，无需更新！");
                    //计数器+1
                    history.Count++;
                    _cache.Set(ConfigConstant.LASTIPCACHEKEY, history);
                }
            }
        }
    }
}
