using DdnsService.Models;
using System.Collections.Generic;

namespace DdnsService.Configs
{
    public class ApisNode
    {
        /// <summary>
        /// 
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public GetIpApiType Type { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Field { get; set; }

        public static string Position { get { return "Apis"; } }
    }
}
