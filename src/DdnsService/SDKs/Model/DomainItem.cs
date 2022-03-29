using System;

namespace DdnsService.SDKs.Model
{
    public class DomainItem
    {
        public string DomainId { get; set; }

        public string Domain { get; set; }

        public DomainStatus Status { get; set; }

        public string Remark { get; set; }

        public uint RecordCount { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime UpdatedOn { get; set; }
    }
}
