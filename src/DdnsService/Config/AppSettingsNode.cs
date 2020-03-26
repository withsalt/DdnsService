using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DdnsService.Config
{
    class AppSettingsNode
    {
        /// <summary>
        /// 
        /// </summary>
        public string IsDebug { get; set; }

        public int IntervalTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsEnableMessageNotice { get; set; }

        /// <summary>
        /// 是否开启自动删除历史IP
        /// </summary>
        public bool IsEnableAutoClearIpLog { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public MessageApiConfigNode MessageApiConfig { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsEnableEmailNotice { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public EmailApiConfigNode EmailApiConfig { get; set; }
    }
}
