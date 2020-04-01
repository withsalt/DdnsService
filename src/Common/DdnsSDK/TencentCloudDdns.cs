using DdnsSDK.Interface;
using DdnsSDK.Model;
using DdnsSDK.Model.Enum;
using System;
using System.Collections.Generic;
using System.Text;
using TencentCloudDnsSDK;
using TencentCloudDnsSDK.Model.Request;
using TencentCloudDnsSDK.Model.Response;

namespace DdnsSDK
{
    public class TencentCloudDdns : IDdnsService
    {
        private readonly CnsSdk client = null;

        public TencentCloudDdns(string acessKey, string accessSecret) : base(acessKey, accessSecret)
        {
            client = new CnsSdk(AccessKey, AccessSecret);
        }

        /// <summary>
        /// 添加解析
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public override DomainRecordActionResult AddDomainRecord(AddDomainRecordParam param)
        {
            DomainListResult domainList = client.DomainList(new DomainListRequestParam()).GetAwaiter().GetResult();
            if (domainList == null || domainList.Code != "0")
            {
                throw new Exception($"Add domain records info failed. can not get now domain info. {domainList.Message}");
            }
            bool haveDomain = false;
            foreach (var item in domainList.Data.Domains)
            {
                if (string.Equals(item.name, param.DomainName, StringComparison.OrdinalIgnoreCase))
                {
                    haveDomain = true;
                    break;
                }
            }
            if (!haveDomain)
            {
                var createResult = client.DomainCreate(new DomainCreateRequestParam()
                {
                    domain = param.DomainName
                }).GetAwaiter().GetResult();
                if (createResult.Code != "0")
                {
                    throw new Exception($"Add domain records info failed. can not add now domain. {domainList.Message}");
                }
            }
            var recordCreateResult = client.RecordCreate(new RecordCreateRequestParam()
            {
                domain = param.DomainName,
                subDomain = param.RR,
                recordType = RecordTypeMapper(param.Type),
                value = param.Value,
                ttl = param.TTL
            }).GetAwaiter().GetResult();
            if (recordCreateResult.Code == "0")
            {
                return new DomainRecordActionResult()
                {

                };
            }
            else
            {
                throw new Exception($"Add domain records info failed. {recordCreateResult.Message}");
            }
        }

        private TencentCloudDnsSDK.Enum.RecordType RecordTypeMapper(DomainRecordType domainRecordType)
        {
            switch (domainRecordType)
            {
                case DomainRecordType.A:
                    return TencentCloudDnsSDK.Enum.RecordType.A;
                case DomainRecordType.AAAA:
                    return TencentCloudDnsSDK.Enum.RecordType.AAAA;
                case DomainRecordType.CNAME:
                    return TencentCloudDnsSDK.Enum.RecordType.CNAME;
                case DomainRecordType.MX:
                    return TencentCloudDnsSDK.Enum.RecordType.MX;
                case DomainRecordType.TXT:
                    return TencentCloudDnsSDK.Enum.RecordType.TXT;
                case DomainRecordType.NS:
                    return TencentCloudDnsSDK.Enum.RecordType.NS;
                case DomainRecordType.SRV:
                    return TencentCloudDnsSDK.Enum.RecordType.SRV;
                default:
                    throw new Exception("Unsupport tencent cloud RecordType.");
            }
        }

        /// <summary>
        /// 根据recordId获取解析的详细信息（可选实现）
        /// </summary>
        /// <param name="recordId"></param>
        /// <returns></returns>
        public override DomainRecordInfo DescribeDomainRecordInfo(string recordId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 删除域名解析
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public override DomainRecordActionResult DeleteDomainRecord(DeleteDomainRecordParam param)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 根据主解析记录删除相关的解析记录（可选实现）
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public override DomainRecordActionResult DeleteSubDomainRecords(DeleteDomainRecordParam param)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取域名全部解析记录
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        public override List<DomainRecord> DescribeSubDomainRecords(string domain)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 更新解析记录
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public override DomainRecordActionResult UpdateDomainRecord(UpdateDomainRecordParam param)
        {
            throw new NotImplementedException();
        }
    }
}
