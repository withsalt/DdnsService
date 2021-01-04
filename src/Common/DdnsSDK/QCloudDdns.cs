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
    public class QCloudDdns : IDdnsService
    {
        private readonly CnsSdk client = null;

        public QCloudDdns(string acessKey, string accessSecret) : base(acessKey, accessSecret)
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
                    Status = true,
                    RequestId = null,
                    RecordId = recordCreateResult.Data.Record.Id.ToString(),
                    RR = param.RR,
                    TotalCount = 1,
                };
            }
            else
            {
                throw new Exception($"Add domain records info failed. {recordCreateResult.Message}");
            }
        }

        /// <summary>
        /// 根据recordId获取解析的详细信息（可选实现）
        /// </summary>
        /// <param name="recordId"></param>
        /// <returns></returns>
        public override DomainRecordInfo DescribeDomainRecordInfo(string recordId, string domain)
        {
            if (string.IsNullOrEmpty(recordId) && string.IsNullOrEmpty(domain))
            {
                throw new Exception("Recordid or domain can not null.");
            }
            TencentCloudDomianInfo info = GetDomianInfo(domain);
            if (info == null)
            {
                throw new Exception("Get tencent domain info from domain string failed.");
            }
            RecordListResult result = client.RecordList(new RecordListRequestParam()
            {
                domain = domain
            }).GetAwaiter().GetResult();
            if (result.Code != "0")
            {
                throw new Exception($"Get domain record info from tencent cloud failed. error id is {result.Code}, {result.Message}");
            }
            foreach (var item in result.Data.Records)
            {
                if (item.id.ToString() == recordId)
                {
                    return new DomainRecordInfo()
                    {
                        RR = item.name,
                        Status = item.status,
                        RecordId = recordId,
                        DomainName = result.Data.Domain.name,
                        TTL = item.ttl,
                        Line = item.line,
                        Type = item.type,
                        Value = item.value,
                    };
                }
            }
            throw new Exception($"Get domain record info from tencent cloud failed. can not find record from this recordid({recordId})");
        }

        /// <summary>
        /// 删除域名解析
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public override DomainRecordActionResult DeleteDomainRecord(DeleteDomainRecordParam param)
        {
            if (!long.TryParse(param.RecordId, out long tcRecordId))
            {
                throw new Exception("RecordId is not tencent cloud recordid.");
            }
            var result = client.RecordDelete(new RecordDeleteRequestParam()
            {
                recordId = long.Parse(param.RecordId),
                domain = param.DomainName
            }).GetAwaiter().GetResult();
            if (result.Code == "0")
            {
                return new DomainRecordActionResult()
                {
                    RecordId = param.RecordId,
                    RR = "",
                    TotalCount = 1,
                };
            }
            else
            {
                throw new Exception($"Delete domain record info from tencent cloud failed. error id is {result.Code}, {result.Message}");
            }
        }

        /// <summary>
        /// 根据主解析记录删除相关的解析记录（可选实现）
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public override DomainRecordActionResult DeleteSubDomainRecords(DeleteDomainRecordParam param)
        {
            RecordListResult result = client.RecordList(new RecordListRequestParam()
            {
                domain = param.DomainName
            }).GetAwaiter().GetResult();
            if (result.Code != "0")
            {
                throw new Exception($"Get domain record info from tencent cloud failed. error id is {result.Code}, {result.Message}");
            }
            StringBuilder sb = new StringBuilder();
            bool isDeleteFailed = false;
            foreach (var item in result.Data.Records)
            {
                try
                {
                    if (item.name != param.RR)
                    {
                        continue;
                    }
                    var recordDeleteResult = client.RecordDelete(new RecordDeleteRequestParam()
                    {
                        domain = param.DomainName,
                        recordId = item.id,
                    }).GetAwaiter().GetResult();
                    if (recordDeleteResult.Code != "0")
                    {
                        throw new Exception($"Delete record info from tencent cloud failed({recordDeleteResult.Code}). {recordDeleteResult.Message}");
                    }
                }
                catch (Exception ex)
                {
                    sb.AppendLine(ex.Message);
                    isDeleteFailed = true;
                }
            }
            if (isDeleteFailed)
            {
                throw new Exception(sb.ToString());
            }
            return new DomainRecordActionResult()
            {
                Status = true,
                RR = param.RR,
            };
        }

        /// <summary>
        /// 获取域名全部解析记录
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        public override List<DomainRecord> DescribeSubDomainRecords(string domain)
        {
            if (string.IsNullOrEmpty(domain))
            {
                throw new Exception("domain can not null.");
            }
            TencentCloudDomianInfo info = GetDomianInfo(domain);
            if (info == null)
            {
                throw new Exception("Get tencent domain info from domain string failed.");
            }
            RecordListResult result = client.RecordList(new RecordListRequestParam()
            {
                domain = info.DomainName,
            }).GetAwaiter().GetResult();
            if (result.Code != "0")
            {
                throw new Exception($"Get domain record info from tencent cloud failed. error id is {result.Code}, {result.Message}");
            }
            List<DomainRecord> infos = new List<DomainRecord>();
            foreach (var item in result.Data.Records)
            {
                if (item.name == info.RR)
                {
                    infos.Add(new DomainRecord()
                    {
                        RR = item.name,
                        Status = item.status,
                        RecordId = item.id.ToString(),
                        DomainName = result.Data.Domain.name,
                        TTL = item.ttl,
                        Line = item.line,
                        Type = item.type,
                        Value = item.value,
                    });
                }
            }
            return infos;
        }

        /// <summary>
        /// 更新解析记录
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public override DomainRecordActionResult UpdateDomainRecord(UpdateDomainRecordParam param)
        {
            var result = client.RecordModify(new RecordModifyRequestParam()
            {
                domain = param.DomainName,
                recordId = long.Parse(param.RecordId),
                subDomain = param.RR,
                recordType = RecordTypeMapper(param.Type),
                value = param.Value,
                ttl = param.TTL
            }).GetAwaiter().GetResult();
            if (result.Code == "0")
            {
                return new DomainRecordActionResult()
                {
                    RecordId = param.RecordId,
                    RR = "",
                    TotalCount = 1,
                };
            }
            else
            {
                throw new Exception($"Delete domain record info from tencent cloud failed. error id is {result.Code}, {result.Message}");
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

        private TencentCloudDomianInfo GetDomianInfo(string domain)
        {
            TencentCloudDomianInfo info = new TencentCloudDomianInfo();
            string[] olds = domain.Split('.');
            if (olds.Length < 3)
            {
                throw new Exception("域名格式不正确，正确的域名格式参考：xxx.xxx.com");
            }
            info.RR = domain.Substring(0, domain.IndexOf('.'));
            info.DomainName = domain.Substring(domain.IndexOf('.') + 1);
            return info;
        }
    }
}
