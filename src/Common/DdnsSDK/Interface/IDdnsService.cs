using DdnsSDK.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace DdnsSDK.Interface
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
        public abstract List<DomainRecord> DescribeSubDomainRecords(string domain);

        /// <summary>
        /// 根据recordId获取解析的详细信息（可选实现）
        /// </summary>
        /// <param name="recordId"></param>
        /// <returns></returns>
        public virtual DomainRecordInfo DescribeDomainRecordInfo(string recordId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 添加域名解析记录
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public abstract DomainRecordActionResult AddDomainRecord(AddDomainRecordParam param);

        /// <summary>
        /// 更具RecordID删除解析记录
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public abstract DomainRecordActionResult DeleteDomainRecord(DeleteDomainRecordParam param);

        /// <summary>
        /// 根据主解析记录删除相关的解析记录（可选实现）
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public virtual DomainRecordActionResult DeleteSubDomainRecords(DeleteDomainRecordParam param)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 更新域名解析记录
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public abstract DomainRecordActionResult UpdateDomainRecord(UpdateDomainRecordParam param);
    }
}
