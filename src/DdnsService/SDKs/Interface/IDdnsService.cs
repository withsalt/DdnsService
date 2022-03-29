using DdnsService.SDKs.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DdnsService.SDKs
{
    public abstract class IDdnsService
    {
        protected string AccessKey { get; set; }

        protected string AccessSecret { get; set; }

        public IDdnsService(string acessKey, string accessSecret)
        {
            if (string.IsNullOrEmpty(acessKey) || string.IsNullOrEmpty(accessSecret))
            {
                throw new Exception("AccessKey or AccessSecret can not null,");
            }
            this.AccessKey = acessKey;
            this.AccessSecret = accessSecret;
        }

        /// <summary>
        /// 获取子域名的全部解析记录
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        public abstract Task<List<DomainRecord>> DescribeSubDomainRecords(string RR, string domain);

        /// <summary>
        /// 根据recordId获取解析的详细信息
        /// </summary>
        /// <param name="recordId"></param>
        /// <returns></returns>
        public abstract Task<DomainRecord> DescribeDomainRecordInfo(string recordId, string domain);

        /// <summary>
        /// 添加域名解析记录
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public abstract Task<DomainRecordActionResult> AddDomainRecord(AddDomainRecordParam param);

        /// <summary>
        /// 更具RecordID删除解析记录
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public abstract Task<DomainRecordActionResult> DeleteDomainRecord(DeleteDomainRecordParam param);

        /// <summary>
        /// 更新域名解析记录
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public abstract Task<DomainRecordActionResult> UpdateDomainRecord(UpdateDomainRecordParam param);

        /// <summary>
        /// 创建域名解析
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        public abstract Task<bool> CreateDomain(string domain);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        public abstract Task<DomainItem> DescribeDomainInfo(string domain);

        /// <summary>
        /// 域名解析是否存在
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        public virtual async Task<bool> DomainIsExist(string domain)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentNullException(nameof(domain));
            }
            try
            {
                var result = await this.DescribeDomainInfo(domain);
                return result != null;
            }
            catch (DomainNotExistsException)
            {
                return false;
            }
        }
    }
}
