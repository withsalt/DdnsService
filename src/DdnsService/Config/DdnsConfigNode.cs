﻿using System;
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

        /// <summary>
        /// 
        /// </summary>
        public List<DdnsServiceProvider> DdnsServiceProviders { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<DomainsItem> Domains { get; set; }
    }
}
