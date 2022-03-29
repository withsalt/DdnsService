using System;
using System.Collections.Generic;
using System.Text;

namespace DdnsService.SDKs.Model
{
    public class DomainRecord
    {
        /// <summary>
        /// 
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string RecordId { get; set; }

        public string RequestId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string RR { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Line { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public uint TTL { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DomainRecordStatus Status { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool Locked { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Weight { get; set; }
    }
}
