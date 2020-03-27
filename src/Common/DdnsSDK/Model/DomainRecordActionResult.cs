using System;
using System.Collections.Generic;
using System.Text;

namespace DdnsSDK.Model
{
    public class DomainRecordActionResult
    {
        public bool Status { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string RequestId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string RecordId { get; set; }

        public string RR { get; set; }

        public int TotalCount { get; set; }
    }
}
