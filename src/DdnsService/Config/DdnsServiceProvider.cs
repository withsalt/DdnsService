using DdnsService.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace DdnsService.Config
{
    public class DdnsServiceProvider
    {
        /// <summary>
        /// 
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DdnsServiceProviderType Type { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string AccessKey { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string AccessKeySecret { get; set; }
    }
}
