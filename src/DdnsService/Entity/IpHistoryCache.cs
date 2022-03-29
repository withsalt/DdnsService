using System;

namespace DdnsService.Entity
{
    public class IpHistoryCache
    {
        /// <summary>
        /// 域名
        /// </summary>
        public string Record { get; set; }

        /// <summary>
        /// 当前的Ip
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        /// 变更次数计数器
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 上次更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; }
    }
}
