using Aliyun.Acs.Alidns.Model.V20150109;
using Aliyun.Acs.Core;
using Aliyun.Acs.Core.Auth;
using Aliyun.Acs.Core.Exceptions;
using Aliyun.Acs.Core.Profile;
using DdnsService.SDKs.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DdnsService.SDKs
{
    public class AliyunDdns : IDdnsService
    {
        readonly DefaultAcsClient _client = null;

        public AliyunDdns(string acessKey, string accessSecret) : base(acessKey, accessSecret)
        {
            AlibabaCloudCredentialsProvider provider = new AccessKeyCredentialProvider(AccessKey, AccessSecret);
            _client = new DefaultAcsClient(DefaultProfile.GetProfile(), provider);
            _client.SetConnectTimeoutInMilliSeconds(60000);
        }

        /// <summary>
        /// 获取子域名的所有解析记录
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        public override Task<List<DomainRecord>> DescribeSubDomainRecords(string RR, string domain)
        {
            long totalCount;
            long pageNumber = 1;
            try
            {
                List<DomainRecord> records = new List<DomainRecord>();
                do
                {
                    var response = _client.GetAcsResponse(new DescribeSubDomainRecordsRequest()
                    {
                        SubDomain = $"{RR}.{domain}",
                        PageNumber = pageNumber,
                        PageSize = 10,
                    });
                    if (response == null || response.DomainRecords == null)
                    {
                        throw new Exception("Describe subDomain records result is null.");
                    }
                    totalCount = response.TotalCount ?? 0;
                    pageNumber++;
                    foreach (var item in response.DomainRecords)
                    {
                        records.Add(new DomainRecord()
                        {
                            Domain = item.DomainName,
                            RecordId = item.RecordId,
                            RR = item.RR,
                            Type = item.Type,
                            Value = item._Value,
                            Line = item.Line,
                            TTL = item.TTL.HasValue ? (uint)item.TTL : 0,
                            Status = item.Status.Equals("ENABLE", StringComparison.OrdinalIgnoreCase) ? DomainRecordStatus.Enabled : DomainRecordStatus.Disabled,
                            Locked = item.Locked.HasValue ? item.Locked.Value : false,
                            Weight = item.Weight.HasValue ? item.Weight.Value : 0,
                        });
                    }
                } while (totalCount != 0 && records.Count != (int)totalCount);
                return Task.FromResult(records);
            }
            catch (ServerException e)
            {
                throw new Exception($"Aliyun server error. {e.Message}");
            }
            catch (ClientException e)
            {
                if (e.ErrorCode.Equals("InvalidDomainName.NoExist", StringComparison.OrdinalIgnoreCase))
                {
                    return Task.FromResult(new List<DomainRecord>());
                }
                else
                {
                    throw new Exception($"Reuqest client error. errcode is {e.ErrorCode}, {e.Message}");
                }
            }
        }

        /// <summary>
        /// 根据recordId获取解析记录的详细信息。
        /// </summary>
        /// <param name="recordId"></param>
        /// <returns></returns>
        public override Task<DomainRecord> DescribeDomainRecordInfo(string recordId, string domain = null)
        {
            var request = new DescribeDomainRecordInfoRequest()
            {
                RecordId = recordId
            };
            var response = _client.GetAcsResponse(request);
            if (response == null)
            {
                throw new Exception("Describe subDomain records info is null.");
            }
            var result = new DomainRecord()
            {
                RR = response.RR,
                Status = response.Status.Equals("ENABLE", StringComparison.OrdinalIgnoreCase) ? DomainRecordStatus.Enabled : DomainRecordStatus.Disabled,
                RequestId = response.RequestId,
                Domain = response.DomainName,
                TTL = response.TTL.HasValue ? (uint)response.TTL.Value : 0,
                Line = response.Line,
                Locked = response.Locked.HasValue ? response.Locked.Value : false,
                Type = response.Type,
                Value = response._Value,
                RecordId = response.RecordId,
            };
            return Task.FromResult(result);
        }

        /// <summary>
        /// 添加解析记录，默认TTL为600
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public override Task<DomainRecordActionResult> AddDomainRecord(AddDomainRecordParam param)
        {
            var request = new AddDomainRecordRequest()
            {
                DomainName = param.Domain,
                RR = param.RR,
                Type = param.Type.ToString(),
                TTL = param.TTL,
                _Value = param.Value
            };
            var response = _client.GetAcsResponse(request);
            if (response == null)
            {
                throw new Exception("Add subdomain records info failed.");
            }
            var result = new DomainRecordActionResult()
            {
                Status = true,
                RequestId = response.RequestId,
                RecordId = response.RecordId,
                RR = param.RR,
                TotalCount = 1,
            };
            return Task.FromResult(result);
        }

        public override Task<DomainRecordActionResult> DeleteDomainRecord(DeleteDomainRecordParam param)
        {
            var request = new DeleteDomainRecordRequest()
            {
                RecordId = param.RecordId,
            };
            var response = _client.GetAcsResponse(request);
            if (response == null)
            {
                throw new Exception($"Delete subdomain record info failed. record id is {param.RecordId}");
            }
            var result = new DomainRecordActionResult()
            {
                Status = true,
                RecordId = response.RecordId,
                RequestId = response.RequestId,
                RR = param.RR,
                TotalCount = 1,
            };
            return Task.FromResult(result);
        }

        public override Task<DomainRecordActionResult> UpdateDomainRecord(UpdateDomainRecordParam param)
        {
            var request = new UpdateDomainRecordRequest()
            {
                RecordId = param.RecordId,
                RR = param.RR,
                Type = param.Type.ToString(),
                _Value = param.Value,
                TTL = param.TTL
            };
            var response = _client.GetAcsResponse(request);
            if (response == null)
            {
                throw new Exception($"Update subdomain record info failed. record id is {param.RecordId}, new value is {param.Value}");
            }
            var result = new DomainRecordActionResult()
            {
                Status = true,
                RequestId = response.RequestId,
                RecordId = response.RecordId,
                RR = param.RR,
                TotalCount = 1,
            };
            return Task.FromResult(result);
        }

        public override Task<bool> CreateDomain(string domain)
        {
            var response = _client.GetAcsResponse(new AddDomainRequest()
            {
                DomainName = domain,
            });
            return Task.FromResult(response != null && !string.IsNullOrWhiteSpace(response.RequestId));
        }

        public override Task<DomainItem> DescribeDomainInfo(string domain)
        {
            try
            {
                DescribeDomainInfoResponse response = _client.GetAcsResponse(new DescribeDomainInfoRequest()
                {
                    DomainName = domain,
                });
                if (response == null)
                {
                    throw new Exception("Describe domain info is null.");
                }
                return Task.FromResult(new DomainItem()
                {
                    DomainId = response.DomainId,
                    Domain = response.DomainName,
                    Status = DomainStatus.Enabled,
                    Remark = response.Remark,
                    RecordCount = (uint)(response.RecordLines.Count),
                    CreatedOn = DateTime.TryParse(response.CreateTime, out DateTime createTime) ? createTime : DateTime.MinValue,
                    UpdatedOn = DateTime.MinValue
                });
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("InvalidDomainName.NoExist", StringComparison.OrdinalIgnoreCase))
                {
                    //域名不存在
                    throw new DomainNotExistsException($"domain '{domain}' does not exist", ex);
                }
                throw;
            }
        }

        public override string ToString()
        {
            return "阿里云DDNS";
        }
    }
}
