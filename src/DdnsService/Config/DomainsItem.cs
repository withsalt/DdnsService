using System;
using System.Collections.Generic;
using System.Text;

namespace DdnsService.Config
{
    public class DomainsItem
    {
        /// <summary>
        /// 
        /// </summary>
        public string Provider { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int TTL { get; set; }
    }
}
