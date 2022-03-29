using DdnsService.SDKs.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TencentCloud.Common;
using TencentCloud.Dnspod.V20210323;
using TencentCloud.Dnspod.V20210323.Models;

namespace DdnsService.SDKs
{
    public class QCloudDdns : IDdnsService
    {
        private readonly DnspodClient _client = null;

        public QCloudDdns(string acessKey, string accessSecret) : base(acessKey, accessSecret)
        {
            Credential cred = new Credential
            {
                SecretId = AccessKey,
                SecretKey = AccessSecret
            };
            _client = new DnspodClient(cred, "");
        }

        /// <summary>
        /// 添加解析
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public override async Task<DomainRecordActionResult> AddDomainRecord(AddDomainRecordParam param)
        {
            var response = await _client.CreateRecord(new CreateRecordRequest()
            {
                Domain = param.Domain,
                SubDomain = param.RR,
                RecordType = param.Type.ToString(),
                Value = param.Value,
                TTL = param.TTL,
                RecordLine = "默认",
            });
            if (response != null)
            {
                return new DomainRecordActionResult()
                {
                    Status = true,
                    RequestId = response.RequestId,
                    RecordId = response.RecordId.ToString(),
                    RR = param.RR,
                    TotalCount = 1,
                };
            }
            else
            {
                throw new Exception($"Add domain records info failed. result is null.");
            }
        }

        /// <summary>
        /// 根据recordId获取解析的详细信息（可选实现）
        /// </summary>
        /// <param name="recordId"></param>
        /// <returns></returns>
        public override async Task<DomainRecord> DescribeDomainRecordInfo(string recordId, string domain)
        {
            if (string.IsNullOrEmpty(recordId) && string.IsNullOrEmpty(domain))
            {
                throw new Exception("Recordid and domain can not null.");
            }
            var response = await _client.DescribeRecord(new DescribeRecordRequest()
            {
                Domain = domain,
                RecordId = ulong.Parse(recordId),
            });
            if (string.IsNullOrEmpty(response.RequestId) || response.RecordInfo == null)
            {
                throw new Exception("Get tencent domain info from recordId failed.");
            }
            return new DomainRecord()
            {
                RR = response.RecordInfo.SubDomain,
                Status = response.RecordInfo.Enabled == 1 ? DomainRecordStatus.Enabled : DomainRecordStatus.Disabled,
                RecordId = recordId,
                Domain = domain,
                TTL = (uint)response.RecordInfo.TTL,
                Line = response.RecordInfo.RecordLine,
                Type = response.RecordInfo.RecordType,
                Value = response.RecordInfo.Value,
                RequestId = response.RequestId,
            };
        }

        /// <summary>
        /// 删除域名解析
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public override async Task<DomainRecordActionResult> DeleteDomainRecord(DeleteDomainRecordParam param)
        {
            var response = await _client.DeleteRecord(new DeleteRecordRequest()
            {
                Domain = param.Domain,
                RecordId = ulong.Parse(param.RecordId),
            });
            return new DomainRecordActionResult()
            {
                RequestId = response.RequestId,
                RecordId = param.RecordId,
                RR = param.RR,
                TotalCount = 1,
            };
        }

        /// <summary>
        /// 获取域名全部解析记录
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        public override async Task<List<DomainRecord>> DescribeSubDomainRecords(string RR, string domain)
        {
            try
            {
                ulong limit = 10;
                ulong offset = 0;
                ulong totalCount = 0;
                List<DomainRecord> result = new List<DomainRecord>();
                do
                {
                    var response = await _client.DescribeRecordList(new DescribeRecordListRequest()
                    {
                        Domain = domain,
                        Subdomain = RR,
                        Limit = limit,
                        Offset = offset,
                    });
                    if (response == null || response.RecordList == null)
                    {
                        throw new Exception("Describe record list failed, result is null.");
                    }
                    totalCount = response.RecordCountInfo.TotalCount ?? 0;
                    foreach (var item in response.RecordList)
                    {
                        result.Add(new DomainRecord()
                        {
                            RR = RR,
                            Status = item.Status.Equals("ENABLE", StringComparison.OrdinalIgnoreCase) ? DomainRecordStatus.Enabled : DomainRecordStatus.Disabled,
                            RecordId = item.RecordId.ToString(),
                            Domain = domain,
                            TTL = (uint)item.TTL,
                            Line = item.Line,
                            Type = item.Type,
                            Value = item.Value,
                            Locked = false
                        });
                        offset++;
                    }
                } while (totalCount != 0 && result.Count != (int)totalCount);
                return result;
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException.Message.Contains("ResourceNotFound.NoDataOfRecord", StringComparison.OrdinalIgnoreCase))
                {
                    return new List<DomainRecord>();
                }
                throw;
            }
        }

        /// <summary>
        /// 更新解析记录
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public override async Task<DomainRecordActionResult> UpdateDomainRecord(UpdateDomainRecordParam param)
        {
            var response = await _client.ModifyRecord(new ModifyRecordRequest()
            {
                Domain = param.Domain,
                RecordId = ulong.Parse(param.RecordId),
                SubDomain = param.RR,
                RecordType = param.Type.ToString(),
                Value = param.Value,
                TTL = param.TTL,
                RecordLine = "默认"
            });
            return new DomainRecordActionResult()
            {
                Status = !string.IsNullOrWhiteSpace(response.RequestId),
                RequestId = response.RequestId,
                RecordId = response.RecordId.ToString(),
                RR = param.RR,
                TotalCount = 1
            };
        }

        /// <summary>
        /// 判断域名是否存在
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        public override async Task<bool> CreateDomain(string domain)
        {
            var response = await _client.CreateDomain(new CreateDomainRequest()
            {
                Domain = domain,
            });
            return response != null && !string.IsNullOrWhiteSpace(response.RequestId);
        }

        public override async Task<DomainItem> DescribeDomainInfo(string domain)
        {
            try
            {
                var response = await _client.DescribeDomain(new DescribeDomainRequest()
                {
                    Domain = domain,
                });
                if (response == null || response.DomainInfo == null)
                {
                    throw new Exception("Describe domain info is null.");
                }
                return new DomainItem()
                {
                    DomainId = response.DomainInfo.DomainId.ToString(),
                    Domain = response.DomainInfo.Domain,
                    Status = response.DomainInfo.Status.Equals("enable", StringComparison.OrdinalIgnoreCase) ? DomainStatus.Enabled : DomainStatus.Disabled,
                    Remark = response.DomainInfo.Remark,
                    RecordCount = (uint)(response.DomainInfo.RecordCount),
                    CreatedOn = DateTime.TryParse(response.DomainInfo.CreatedOn, out DateTime createTime) ? createTime : DateTime.MinValue,
                    UpdatedOn = DateTime.TryParse(response.DomainInfo.UpdatedOn, out DateTime updateTime) ? updateTime : DateTime.MinValue,
                };
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("InvalidParameterValue.DomainNotExists", StringComparison.OrdinalIgnoreCase))
                {
                    //域名不存在
                    throw new DomainNotExistsException($"domain '{domain}' does not exist", ex);
                }
                throw;
            }
        }

        public override string ToString()
        {
            return "腾讯云DDNS";
        }
    }
}
