using System;
using System.Collections.Generic;
using System.Text;

namespace DdnsService.Configs
{
    public class DomainItem
    {
        /// <summary>
        /// 
        /// </summary>
        public int Provider { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Record { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public uint TTL { get; set; }

        /// <summary>
        /// 域名
        /// </summary>
        public string Domain
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Record))
                {
                    return null;
                }
                return Record.Substring(Record.IndexOf('.') + 1);
            }
        }

        /// <summary>
        /// 记录值
        /// </summary>
        public string RR
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Record))
                {
                    return null;
                }
                return Record.Substring(0, Record.IndexOf('.'));
            }
        }
    }
}
