using System;
using System.Collections.Generic;
using System.Text;

namespace DdnsService.Config
{
    public class DdnsConfigNode
    {
        /// <summary>
        /// 
        /// </summary>
        public bool IsEnableDdns { get; set; }

        public string Domain { get; set; }

        public int TTL { get; set; } = 600;

        /// <summary>
        /// 
        /// </summary>
        public string AccessKeyId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string AccessKeySecret { get; set; }
    }
}
