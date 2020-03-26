using System;
using System.Collections.Generic;
using System.Text;

namespace DdnsService.Config
{
    public class MessageApiConfigNode
    {
        /// <summary>
        /// 
        /// </summary>
        public string URL { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string AppKey { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// IP地址变更提醒：IP地址已变更，当前IP[{0}]，历史IP[{1}]。【极客物联】
        /// </summary>
        public string MessageTemplate { get; set; }
    }
}
