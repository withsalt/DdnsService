using System;
using System.Collections.Generic;
using System.Text;

namespace DdnsService.Config
{
    public class EmailApiConfigNode
    {
        /// <summary>
        /// 
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Passwd { get; set; }

        /// <summary>
        /// 邮件接收人
        /// </summary>
        public string ReceiveAddress { get; set; }

        public bool UseDefaultCredentials { get; set; }

    }
}
