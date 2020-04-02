using DdnsSDK;
using DdnsSDK.Interface;
using DdnsSDK.Model;
using DdnsService.Config;
using DdnsService.Enum;
using DdnsService.Model;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DdnsService.ApiService
{
    class DomainDdnsService
    {
        public void UpdateDomainInfo(string ip)
        {
            List<DdnsServiceProvider> providers = ConfigManager.Now.DdnsConfig.DdnsServiceProviders;
            if (providers == null || providers.Count == 0)
            {
                throw new Exception("配置DDNS时发生错误，未发现DDNS服务提供配置。");
            }

            foreach (var item in ConfigManager.Now.DdnsConfig.Domains)
            {
                if (!DomainInfoVal(item))
                {
                    Log.Warn($"DDNS域名格式不正确，正确的域名格式参考：xxx.xxx.com，已跳过该域名[{item.Domain}]DNNS。");
                    continue;
                }
                if (!DomainTTLVal(item))
                {
                    Log.Warn($"DDNS域名TTL不正确，TTL不能小于60秒，已跳过该域名[{item.Domain}]DNNS。");
                    continue;
                }
                DdnsServiceProvider provider = providers.Where(p => p.Id == item.Provider).SingleOrDefault();
                if (provider == null)
                {
                    Log.Warn($"未找到该域名[{item.Domain}]的DDNS服务提供者，跳过该域名DNNS。");
                    continue;
                }
                bool state = false;
                switch (provider.Type)
                {
                    case DdnsServiceProviderType.Aliyun:
                        state = UpdateDdnsInfo(new AliyunDdns(provider.AccessKey, provider.AccessKeySecret), ip, item);
                        if (state)
                            Log.Info($"域名[{item.Domain}]DDNS信息已变更，当前IP：{ip}");
                        else
                            Log.Info($"域名[{item.Domain}]DDNS信息未变更，当前IP：{ip}");
                        break;
                    case DdnsServiceProviderType.TencentCloud:
                        state = UpdateDdnsInfo(new TencentCloudDdns(provider.AccessKey, provider.AccessKeySecret), ip, item);
                        if (state)
                            Log.Info($"域名[{item.Domain}]DDNS信息已变更，当前IP：{ip}");
                        else
                            Log.Info($"域名[{item.Domain}]DDNS信息未变更，当前IP：{ip}");
                        break;
                    default:
                        Log.Warn($"未知的DDNS服务提供者（{provider.Type}），跳过该域名[{item.Domain}]DNNS。");
                        break;
                }
            }
        }

        private bool UpdateDdnsInfo(IDdnsService ddns, string ip, DomainsItem domain)
        {

            if (ddns == null)
            {
                throw new Exception("Ddns service provider is null.");
            }
            List<DomainRecord> records = ddns.DescribeSubDomainRecords(domain.Domain);
            DomianInfo configDomainInfo = DomianInfo(domain.Domain);
            DomainRecord domainInfo = null;
            foreach (var item in records)
            {
                if ($"{item.RR}.{item.DomainName}".ToLower() == domain.Domain.ToLower())
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
                    TTL = domain.TTL
                });
            }
            else
            {
                if (domainInfo.RR == configDomainInfo.RR
                    && domainInfo.TTL == domain.TTL
                    && domainInfo.Value == ip)
                {
                    return false;
                }
                ddns.UpdateDomainRecord(new UpdateDomainRecordParam()
                {
                    DomainName = configDomainInfo.DomainName,
                    RecordId = domainInfo.RecordId,
                    RR = configDomainInfo.RR,
                    Type = DdnsSDK.Model.Enum.DomainRecordType.A,
                    Value = ip,
                    TTL = domain.TTL
                });
            }
            return true;
        }

        private DomianInfo DomianInfo(string domain)
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

        private bool DomainInfoVal(DomainsItem domain)
        {
            if (domain == null || string.IsNullOrEmpty(domain.Domain))
            {
                return false;
            }
            string[] dos = domain.Domain.Split('.');
            if (dos.Length < 3)
            {
                return false;
            }
            return true;
        }

        private bool DomainTTLVal(DomainsItem domain)
        {
            if (domain == null)
            {
                return false;
            }
            if (domain.TTL < 60)
            {
                return false;
            }
            return true;
        }
    }
}
