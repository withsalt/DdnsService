using System;
using System.Collections.Generic;
using System.Text;

namespace DdnsSDK.Model
{
    public class DomainRecord
    {
        /// <summary>
        /// 
        /// </summary>
        public string DomainName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string RecordId { get; set; }

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
        public int TTL { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Locked { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Weight { get; set; }
    }
}
