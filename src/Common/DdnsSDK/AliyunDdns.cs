using Aliyun.Acs.Alidns.Model.V20150109;
using Aliyun.Acs.Core;
using Aliyun.Acs.Core.Exceptions;
using Aliyun.Acs.Core.Profile;
using DdnsSDK.Interface;
using DdnsSDK.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DdnsSDK
{
    public class AliyunDdns : IDdnsService
    {
        readonly DefaultAcsClient client = null;

        public AliyunDdns(string acessKey, string accessSecret) : base(acessKey, accessSecret)
        {
            client = new DefaultAcsClient(DefaultProfile.GetProfile("cn-hangzhou", AccessKey, AccessSecret));
        }

        /// <summary>
        /// 获取子域名的所有解析记录
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        public override List<DomainRecord> DescribeSubDomainRecords(string domain)
        {
            var request = new DescribeSubDomainRecordsRequest()
            {
                SubDomain = domain,
                PageNumber = 1,
                PageSize = 500,
            };
            try
            {
                var response = client.GetAcsResponse(request);
                if (response == null || response.HttpResponse.Content == null || response.HttpResponse.Content.Length == 0)
                {
                    throw new Exception("Describe subDomain records result is null.");
                }
                string result = Encoding.UTF8.GetString(response.HttpResponse.Content);
                JObject jObject = JObject.Parse(result);
                var recordsObj = jObject["DomainRecords"]["Record"];
                if (recordsObj == null)
                {
                    throw new Exception("Can not fins records result from json object.");
                }
                List<DomainRecord> records = recordsObj.ToObject<List<DomainRecord>>();
                return records;
            }
            catch (ServerException e)
            {
                throw new Exception($"Aliyun server error. {e.Message}");
            }
            catch (ClientException e)
            {
                throw new Exception($"Reuqest client error. errcode is {e.ErrorCode}, {e.Message}");
            }
        }

        /// <summary>
        /// 根据recordId获取解析记录的详细信息。
        /// </summary>
        /// <param name="recordId"></param>
        /// <returns></returns>
        public override DomainRecordInfo DescribeDomainRecordInfo(string recordId)
        {
            var request = new DescribeDomainRecordInfoRequest()
            {
                RecordId = recordId
            };
            try
            {
                var response = client.GetAcsResponse(request);
                if (response == null || response.HttpResponse.Content == null || response.HttpResponse.Content.Length == 0)
                {
                    throw new Exception("Describe subDomain records info is null.");
                }
                string result = Encoding.UTF8.GetString(response.HttpResponse.Content);
                return new JsonSerializer().Deserialize<DomainRecordInfo>(new JsonTextReader(new StringReader(result)));
            }
            catch (ServerException e)
            {
                throw new Exception($"Aliyun server error. {e.Message}");
            }
            catch (ClientException e)
            {
                throw new Exception($"Reuqest client error. errcode is {e.ErrorCode}, {e.Message}");
            }
        }

        /// <summary>
        /// 添加解析记录，默认TTL为600
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public override DomainRecordActionResult AddDomainRecord(AddDomainRecordParam param)
        {
            var request = new AddDomainRecordRequest()
            {
                DomainName = param.DomainName,
                RR = param.RR,
                Type = param.Type.ToString(),
                TTL = param.TTL,
                _Value = param.Value
            };
            try
            {
                var response = client.GetAcsResponse(request);
                if (response == null || response.HttpResponse.Content == null || response.HttpResponse.Content.Length == 0)
                {
                    throw new Exception("Add subdomain records info failed.");
                }
                string result = Encoding.UTF8.GetString(response.HttpResponse.Content);
                var resultObj = new JsonSerializer().Deserialize<DomainRecordActionResult>(new JsonTextReader(new StringReader(result)));
                if (resultObj != null)
                    resultObj.Status = true;
                return resultObj;
            }
            catch (ServerException e)
            {
                throw new Exception($"Aliyun server error. {e.Message}");
            }
            catch (ClientException e)
            {
                throw new Exception($"Reuqest client error. errcode is {e.ErrorCode}, {e.Message}");
            }
        }

        public override DomainRecordActionResult DeleteDomainRecord(DeleteDomainRecordParam param)
        {
            var request = new DeleteDomainRecordRequest()
            {
                RecordId = param.RecordId,
            };
            try
            {
                var response = client.GetAcsResponse(request);
                if (response == null || response.HttpResponse.Content == null || response.HttpResponse.Content.Length == 0)
                {
                    throw new Exception($"Delete subdomain record info failed. record id is {param.RecordId}");
                }
                string result = Encoding.UTF8.GetString(response.HttpResponse.Content);
                var resultObj = new JsonSerializer().Deserialize<DomainRecordActionResult>(new JsonTextReader(new StringReader(result)));
                if (resultObj != null)
                    resultObj.Status = true;
                return resultObj;
            }
            catch (ServerException e)
            {
                throw new Exception($"Aliyun server error. {e.Message}");
            }
            catch (ClientException e)
            {
                throw new Exception($"Reuqest client error. errcode is {e.ErrorCode}, {e.Message}");
            }
        }

        public override DomainRecordActionResult DeleteSubDomainRecords(DeleteDomainRecordParam param)
        {
            var request = new DeleteSubDomainRecordsRequest()
            {
                DomainName = param.DomainName,
                RR = param.RR
            };
            try
            {
                var response = client.GetAcsResponse(request);
                if (response == null || response.HttpResponse.Content == null || response.HttpResponse.Content.Length == 0)
                {
                    throw new Exception($"Delete subdomain record info failed. record domain is {param.RR}.{param.DomainName}");
                }
                string result = Encoding.UTF8.GetString(response.HttpResponse.Content);
                var resultObj = new JsonSerializer().Deserialize<DomainRecordActionResult>(new JsonTextReader(new StringReader(result)));
                if (resultObj != null)
                    resultObj.Status = true;
                return resultObj;
            }
            catch (ServerException e)
            {
                throw new Exception($"Aliyun server error. {e.Message}");
            }
            catch (ClientException e)
            {
                throw new Exception($"Reuqest client error. errcode is {e.ErrorCode}, {e.Message}");
            }
        }

        public override DomainRecordActionResult UpdateDomainRecord(UpdateDomainRecordParam param)
        {
            var request = new UpdateDomainRecordRequest()
            {
                RecordId = param.RecordId,
                RR = param.RR,
                Type = param.Type.ToString(),
                _Value = param.Value,
                TTL = param.TTL
            };
            try
            {
                var response = client.GetAcsResponse(request);
                if (response == null || response.HttpResponse.Content == null || response.HttpResponse.Content.Length == 0)
                {
                    throw new Exception($"Update subdomain record info failed. record id is {param.RecordId}, new value is {param.Value}");
                }
                string result = Encoding.UTF8.GetString(response.HttpResponse.Content);
                var resultObj = new JsonSerializer().Deserialize<DomainRecordActionResult>(new JsonTextReader(new StringReader(result)));
                if (resultObj != null)
                    resultObj.Status = true;
                return resultObj;
            }
            catch (ServerException e)
            {
                throw new Exception($"Aliyun server error. {e.Message}");
            }
            catch (ClientException e)
            {
                throw new Exception($"Reuqest client error. errcode is {e.ErrorCode}, {e.Message}");
            }
        }
    }
}
