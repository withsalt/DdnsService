using System;
using System.Collections.Generic;
using System.Text;

namespace DdnsSDK.Model
{
    public class DeleteDomainRecordParam
    {
        public string RecordId { get; set; }

        public string DomainName { get; set; }

        public string RR { get; set; }
    }
}
