using DdnsService.Models;

namespace DdnsService.Configs
{
    public class ProviderItem
    {
        /// <summary>
        /// 
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DdnsProviderType Type { get; set; }

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
