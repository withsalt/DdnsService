using System.Collections.Generic;

namespace DdnsService.Configs
{
    public class DdnsConfigNode
    {
        public bool IsEnableDdns { get; set; }

        public List<ProviderItem> Providers { get; set; }

        public List<DomainItem> Domains { get; set; }

        public static string Position { get { return "DdnsConfig"; } }
    }
}
