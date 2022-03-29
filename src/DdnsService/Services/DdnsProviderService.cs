using DdnsService.Configs;
using DdnsService.Extensions;
using DdnsService.Models;
using DdnsService.SDKs;
using DdnsService.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DdnsService.Services
{
    public class DdnsProviderService : IDdnsProviderService
    {
        private readonly ILogger<DdnsProviderService> _logger;
        private readonly DdnsConfigNode _ddnsConfigNode;

        public DdnsProviderService(ILogger<DdnsProviderService> logger
            , IOptions<DdnsConfigNode> ddnsConfigNodeOptions)
        {
            _ddnsConfigNode = ddnsConfigNodeOptions.Value ?? throw new ArgumentNullException(nameof(ddnsConfigNodeOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            //获取DDNS服务商配置
            if (_ddnsConfigNode.Providers == null || !_ddnsConfigNode.Providers.Any())
            {
                _logger.ThrowLogError("获取DDNS提供厂商列表失败，请配置DDNS提供厂商（配置文件：DdnsConfig->DdnsProviders）。");
            }
            //检查Id是否重复
            var ids = _ddnsConfigNode.Providers.GroupBy(p => p.Id).Select(x => new
            {
                Id = x.Key,
                Count = x.Count()
            });
            foreach (var item in ids)
            {
                if (item.Count > 1)
                {
                    _logger.ThrowLogError($"DDNS提供厂商列表设置不正确，Id重复（Id：{item.Id}）。");
                }
            }
            //各项检查
            for (int i = 0; i < _ddnsConfigNode.Providers.Count; i++)
            {
                //检查Id不能小于0
                if (_ddnsConfigNode.Providers[i].Id <= 0)
                {
                    _logger.ThrowLogError($"DDNS提供厂商列表中第{i + 1}项Id设置不正确，Id不能为负数。");
                }
                //检查AccessKey和AccessKeySecret
                if (string.IsNullOrWhiteSpace(_ddnsConfigNode.Providers[i].AccessKey)
                    || string.IsNullOrWhiteSpace(_ddnsConfigNode.Providers[i].AccessKeySecret))
                {
                    _logger.ThrowLogError($"DDNS提供厂商列表中Id为{_ddnsConfigNode.Providers[i].Id}配置错误，AccessKey或AccessKeySecret不能为空。");
                }
                //检查Type
                if (!EnumUtil.EnumToList<DdnsProviderType>().Any(p => p.Value == (int)_ddnsConfigNode.Providers[i].Type))
                {
                    _logger.ThrowLogError($"DDNS提供厂商列表中Id为{_ddnsConfigNode.Providers[i].Id}配置错误，Type值错误。");
                }
            }
        }

        public IDdnsService Get(int providerId)
        {
            var providerConfig = _ddnsConfigNode.Providers.Where(p => p.Id == providerId).FirstOrDefault();
            if (providerConfig == null)
            {
                return null;
            }
            switch (providerConfig.Type)
            {
                case DdnsProviderType.Aliyun:
                    {
                        return new AliyunDdns(providerConfig.AccessKey, providerConfig.AccessKeySecret);
                    }
                case DdnsProviderType.QCloud:
                    {
                        return new QCloudDdns(providerConfig.AccessKey, providerConfig.AccessKeySecret);
                    }
                default:
                    throw new Exception($"Unknow provider type: {providerConfig.Type}");
            }
        }
    }
}
